using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class SecurityGroupModel
    {
        public int GroupId { get; set; }
        [RegularExpression(
@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
ErrorMessage = "Group name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
)]
        [Required(ErrorMessage = "Group Name is required.")]
        [StringLength(50, ErrorMessage = "Group Name cannot be longer than 50 characters.")]
        public string? GroupName { get; set; }
        [RegularExpression(
@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
ErrorMessage = "Group Desc can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
)]
        [StringLength(250, ErrorMessage = "Group Description cannot be longer than 250 characters.")]
        public string? GroupDesc { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }
    public class SecurityGroupUpdateModel
    {
        public int GroupId { get; set; }
        [RegularExpression(
        @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
        ErrorMessage = "Group name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
        )]
        [Required(ErrorMessage = "Group Name is required.")]
        [StringLength(50, ErrorMessage = "Group Name cannot be longer than 50 characters.")]
        public string? GroupName { get; set; }
        [RegularExpression(
        @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
        ErrorMessage = "Group Desc can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
        )]
        [StringLength(250, ErrorMessage = "Group Description cannot be longer than 250 characters.")]
        public string? GroupDesc { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
        [JsonIgnore]

        public string? CreatedBy { get; set; }
        [JsonIgnore]

        public DateTime CreatedByDateTime { get; set; }
        [JsonIgnore]

        public string? UpdatedBy { get; set; }
    }

}
