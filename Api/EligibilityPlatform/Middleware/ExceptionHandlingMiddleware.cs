using MEligibilityPlatform.Domain.Models;
using MEligibilityPlatform.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
namespace MEligibilityPlatform.Middleware
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="logger">The logger used to log exceptions.</param>
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        /// <summary>
        /// Invokes the middleware and handles exceptions that occur in the request pipeline.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Proceed to the next middleware
            }
            catch (MakerCheckerException ex)
            {
                _logger.LogError(ex, "MakerCheckerException occurred: {ErrorMessage}", ex.Message);

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    isSuccess = true,
                    message = ex.Message
                });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {ErrorMessage}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles exceptions and writes a standardized JSON response to the HTTP response.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            //context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError,
            };
            //var type = exception.GetType();

            if (exception.GetType() == typeof(DbUpdateException) && exception.InnerException != null && !string.IsNullOrEmpty(exception.InnerException.Message) && exception.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE"))
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsJsonAsync(new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Could not delete given records because there are depenent values."
                });
            }
            else if (exception.GetType() == typeof(DbUpdateConcurrencyException) && !string.IsNullOrEmpty(exception.Message) && exception.Message.Contains("The database operation was expected to affect 1 row(s), but actually affected 0 row(s)"))
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsJsonAsync(new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Record Not found."
                });
            }
            else if (!string.IsNullOrEmpty(exception.Message) && exception.Message.Contains("Modification has been successfully stored in MakerChecker."))
            {
                context.Response.StatusCode = 200;
                return context.Response.WriteAsJsonAsync(new ResponseModel
                {
                    IsSuccess = true,
                    Message = exception.Message
                });
            }
            else
            {
                context.Response.StatusCode = 500;
                return context.Response.WriteAsJsonAsync(new ResponseModel
                {
                    IsSuccess = false,
                    Message = exception.Message
                });
            }

        }
    }
}
