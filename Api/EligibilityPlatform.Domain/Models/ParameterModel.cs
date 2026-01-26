using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class ParameterModel
    {
        [Key]
        public int ParameterId { get; set; }

        [Required(ErrorMessage = "Parameter name is required.")]
        [StringLength(50, ErrorMessage = "Parameter name cannot exceed 50 characters.")]
        [RegularExpression(
       @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
       ErrorMessage = "ParameterName allows only letters, numbers, and underscore"
   )]
        public string? ParameterName { get; set; }

        [Required]
        public bool HasFactors { get; set; }

        [Required(ErrorMessage = "Identifier is required.")]
        public int? Identifier { get; set; }
        [Required]
        public string RejectionReason { get; set; }
        [Required]
        public string RejectionReasonCode { get; set; }
        [Required]
        public bool IsKyc { get; set; }

        public bool IsRequired { get; set; }

        [JsonIgnore]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "DataTypeId is required.")]
        public int? DataTypeId { get; set; }

        public int? ConditionId { get; set; }

        [StringLength(3, ErrorMessage = "FactorOrder cannot exceed 3 characters.")]
        public string? FactorOrder { get; set; }

        [MaxLength(1000, ErrorMessage = "ValueSource cannot exceed 1000 characters.")]
        public string? ValueSource { get; set; }

        [MaxLength(1000, ErrorMessage = "StaticValue cannot exceed 1000 characters.")]
        public string? StaticValue { get; set; }

        public bool IsMandatory { get; set; }

        public List<ParameterComputedValueModel> ComputedValues { get; set; } = [];
    }
    public class ParameterListModel : ParameterModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public new int? TenantId { get; set; }
        public string DataType { get; set; } = string.Empty;

    }
    public class ParameterAddUpdateModel : ParameterModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }

    }
    public class ParameterCsvModel
    {

        public int ParameterId { get; set; }

        public string? ParameterName { get; set; }

        public bool HasFactors { get; set; }

        public int? Identifier { get; set; }

        public bool IsKyc { get; set; }

        public bool IsRequired { get; set; }

        public int TenantId { get; set; }
        public string? EntityName { get; set; }
        public int? DataTypeId { get; set; }
        public string? DataTypeName { get; set; }

        public int? ConditionId { get; set; }
        public string? ConditionValue { get; set; }

        public string? FactorOrder { get; set; }




    }

}
