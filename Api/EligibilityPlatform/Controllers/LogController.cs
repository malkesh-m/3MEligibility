using EligibilityPlatform.Application.Middleware;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.MicrosoftExtensions;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing log operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LogController"/> class.
    /// </remarks>
    /// <param name="logService">The log service.</param>
    /// <param name="logger">The logger instance.</param>
    [Route("api/log")]
    [ApiController]
    public class LogController(ILogService logService, ILogger<LogController> logger) : ControllerBase
    {
        private readonly ILogService _logService = logService;
        private readonly ILogger<LogController> _logger = logger;

        /// <summary>
        /// Retrieves all log records with pagination.
        /// </summary>
        /// <param name="pageIndex">The page index (default is 0).</param>
        /// <param name="pageSize">The page size (default is 10).</param>
        /// <returns>An <see cref="IActionResult"/> containing the log records.</returns>
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageIndex = 0, int pageSize = 10)
        {
            // Retrieves all log records with pagination
            var result = await _logService.GetAll(pageIndex, pageSize);
            // Returns success response with the retrieved log data
            return Ok(new ResponseModel { IsSuccess = true, Data = result });
        }

        /// <summary>
        /// Logs a frontend error.
        /// </summary>
        /// <param name="error">The frontend error log model.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the log status.</returns>
        [HttpPost("frontend-error"), AllowAnonymous]
        public IActionResult LogFrontendError(FrontendErrorLog error)
        {
            // Gets the current user's name
            var UserName = User.GetUserName();
            // Logs the frontend error with detailed information
            //_logger.LogError(
            //    "Frontend exception | User: {User} | Component: {Component} | Path: {Path} | Request: {Request} | Exception: {Exception} | Status: {Status} | UserAgent: {UserAgent}",
            //    UserName,
            //    error.Component,
            //    error.Path,
            //    error.Request,
            //    $"{error.Message} - {error.Stack}",
            //    error.Status,
            //    error.UserAgent
            //);
            _logger.LogError(
    "Frontend Error {@LogDetails}",
    new
    {
        User = UserName,
        error.Component,
        error.Path,
        error.Request,
        Exception = $"{error.Message} - {error.Stack}",
        error.Status,
        error.UserAgent,
    });
            // Returns success response indicating the error was logged
            return Ok(new { Status = "Logged" });
        }

        /// <summary>
        /// Logs a user activity event.
        /// </summary>
        /// <param name="userActivity">The user activity model.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the log status.</returns>
        [HttpPost("loguseractivity"), AllowAnonymous]
        public IActionResult LogUserActivity(UserActivityModel userActivity)
        {
            // Gets the current user's name or defaults to "Anonymous"
            var UserName = User?.Identity?.Name ?? "Annoymouns";
            // Logs the user activity with detailed information
            //_logger.LogInformation(
            //    "User Activity | User: {User} | Component: {Component} | ActionType: {ActionType} | ActionName: {ActionName} | PageUrl: {PageUrl} ",
            //    UserName,
            //    userActivity.ComponentName,
            //    userActivity.ActionType,
            //    userActivity.ActionName,
            //    userActivity.PageUrl
            //);
            _logger.LogInformation(
       "User Activity {@LogDetails}",
       new
       {
           User = UserName,
           Component = userActivity.ComponentName,
           userActivity.ActionType,
           userActivity.ActionName,
           userActivity.PageUrl,
       });
            // Returns success response indicating the activity was logged
            return Ok(new { Status = "Logged" });
        }
    }
}
