using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "UserId is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Current password is required.")]
        public required string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be between 6 and 100 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{6,}$", ErrorMessage = "New password must contain at least one letter and one number.")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm the new password.")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
        public required string ConfirmNewPassword { get; set; }
    }
}
