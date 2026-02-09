using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing dashboard operations and analytics.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </remarks>
    /// <param name="dashboardService">The dashboard service.</param>
    /// 

    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController(IDashboardService dashboardService) : ControllerBase
    {
        /// <summary>
        /// The dashboard service instance for dashboard analytics operations.
        /// </summary>
        private readonly IDashboardService _dashboardService = dashboardService;

        /// <summary>
        /// Retrieves the monthly evaluation summary for the specified year.
        /// </summary>
        /// <param name="year">The year to filter the summary (optional).</param>
        /// <returns>An <see cref="IActionResult"/> containing the monthly summary.</returns>
        /// 
        [Authorize(Policy =Permissions.Dashboard.View)]
        [HttpGet("monthly-summary")]
        public async Task<IActionResult> GetMonthlySummary(int? year = null)
        {
            /// <summary>
            /// Calls the service to retrieve monthly evaluation summary for the current user's entity.
            /// </summary>
            var result = await _dashboardService.GetEvaluationSummaryByMonthAsync(User.GetTenantId(), year);

            /// <summary>
            /// Returns successful response with the monthly summary data.
            /// </summary>
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the breakdown of failure reasons for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the failure reason breakdown.</returns>
        /// 
        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpGet("failure-reasons")]
        public async Task<IActionResult> GetFailureReasonBreakdown()
        {
            /// <summary>
            /// Gets the current user's entity ID.
            /// </summary>
            var tenantId = User.GetTenantId();

            /// <summary>
            /// Calls the service to retrieve failure reason breakdown for the entity.
            /// </summary>
            var data = await _dashboardService.GetFailureReasonBreakdownAsync(tenantId);

            /// <summary>
            /// Returns successful response with the failure reason breakdown data.
            /// </summary>
            return Ok(data);
        }

        /// <summary>
        /// Retrieves the evaluation history based on the provided filter.
        /// </summary>
        /// <param name="filter">The evaluation history filter.</param>
        /// <returns>An <see cref="IActionResult"/> containing the evaluation history.</returns>
        /// 
        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpPost("evaluation-history")]
        public async Task<IActionResult> GetEvaluationHistory([FromBody] EvaluationHistoryFilter filter)
        {
            var (data, totalCount) = await _dashboardService.GetEvaluationHistoryAsync(filter, User.GetTenantId());

            // Return paginated response
            return Ok(new
            {
                data,
                totalCount
            });
        }

        /// <summary>
        /// Retrieves the processing time distribution for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the processing time distribution.</returns>
        /// 
        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpGet("processing-time-distribution")]
        public async Task<IActionResult> GetProcessingTimeDistribution()
        {
            /// <summary>
            /// Calls the service to retrieve processing time distribution for the current user's entity.
            /// </summary>
            var result = await _dashboardService.GetProcessingTimeDistributionAsync(User.GetTenantId());

            /// <summary>
            /// Returns successful response with the processing time distribution data.
            /// </summary>
            return Ok(result);
        }

        //[HttpGet("get-failure-reasons")]


        /// <summary>
        /// Retrieves the number of customers evaluated for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the number of customers evaluated.</returns>
        /// 
        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpGet("customers-evaluated")]
        public async Task<IActionResult> GetCustomersEvaluated()
            /// <summary>
            /// Calls the service to retrieve the number of customers evaluated for the current user's entity and returns the result.
            /// </summary>
            => Ok(await _dashboardService.GetCustomersEvaluatedAsync(User.GetTenantId()));

        /// <summary>
        /// Retrieves the approval rate for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the approval rate.</returns>
        /// 
        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpGet("approval-rate")]
        public async Task<IActionResult> GetApprovalRate()
            /// <summary>
            /// Calls the service to retrieve the approval rate for the current user's entity and returns the result.
            /// </summary>
            => Ok(await _dashboardService.GetApprovalRateAsync(User.GetTenantId()));

        /// <summary>
        /// Retrieves the rejection rate for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the rejection rate.</returns>
        /// 
        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpGet("rejection-rate")]
        public async Task<IActionResult> GetRejectionRate()
            /// <summary>
            /// Calls the service to retrieve the rejection rate for the current user's entity and returns the result.
            /// </summary>
            => Ok(await _dashboardService.GetRejectionRateAsync(User.GetTenantId()));

        /// <summary>
        /// Retrieves the top failure reason for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the top failure reason.</returns>
        /// 
        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpGet("top-failure-reason")]
        public async Task<IActionResult> GetTopFailureReason()
            /// <summary>
            /// Calls the service to retrieve the top failure reason for the current user's entity and returns the result.
            /// </summary>
            => Ok(await _dashboardService.GetTopFailureReasonAsync(User.GetTenantId()));

        /// <summary>
        /// Retrieves the average approved score for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the average approved score.</returns>
        /// 
        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpGet("avg-approved-score")]
        public async Task<IActionResult> GetAvgApprovedScore()
            /// <summary>
            /// Calls the service to retrieve the average approved score for the current user's entity and returns the result.
            /// </summary>
            => Ok(await _dashboardService.GetAverageApprovedScoreAsync(User.GetTenantId()));

        /// <summary>
        /// Retrieves the average processing time for all entities.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the average processing time.</returns>
        /// 
        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpGet("avg-processing-time")]
        public async Task<IActionResult> GetAvgProcessingTime()
            /// <summary>
            /// Calls the service to retrieve the average processing time across all entities and returns the result.
            /// </summary>
            => Ok(await _dashboardService.GetAverageProcessingTimeAsync());

        [Authorize(Policy = Permissions.Dashboard.View)]

        [HttpGet("apievaluationhistory")]
        public async Task<IActionResult> ApiEvaluationHistory(int EvaluationHistoryId)
        {
            /// <summary>
            /// Calls the service to retrieve the top failure reason for the current user's entity and returns the result.
            /// </summary>
            var data = await _dashboardService.ApiEvaluationHistory(EvaluationHistoryId);
            if (data.Count != 0)
            {
                return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Success, Data = data });
            }

            return Ok(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound, Data = null });
        }
    }
}
