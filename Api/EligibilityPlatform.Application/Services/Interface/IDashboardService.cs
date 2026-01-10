using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for dashboard operations.
    /// Provides methods for retrieving dashboard metrics and analytics data.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves evaluation summary data grouped by month for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <param name="year">The optional year filter for the summary data.</param>
        /// <returns>A list of <see cref="PassFailSummaryModel"/> objects containing monthly evaluation summaries.</returns>
        Task<List<PassFailSummaryModel>> GetEvaluationSummaryByMonthAsync(int entityId, int? year = null);

        /// <summary>
        /// Retrieves failure reason breakdown for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="FailureReasonSummaryModel"/> objects containing failure reason statistics.</returns>
        Task<List<FailureReasonSummaryModel>> GetFailureReasonBreakdownAsync(int entityId);

        /// <summary>
        /// Retrieves evaluation history data based on the specified filter for a specific entity.
        /// </summary>
        /// <param name="filter">The <see cref="EvaluationHistoryFilter"/> containing filter criteria.</param>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="EvaluationHistoryModel"/> objects containing evaluation history data.</returns>
        Task<(List<EvaluationHistoryModel> Data, int TotalCount)> GetEvaluationHistoryAsync(EvaluationHistoryFilter filter, int entityId);

        /// <summary>
        /// Retrieves processing time distribution data for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="ProcessingTimeBucketModel"/> objects containing processing time distribution data.</returns>
        Task<List<ProcessingTimeBucketModel>> GetProcessingTimeDistributionAsync(int entityId);
        /// <summary>
        /// Gets the total number of customers evaluated for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>The total number of customers evaluated.</returns>
        Task<int> GetCustomersEvaluatedAsync(int entityId);

        /// <summary>
        /// Gets the approval rate percentage for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>The approval rate as a percentage.</returns>
        Task<double> GetApprovalRateAsync(int entityId);

        /// <summary>
        /// Gets the rejection rate percentage for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>The rejection rate as a percentage.</returns>
        Task<double> GetRejectionRateAsync(int entityId);

        /// <summary>
        /// Gets the average processing time across all evaluations.
        /// </summary>
        /// <returns>The average processing time in the appropriate unit.</returns>
        Task<double> GetAverageProcessingTimeAsync();

        /// <summary>
        /// Gets the top failure reason for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>The description of the top failure reason.</returns>
        Task<string?> GetTopFailureReasonAsync(int entityId);
        Task<List<IntegrationApiEvaluationModel>> ApiEvaluationHistory(int evaluationHistoryId);


        /// <summary>
        /// Gets the average approved score for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>The average score of approved evaluations.</returns>
        Task<double> GetAverageApprovedScoreAsync(int entityId);
    }
}
