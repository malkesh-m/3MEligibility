using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Models
{
    public class ScreenModel
    {
        public int ScreenId { get; set; }
        [RegularExpression(
@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
ErrorMessage = "Screen name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
)]
        [Required(ErrorMessage = "Screen Name is required.")]
        [StringLength(60, ErrorMessage = "Screen Name cannot be longer than 60 characters.")]
        public string? ScreenName { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
    }
}
