using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class SecurityRoleModel
    {
        public int RoleId { get; set; }
        [RegularExpression(
        @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
        ErrorMessage = "Role name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
        )]
        [Required(ErrorMessage = "Role Name is required.")]
        [StringLength(50, ErrorMessage = "Role Name cannot be longer than 50 characters.")]
        public string? RoleName { get; set; }
        [RegularExpression(
        @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
        ErrorMessage = "Role Desc can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
        )]
        [StringLength(250, ErrorMessage = "Role Description cannot be longer than 250 characters.")]
        public string? RoleDesc { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public int UserCount { get; set; }
    }
    public class SecurityRoleUpdateModel
    {
        public int RoleId { get; set; }
        [RegularExpression(
        @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
        ErrorMessage = "Role name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
        )]
        [Required(ErrorMessage = "Role Name is required.")]
        [StringLength(50, ErrorMessage = "Role Name cannot be longer than 50 characters.")]
        public string? RoleName { get; set; }
        [RegularExpression(
        @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
        ErrorMessage = "Role Desc can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
        )]
        [StringLength(250, ErrorMessage = "Role Description cannot be longer than 250 characters.")]
        public string? RoleDesc { get; set; }
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
    }

}
