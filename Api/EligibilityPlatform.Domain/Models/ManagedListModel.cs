using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class ManagedListModel
    {
        public int ListId { get; set; }
        [Required(ErrorMessage = "ListName is required.")]
        [StringLength(50, ErrorMessage = "ListName cannot exceed 50 characters.")]
        [RegularExpression(
    @"^[\u0600-\u06FFa-zA-Z0-9_\-\s,\(\)]+$", ErrorMessage = "ListName contains invalid characters.")]
        public string? ListName { get; set; }


        public int? TenantId { get; set; }

    }
    public class ManagedListGetModel : ManagedListModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }
    public class ManagedListAddUpdateModel : ManagedListModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class ManagedListUpdateModel : ManagedListModel
    {
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class ManagedListModelDescription
    {
        public int ListId { get; set; }

        public string? ListName { get; set; }

        public int TenantId { get; set; }
        public string? EntityName { get; set; }

    }
}
