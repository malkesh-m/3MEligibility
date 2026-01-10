using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Models
{
    public class GroupRoleModel
    {
        [Required(ErrorMessage = "Group ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Group ID must be a positive integer.")]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "At least one Role ID is required.")]
        [MinLength(1, ErrorMessage = "RoleIds list must contain at least one item.")]
        public List<int> RoleIds { get; set; } = [];
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
