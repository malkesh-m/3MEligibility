using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Domain.Entities;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class EvaluationHistoryModel
    {
        public int EvaluationHistoryId { get; set; }
        [StringLength(50, ErrorMessage = "NationalId cannot exceed 50 characters.")]
        public string NationalId { get; set; } = string.Empty;
        [StringLength(50, ErrorMessage = "LoanNo cannot exceed 50 characters.")]
        public string LoanNo { get; set; } = string.Empty;
        public DateTime EvaluationTimeStamp { get; set; }
        [StringLength(1000, ErrorMessage = "Outcome cannot exceed 1000 characters.")]
        public string? OutCome { get; set; }
        [StringLength(1000, ErrorMessage = "FailurReason cannot exceed 1000 characters.")]
        public string? FailurReason { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "CreditScore cannot be negative.")]
        public int CreditScore { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "PreviousApplication cannot be negative.")]
        public int PreviousApplication { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "ProcessingTime cannot be negative.")]
        public double ProcessingTime { get; set; }
        [StringLength(4000, ErrorMessage = "BreRequest cannot exceed 4000 characters.")]
        public string? BreRequest { get; set; }
        [StringLength(4000, ErrorMessage = "BreResponse cannot exceed 4000 characters.")]
        public string? BreResponse { get; set; }
        public int EntityId { get; set; }
    }
    public class IntegrationApiEvaluationModel
    {
        public int Id { get; set; }

        public int? EvaluationHistoryId { get; set; }
        public string? ApiName { get; set; }
        public int NodeApiId { get; set; }
        public string? ApiRequest { get; set; }
        public string? ApiResponse { get; set; }
        public string? BreRequest { get; set; }
        public string? BreResponse { get; set; }

        public DateTime EvaluationTimeStamp { get; set; } = DateTime.Now;
        public List<EligibleProduct>? EligibleProducts { get; set; }
        public List<NonEligibleProduct>? NonEligibleProducts { get; set; }
    }
    public class BreResponse
    {
        public List<EligibleProductRaw>? EligibleProducts { get; set; }
        public List<NonEligibleProductRaw>? NonEligibleProducts { get; set; }
    }

    public class EligibleProductRaw
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal EligibleAmount { get; set; }
    }

    public class NonEligibleProductRaw
    {
        public string ProductName { get; set; } = string.Empty;
        public List<RejectionReason>? RejectionReasons { get; set; }
    }
}
