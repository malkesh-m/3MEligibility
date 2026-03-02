namespace MEligibilityPlatform.Domain.Models
{
    public class ValidationResult
    {
        public bool IsValidationPassed { get; set; }
        public List<ValidationDetail> ValidationDetails { get; set; } = [];
        public double EligibilityPercentage { get; set; }
        public List<string>? ErrorMessage { get; set; }
        //public List<RuleResult> RuleResults { get; set; }
        //public List<ProductEligibilityModel> EligibleProducts { get; set; }
        public List<RejectionReason>? RejectionReason { get; set; }

    }

    public class ValidationDetail
    {
        public int? ProductId { get; set; }
        public bool IsValid { get; set; }
        public string? FactorName { get; set; }
        public string? Condition { get; set; }
        public string? FactorValue { get; set; }
        public string? ProvidedValue { get; set; }
        public List<string>? ErrorMessage { get; set; }
        public int? ParameterId { get; set; }
        public RejectionReason? RejectionReason { get; set; }
    }
    public class RuleResult
    {
        public int RuleID { get; set; }
        public bool IsValid { get; set; }
        public List<ValidationDetail>? ValidationDetails { get; set; }
        public double EligibilityPercentage { get; set; }
        public List<string>? ErrorMessage { get; set; }

    }
    public class EcardResult
    {
        public int EcardID { get; set; }
        public bool Result { get; set; }
    }
    public class PcardResult
    {
        public int PcardID { get; set; }
        public bool Result { get; set; }
    }
    public class RuleEvaluationResult
    {
        public int RuleID { get; set; }


        public bool IsValid { get; set; }

        public List<int> ApplicableProductIds { get; set; } = [];


        public List<string> ErrorMessage { get; set; } = [];


        public Dictionary<string, object> EvaluationDetails { get; set; } = [];
    }
    public class ProductEligibilityResult
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        //public ValidationResult ValidationResult { get; set; } = new ValidationResult();
        //public List<int> ProductIdsWithException { get; set; }
        public decimal EligibleAmount { get; set; } // Add this
        public bool IsProcessedByException { get; set; }
        public bool IsEligible { get; set; } = true; // Default to true
        public decimal EligibilityPercent { get; set; }
        public string? ErrorMessage { get; set; }
        public int Score { get; set; }
        public decimal MaxEligibleAmount { get; set; }

        public string? ExceptionScope { get; set; }
        public string? LimitAmountType { get; set; }
        public decimal LimitAmountPercent { get; set; }
        public string? ProductCapScore { get; set; }
        public decimal? ProductCapPercent { get; set; }
        public int ProbabilityOfDefault { get; set; }
        public bool IsGuaranteed { get; set; } = false;
        public decimal? LimitAmount { get; set; } = 0;
        public string ProductCode { get; set; } = "";
        public List<RejectionReason>? RejectionReason { get; set; }


    }

    public class EligibleAmountResult
    {
        public int Score { get; set; }
        public List<ProductEligibilityResult>? Products { get; set; }
    }
    public class ProductEligibilityResults
    {
        public string? ProductCode { get; set; }
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public bool IsEligible { get; set; }

        //public ValidationResult ValidationResult { get; set; } = new ValidationResult();
        //public List<int> ProductIdsWithException { get; set; }
        public decimal ProductCapAmount { get; set; }
        public int ProbabilityOfDefault { get; set; }
        public decimal MaximumProductCapPercentage { get; set; }
        public string? Message { get; set; }
        public decimal EligibleAmount { get; set; } // Add this

        public decimal? LimitAmount { get; set; } = 0;
        public List<RejectionReason>? RejectionReason { get; set; }



    }
    public class EligibleAmountResults
    {
        public int Score { get; set; }
        public List<ProductEligibilityResults>? Products { get; set; }


    }


}
