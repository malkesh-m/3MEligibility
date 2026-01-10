using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class RoleModel
    {
        public int RoleId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Screen ID must be a positive integer.")]
        public int? ScreenId { get; set; }
        [RegularExpression(
   @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
   ErrorMessage = "Role Action can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
)]
        [Required(ErrorMessage = "Role Action is required.")]
        [StringLength(50, ErrorMessage = "Role Action cannot be longer than 50 characters.")]
        public string? RoleAction { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }
    public class RoleCreateUpdateModel
    {
        public int RoleId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Screen ID must be a positive integer.")]
        public int? ScreenId { get; set; }
        [RegularExpression(
            @"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
            ErrorMessage = "Role Action can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
              )]
        [Required(ErrorMessage = "Role Action is required.")]
        [StringLength(50, ErrorMessage = "Role Action cannot be longer than 50 characters.")]
        public string? RoleAction { get; set; }
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
