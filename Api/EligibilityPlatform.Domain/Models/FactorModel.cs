using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class FactorModel
    {
        public int FactorId { get; set; }
        [Required(ErrorMessage = "FactorName is required.")]
        [StringLength(50, ErrorMessage = "FactorName cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "FactorName contains invalid characters.")]
        public string? FactorName { get; set; }
        [Required(ErrorMessage = "Note is required.")]
        [StringLength(50, ErrorMessage = "Note cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Note contains invalid characters.")]
        public string? Note { get; set; }
        [Required(ErrorMessage = "Value1 is required.")]
        [StringLength(50, ErrorMessage = "Value1 cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s,\(\)]+$", ErrorMessage = "Value1 contains invalid characters.")]

        public string? Value1 { get; set; }

        [StringLength(50, ErrorMessage = "Value2 cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Value2 contains invalid characters.")]
        public string? Value2 { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ParameterId must be greater than 0.")]
        public int ParameterId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "ConditionId must be greater than 0.")]
        public int? ConditionId { get; set; }

        [JsonIgnore]
        public int TenantId { get; set; }

    }
    public class FactorListModel : FactorModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class FactorAddUpdateModel : FactorModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }

    }
    public class FactorModelDescription
    {
        public int FactorId { get; set; }

        public string? FactorName { get; set; }

        public string? Note { get; set; }

        public string? Value1 { get; set; }

        public string? Value2 { get; set; }



        public int? ConditionId { get; set; }

        public int TenantId { get; set; }

        public string? EntityName { get; set; }
        public int? ParameterId { get; set; }
        public string? ParameterName { get; set; }
        public string? ConditionValue { get; set; }
    }

}
