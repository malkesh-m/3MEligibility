using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MEligibilityPlatform.Middleware
{
    /// <summary>
    /// Middleware for handling permission-based authorization and request/response logging.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PermissionBasedAuthorizationMiddleware"/> class.
    /// </remarks>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="logger">The logger instance for logging information and errors.</param>
    /// <param name="serviceScopeFactory">Factory for creating scoped service providers.</param>
    public class PermissionBasedAuthorizationMiddleware(
        RequestDelegate next,
        ILogger<PermissionBasedAuthorizationMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<PermissionBasedAuthorizationMiddleware> _logger = logger;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        /// <summary>
        /// Invokes the middleware logic to check authorization, log requests/responses,
        /// and handle permissions for the current <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var contentType = context.Request.ContentType;

            if (!string.IsNullOrEmpty(contentType) &&
                contentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = 0L;

            var endpoint = context.GetEndpoint();
            context.Request.EnableBuffering();
            string requestBody;

            try
            {
                // Read request body early and handle exceptions
                requestBody = await ReadRequestBodyAsync(context.Request);
            }
            catch (IOException ex) when (ex.Message.Contains("reset", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var routeData = context.GetRouteData();
            var controllerName = routeData.Values["controller"]?.ToString();
            var actionName = routeData.Values["action"]?.ToString();
            var user = context.User;
            var userName = user?.Identity?.IsAuthenticated == true
                ? user?.FindFirst("preferred_username")?.Value
                : "Anonymous";

            try
            {
                var allowAnonymousAttribute = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();
                if (allowAnonymousAttribute != null)
                {
                    await _next(context);
                    elapsedMs = stopwatch.ElapsedMilliseconds;
                    await LogRequestAndResponse(context, requestBody, userName ?? "Anonymous", controllerName ?? "", actionName ?? "", elapsedMs);
                    await RestoreResponseStream(context, responseBody, originalBodyStream);
                    return;
                }

                var authorizeAttribute = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();
                if (authorizeAttribute != null)
                {
                    // Create scoped service provider to check permissions
                    using var scope = _serviceScopeFactory.CreateScope();
                    //var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    //var hasPermission = await userService.UserHasPermission(user!, controllerName ?? "", actionName ?? "");

                    //if (!hasPermission)
                    //{
                    //    _logger.LogWarning(
                    //        "User {User} does NOT have permission for {Controller}/{Action} (403)",
                    //        userName, controllerName, actionName
                    //    );

                    //}

                    // Continue the request normally
                    await _next(context);
                    elapsedMs = stopwatch.ElapsedMilliseconds;
                    await LogRequestAndResponse(context, requestBody, userName ?? "Anonymous", controllerName ?? "", actionName ?? "", elapsedMs);
                    await RestoreResponseStream(context, responseBody, originalBodyStream);
                    return;
                }

                // If no Authorize attribute, proceed normally
                await _next(context);
                elapsedMs = stopwatch.ElapsedMilliseconds;
                await LogRequestAndResponse(context, requestBody, userName ?? "Anonymous", controllerName ?? "", actionName ?? "", elapsedMs);
                await RestoreResponseStream(context, responseBody, originalBodyStream);
            }
            catch (Exception ex)
            {
                var statusCode = context.Response.StatusCode == 200 ? StatusCodes.Status500InternalServerError : context.Response.StatusCode;
                _logger.LogError(
               ex,
                    "Backend Exception {@LogDetails}",
                    new
                    {
                        User = userName ?? "Anonymous",
                        Controller = controllerName,
                        Action = actionName,
                        context.Request.Path,
                        Request = string.IsNullOrEmpty(requestBody) ? "" : requestBody,
                        Exception = new { ex.Message, ex.StackTrace, ex.InnerException },
                        Status = statusCode
                    }
                );
                context.Response.Body = originalBodyStream;
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        /// <summary>
        /// Reads the body of the incoming request as a string.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The request body as a string.</returns>
        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength == 0)
                return string.Empty;
            // Resets request body position
            request.Body.Position = 0;
            // Creates stream reader for request body
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            // Reads request body content
            var body = await reader.ReadToEndAsync();
            // Resets request body position
            request.Body.Position = 0;
            // Returns request body content
            return body;
        }

        /// <summary>
        /// Reads the body of the outgoing response as a string.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <returns>The response body as a string.</returns>
        private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
        {
            // Resets response body position
            response.Body.Seek(0, SeekOrigin.Begin);
            // Creates stream reader for response body
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            // Resets response body position
            response.Body.Seek(0, SeekOrigin.Begin);
            // Returns response body content
            return text;
        }

        /// <summary>
        /// Logs the request and response details including user, controller, action, request body, response body, and status code.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="requestBody">The request body content.</param>
        /// <param name="userId">The ID or name of the user making the request.</param>
        /// <param name="controller">The controller name from route data.</param>
        /// <param name="action">The action name from route data.</param>
        private async Task LogRequestAndResponse(HttpContext context, string requestBody, string userId, string controller, string action, long executionTimeMs)
        {
            // Trims controller name
            controller = controller.Trim();

            // Skips logging for Log and Audit controllers
            if (controller != "Log" && controller != "Audit")
            {
                // Reads response body content
                var responseBody = await ReadResponseBodyAsync(context.Response);
                // Gets response status code
                var statusCode = context.Response.StatusCode;

                // Creates log message with request/response details

                //_logger.LogInformation(
                //    "| User: {User} | Controller: {Controller} | Action: {Action} | Path: {Path} | Request: {Request} | Response: {Response} | Status: {Status}",
                //    userId,
                //    controller,
                //    action,
                //    context.Request.Path,
                //    requestBody,
                //    responseBody,
                //    statusCode
                //);
                //if (controller = null)
                //{
                //}
                //var logMessage = $"API Call | User: {userId} | Controller: {controller} | Action: {action}  " +
                //                 $"Request: {requestBody}  Response: {responseBody} | Status: {statusCode}";
                //// Logs information message
                //_logger.LogInformation(logMessage);
                //            _logger.LogInformation(
                //      "API Call | User: {UserId} | Controller: {Controller} | Action: {Action} | Request: {RequestBody} | Response: {ResponseBody} | Status: {StatusCode}",
                //userId?? "Annoymouns", controller, action, requestBody, responseBody, statusCode);

                _logger.LogInformation(
                    "API Call {@LogDetails}",
                    new
                    {
                        User = string.IsNullOrEmpty(userId) ? "Anonymous" : userId,
                        Controller = controller,
                        Action = action,
                        Request = string.IsNullOrEmpty(requestBody) ? "" : requestBody,
                        Response = string.IsNullOrEmpty(responseBody) ? "" : responseBody,
                        Status = statusCode,
                        ExecutionTimeMs = executionTimeMs
                    });
                 }

        }

        /// <summary>
        /// Restores the original response stream after reading and logging the response body.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="responseBody">The memory stream containing the response body.</param>
        /// <param name="originalBodyStream">The original response stream.</param>
        private static async Task RestoreResponseStream(HttpContext context, MemoryStream responseBody, Stream originalBodyStream)
        {
            // Resets memory stream position
            responseBody.Seek(0, SeekOrigin.Begin);
            // Copies memory stream content to original stream
            await responseBody.CopyToAsync(originalBodyStream);
            // Flushes the original stream
            await originalBodyStream.FlushAsync();
            // Restores original response stream
            context.Response.Body = originalBodyStream;
        }
    }
    
}
