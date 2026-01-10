using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "LoginId is required.")]
        public required string LoginId { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public required string Password { get; set; }
    }
}
