using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class ListItemModel
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "ItemName is required.")]
        [StringLength(50, ErrorMessage = "ItemName cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "ItemName contains invalid characters.")]
        public string? ItemName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "ListId must be greater than 0.")]
        public int? ListId { get; set; }
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Code contains invalid characters.")]
        public string Code { get; set; } = string.Empty;

        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public int TenantId { get; set; }

        public bool IsImport { get; set; }
    }
    public class ListItemCreateUpdateModel
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "ItemName is required.")]
        [StringLength(50, ErrorMessage = "ItemName cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "ItemName contains invalid characters.")]
        public string? ItemName { get; set; }
        public string? Code { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "ListId must be greater than 0.")]
        public int? ListId { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
        [JsonIgnore]

        public string? CreatedBy { get; set; }
        [JsonIgnore]

        public DateTime CreatedByDateTime { get; set; }
        [JsonIgnore]

        public string? UpdatedBy { get; set; }

        [JsonIgnore]
        public int TenantId { get; set; }
        public bool IsImport { get; set; }
    }
    public class ListItemModelDescription
    {
        public int ItemId { get; set; }

        public string? ItemName { get; set; }

        public int? ListId { get; set; }
        public required string ListName { get; set; }
        public string? Code { get; set; }

    }
}
