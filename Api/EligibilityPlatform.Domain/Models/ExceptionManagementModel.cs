using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Domain.Models
{
    public class ExceptionManagementModel
    {
        public int ExceptionManagementId { get; set; }
        [Required]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
            ErrorMessage = "Exception Name contains invalid characters")]
        public required string ExceptionName { get; set; }

        public bool IsTemporary { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0, 100, ErrorMessage = "Fixed Percentage must be between 0 and 100")]
        public decimal FixedPercentage { get; set; }

        [Range(0, 100, ErrorMessage = "Variation Percentage must be between 0 and 100")]
        public decimal VariationPercentage { get; set; }

        [Required(ErrorMessage = "Scope is required")]
        [StringLength(500, ErrorMessage = "Scope cannot exceed 500 characters")]
        public required string Scope { get; set; }

        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
    ErrorMessage = "Description allows only letters, numbers, and underscore")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Expression is required")]
        [StringLength(2000, ErrorMessage = "Expression cannot exceed 2000 characters")]
        public required string Expression { get; set; }

        [StringLength(2000, ErrorMessage = "ExpShown cannot exceed 2000 characters")]
        public string? ExpShown { get; set; }

        [JsonIgnore]
        public int TenantId { get; set; }

        [StringLength(100, ErrorMessage = "AmountType cannot exceed 100 characters")]
        public string? AmountType { get; set; }



    }

    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        // Add only fields you need — no navigation properties
    }
    public class ExceptionManagementGetModel : ExceptionManagementModel
    {
        //public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        //public string? UpdatedBy { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
        public List<int>? ProductId { get; set; }
        //public List<ProductDto> Products { get; set; }
    }

    public class ExceptionManagementListModel : ExceptionManagementModel
    {
        //public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        //public string? UpdatedBy { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class ExceptionManagementCreateOrUpdateModel : ExceptionManagementModel
    {
        public List<int>? ProductId { get; set; }
        [JsonIgnore]
        public string? CreatedBy { get; set; }

        //public List<int>? ProductId { get; set; }
    }
}
