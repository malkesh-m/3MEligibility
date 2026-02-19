using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class UserRoleModel
    {
        [Required(ErrorMessage = "User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive integer.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Role ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Role ID must be a positive integer.")]
        public int RoleId { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class UserRoleCreateUpdateModel
    {
        [Required(ErrorMessage = "User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive integer.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Role ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Role ID must be a positive integer.")]
        public int RoleId { get; set; }
        [JsonIgnore]
        public int TenantId { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
        [JsonIgnore]

        public string? CreatedBy { get; set; }
        [JsonIgnore]

        public DateTime CreatedByDateTime { get; set; }
        [JsonIgnore]

        public string? UpdatedBy { get; set; }
    }

    public class AssignedAndUnAssignedPermissionModel
    {
        public int PermissionId { get; set; }
        public int RoleId { get; set; }
        public required string PermissionAction { get; set; }
        public bool IsAssigned { get; set; }
    }

    public class AssignedPermissionModel
    {
        public int PermissionId { get; set; }
        public int RoleId { get; set; }
        public required string PermissionAction { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
    }

    public class UnAssignedPermissionModel
    {
        public int PermissionId { get; set; }
        public int RoleId { get; set; }
        public required string PermissionAction { get; set; }
    }
}
