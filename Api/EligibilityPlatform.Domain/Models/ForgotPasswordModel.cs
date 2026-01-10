using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "ResetLink is required.")]

        public required string ResetLink { get; set; }
    }
}
