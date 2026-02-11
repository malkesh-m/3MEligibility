using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing dashboard operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DashboardService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    public class DashboardService(IUnitOfWork uow, IMapper mapper) : IDashboardService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Gets the evaluation summary by month for a given entity and year.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="year">The year to filter by (optional).</param>
        /// <returns>A list of <see cref="PassFailSummaryModel"/> representing the summary.</returns>
        public async Task<List<PassFailSummaryModel>> GetEvaluationSummaryByMonthAsync(int tenantId, int? year = null)
        {
            // Uses current year if no year is specified
            int selectedYear = year ?? DateTime.Now.Year;

            // Queries evaluation history for the specified entity and year
            var data = await _uow.EvaluationHistoryRepository.Query()
                .Where(e => e.TenantId == tenantId && e.EvaluationTimeStamp.Year == selectedYear)
                // Groups by year and month
                .GroupBy(e => new { e.EvaluationTimeStamp.Year, e.EvaluationTimeStamp.Month })
                // Selects counts for approved and rejected outcomes
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    ApprovedCount = g.Count(e => e.Outcome!.StartsWith("Approved")),
                    RejectedCount = g.Count(e => e.Outcome!.StartsWith("Rejected"))
                })
                // Executes the query and gets results as list
                .ToListAsync();

            // Processes the data in memory to format the results
            var result = data
                .Select(g => new PassFailSummaryModel
                {
                    // Converts month number to month name
                    Month = new DateTime(g.Year, g.Month, 1).ToString("MMMM"),
                    ApprovedCount = g.ApprovedCount,
                    RejectedCount = g.RejectedCount
                })
                // Orders by month number
                .OrderBy(x => DateTime.ParseExact(x.Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month)
                .ToList();

            return result;
        }

        /// <summary>
        /// Gets the breakdown of failure reasons for a given entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of <see cref="FailureReasonSummaryModel"/> representing the breakdown.</returns>
        public async Task<List<FailureReasonSummaryModel>> GetFailureReasonBreakdownAsync(int tenantId)
        {
            var rejectedEvaluations = await _uow.EvaluationHistoryRepository.Query()
                .Where(e => e.Outcome != null &&
                            e.Outcome.Contains("Rejected") &&
                            e.TenantId == tenantId)
                .ToListAsync();

            int customerSalaryCount = 0;
            int scoreCount = 0;
            int multipleDefaultsCount = 0;

            foreach (var eval in rejectedEvaluations)
            {
                if (string.IsNullOrWhiteSpace(eval.FailurReason)) continue;

                var reasons = eval.FailurReason
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToArray();

                if (reasons.Length > 1)
                {
                    multipleDefaultsCount++;
                }
                else if (reasons.Length == 1)
                {
                    var reason = reasons[0];

                    if (reason.Contains("Customer Salary", StringComparison.OrdinalIgnoreCase))
                        customerSalaryCount++;

                    else if (reason.Contains("Score", StringComparison.OrdinalIgnoreCase))
                        scoreCount++;
                }
            }

            return
    [
        new() { Reason = "Customer Salary", Count = customerSalaryCount },
        new() { Reason = "Score", Count = scoreCount },
        new() { Reason = "Multiple Defaults", Count = multipleDefaultsCount }
    ];
        }



        /// <summary>
        /// Gets the evaluation history for a given entity and filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of <see cref="EvaluationHistoryModel"/> representing the history.</returns>
        public async Task<(List<EvaluationHistoryModel> Data, int TotalCount)> GetEvaluationHistoryAsync(EvaluationHistoryFilter filter, int tenantId)
        {
            filter ??= new EvaluationHistoryFilter();

            var query = _uow.EvaluationHistoryRepository.Query();
            query = query.Where(e => e.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim();
                query = query.Where(e =>
                    e.NationalId!.ToString()!.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(filter.Decision))
            {
                var decision = filter.Decision.Trim();
                query = query.Where(e => e.Outcome!.Contains(decision));
            }

            if (!string.IsNullOrWhiteSpace(filter.FailureReason))
            {
                var reason = filter.FailureReason.Trim();
                query = query.Where(e => e.FailurReason == reason);
            }

            if (filter.FromDate.HasValue)
                query = query.Where(e => e.EvaluationTimeStamp >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(e => e.EvaluationTimeStamp <= filter.ToDate.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Pagination
            var data = await query
                .OrderByDescending(e => e.EvaluationTimeStamp)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(e => new EvaluationHistoryModel
                {
                    EvaluationHistoryId = e.EvaluationHistoryId,
                    NationalId = e.NationalId,
                    LoanNo = e.LoanNo,
                    EvaluationTimeStamp = e.EvaluationTimeStamp,
                    Outcome = e.Outcome,
                    FailurReason = e.FailurReason,
                    CreditScore = e.CreditScore,
                    PreviousApplication = e.PreviousApplication ?? 0
                }).ToListAsync();

            return (data, totalCount);
        }

        /// <summary>
        /// Gets the processing time distribution for a given entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of <see cref="ProcessingTimeBucketModel"/> representing the distribution.</returns>
        public async Task<List<ProcessingTimeBucketModel>> GetProcessingTimeDistributionAsync(int tenantId)
        {
            // Gets all processing times for the entity
            var data = await _uow.EvaluationHistoryRepository.Query()
                .Where(e => e.TenantId == tenantId)
                .Select(e => e.ProcessingTime)
                .ToListAsync();

            // Creates buckets for different time ranges
            var result = new List<ProcessingTimeBucketModel>
            {
                new() { Range = "0-2s", Count = data.Count(x => x >= 0 && x < 2) },
                new() { Range = "2-5s", Count = data.Count(x => x >= 2 && x < 5) },
                new() { Range = "5-10s", Count = data.Count(x => x >= 5 && x < 10) },
                new() { Range = "10-20s", Count = data.Count(x => x >= 10 && x < 20) },
                new() { Range = "20s+", Count = data.Count(x => x >= 20) }
            };

            return result;
        }

        /// <summary>
        /// Gets the number of customers evaluated for a given entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>The number of customers evaluated.</returns>
        public async Task<int> GetCustomersEvaluatedAsync(int tenantId)
        {
            // Counts all evaluation records for the entity
            return await _uow.EvaluationHistoryRepository.Query()
                .Where(x => x.TenantId == tenantId)
                .CountAsync();
        }

        /// <summary>
        /// Gets the approval rate for a given entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>The approval rate as a percentage.</returns>
        public async Task<double> GetApprovalRateAsync(int tenantId)
        {
            // Base query for the entity
            var query = _uow.EvaluationHistoryRepository.Query()
                .Where(x => x.TenantId == tenantId);

            // Gets total number of evaluations
            var total = await query.CountAsync();
            // Returns 0 if no evaluations
            if (total == 0) return 0;

            // Counts approved evaluations
            var approved = await query
                .CountAsync(x => x.Outcome!.StartsWith("Approved"));

            // Calculates and returns approval percentage
            return Math.Round((double)approved / total * 100, 2);
        }

        /// <summary>
        /// Gets the rejection rate for a given entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>The rejection rate as a percentage.</returns>
        public async Task<double> GetRejectionRateAsync(int tenantId)
        {
            // Base query for the entity
            var query = _uow.EvaluationHistoryRepository.Query()
                .Where(x => x.TenantId == tenantId);

            // Gets total number of evaluations
            var total = await query.CountAsync();
            // Returns 0 if no evaluations
            if (total == 0) return 0;

            // Counts rejected evaluations
            var rejected = await query
                .CountAsync(x => x.Outcome!.StartsWith("Rejected"));

            // Calculates and returns rejection percentage
            return Math.Round((double)rejected / total * 100, 2);
        }

        /// <summary>
        /// Gets the top failure reason for a given entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>The top failure reason as a string.</returns>
        public async Task<string?> GetTopFailureReasonAsync(int tenantId)
        {
            // Fetch all failure reason strings
            var allFailureText = await _uow.EvaluationHistoryRepository.Query()
                .Where(x => x.TenantId == tenantId && !string.IsNullOrEmpty(x.FailurReason))
                .Select(x => x.FailurReason)
                .ToListAsync();

            if (allFailureText.Count == 0)
                return null;

            // Split all reasons by comma, trim, and flatten into list
            var allReasons = allFailureText
                .SelectMany(r => r!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            if (allReasons.Count == 0)
                return null;

            // Get top occurring reason
            var topReason = allReasons
                .GroupBy(r => r)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .FirstOrDefault();

            return topReason;
        }


        /// <summary>
        /// Gets the average approved score for a given entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>The average approved score.</returns>
        public async Task<double> GetAverageApprovedScoreAsync(int tenantId)
        {
            // Gets credit scores for approved evaluations
            //var scores = await _uow.EvaluationHistoryRepository.Query()
            //    .Where(x => x.TenantId == tenantId && x.OutCome!.StartsWith("Approved") && x.CreditScore != null)
            //    .Select(x => x.CreditScore)
            //    .ToListAsync();
            var scores = await _uow.EvaluationHistoryRepository.Query()
              .Where(x => x.TenantId == tenantId && x.Outcome!.StartsWith("Approved"))
              .Select(x => x.CreditScore)
              .ToListAsync();

            // Returns average score or 0 if no scores
            return scores.Count == 0 ? 0 : Math.Round(scores.Average(), 0);
        }

        /// <summary>
        /// Gets the average processing time for all evaluations.
        /// </summary>
        /// <returns>The average processing time.</returns>
        public async Task<double> GetAverageProcessingTimeAsync()
        {
            // Gets all processing times
            var times = await _uow.EvaluationHistoryRepository.Query()
                          .Select(x => x.ProcessingTime)
                          .ToListAsync();

            // Returns average time or 0 if no times
            return times.Count == 0 ? 0 : Math.Round(times.Average(), 1);
        }

        public async Task<List<IntegrationApiEvaluationModel>> ApiEvaluationHistory(int evaluationHistoryId)
        {
            var result = await _uow.IntegrationApiEvaluationRepository.Query()
                .Where(x => x.EvaluationHistoryId == evaluationHistoryId)
                .Include(x => x.NodeApi) // Ensure related entity is loaded
                .Select(x => new IntegrationApiEvaluationModel
                {
                    Id = x.Id,
                    EvaluationHistoryId = x.EvaluationHistoryId,
                    NodeApiId = x.NodeApiId,
                    ApiName = x.NodeApi != null ? x.NodeApi.Apiname : string.Empty,
                    ApiRequest = x.ApiRequest ?? string.Empty,
                    ApiResponse = x.ApiResponse ?? string.Empty,
                    EvaluationTimeStamp = x.EvaluationTimeStamp
                })
                .ToListAsync();

            return result ?? [];
        }

    }
}
