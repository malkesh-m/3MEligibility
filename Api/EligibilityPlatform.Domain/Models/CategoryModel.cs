using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Category name contains invalid characters.")]
        public string? CategoryName { get; set; }
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Description contains invalid characters.")]
        public string? CatDescription { get; set; } // Nullable
        [JsonIgnore]
        public int TenantId { get; set; }

    }
    public class CategoryListModel : CategoryModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public new int? TenantId { get; set; }
    }
    public class CategoryCreateUpdateModel : CategoryModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class CategoryUpdateModel : CategoryModel
    {
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class CategoryDescription
    {


        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public string? CatDescription { get; set; }

        public int EntityId { get; set; }

    }
    public class CategoryCsvModel
    {


        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public string? CatDescription { get; set; }

        public int TenantId { get; set; }
        public string? EntityName { get; set; }

    }

}
