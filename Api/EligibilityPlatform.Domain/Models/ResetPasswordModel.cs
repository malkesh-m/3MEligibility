using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Models
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        [StringLength(100, ErrorMessage = "New password cannot exceed 100 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
