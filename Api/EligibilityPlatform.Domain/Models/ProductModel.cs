using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(50, ErrorMessage = "Product Name cannot exceed 50 characters.")]
        [RegularExpression(
    @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
    ErrorMessage = "Product name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
)]
        public string? ProductName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0.")]
        public int? CategoryId { get; set; }

        // CategoryName: optional string, max length 50 characters
        [StringLength(50, ErrorMessage = "CategoryName cannot exceed 50 characters.")]
        public string? CategoryName { get; set; }

        [JsonIgnore]
        public int TenantId { get; set; }

        [StringLength(10, ErrorMessage = "Code cannot exceed 10 characters.")]
        public string? Code { get; set; }

        public byte[]? ProductImage { get; set; }

        public string? Narrative { get; set; }
        [RegularExpression(
   @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
   ErrorMessage = "Description can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
)]
        [StringLength(50, ErrorMessage = "Description cannot exceed 50 characters.")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "MimeType cannot exceed 20 characters.")]
        public string? MimeType { get; set; }

        public int? ExceptionId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MaxEligibleAmount must be non-negative.")]
        public decimal? MaxEligibleAmount { get; set; }
    }
    public class ProductListModel : ProductModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public new int TenantId { get; set; }

    }
    public class ProductAddUpdateModel : ProductModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class ProductDescription
    {
        public string? Code { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int TenantId { get; set; }
        public string? EntityName { get; set; }
        public byte[]? ProductImage { get; set; }
        public string? Narrative { get; set; }
        public string? Description { get; set; }
        public string? MimeType { get; set; }
    }
    public class ProductIdAndNameModel
    {
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public byte[]? ProductImage { get; set; }

    }
    public class ProductEligibleModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal MaxEligibleAmount { get; set; }
        public string? ProductCode { get; set; }


    }
}
