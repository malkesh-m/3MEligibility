using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Domain.Models
{
    public class PassFailSummaryModel
    {
        [Required]
        [StringLength(50)]
        public required string Month { get; set; }

        [Range(0, int.MaxValue)]
        public int ApprovedCount { get; set; }

        [Range(0, int.MaxValue)]
        public int RejectedCount { get; set; }
    }

    public class FailureReasonSummaryModel
    {
        [Required]
        [StringLength(100)]
        public required string Reason { get; set; }

        [Range(0, int.MaxValue)]
        public int Count { get; set; }
    }
    public class EvaluationHistoryFilter
    {
        [StringLength(200)]
        public string? SearchText { get; set; }

        [StringLength(50)]
        public string? Decision { get; set; }

        [StringLength(100)]
        public string? FailureReason { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 1000)]
        public int PageSize { get; set; } = 10;
    }
    public class ProcessingTimeBucketModel
    {
        [Required]
        [StringLength(50)]
        public required string Range { get; set; }

        [Range(0, int.MaxValue)]
        public int Count { get; set; }
    }
}
