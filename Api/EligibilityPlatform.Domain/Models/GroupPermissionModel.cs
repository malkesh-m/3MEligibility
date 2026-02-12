using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class GroupPermissionModel
    {
        [Required(ErrorMessage = "Group ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Group ID must be a positive integer.")]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "At least one Permission ID is required.")]
        [MinLength(1, ErrorMessage = "PermissionIds list must contain at least one item.")]
        public List<int> PermissionIds { get; set; } = [];
        [JsonIgnore]
        public int TenantId { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

