using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Models
{
    public class SoapApiModel
    {
        [Required(ErrorMessage = "URL is required.")]
        [StringLength(2000, ErrorMessage = "URL cannot exceed 2000 characters.")]
        [RegularExpression(@"^(https?:\/\/)?([\w\-])+(\.[\w\-]+)+[/#?]?.*$",
               ErrorMessage = "URL is not in a valid format.")]
        public string Url { get; set; } = string.Empty;
        [Required(ErrorMessage = "Action is required.")]
        [StringLength(200, ErrorMessage = "Action cannot exceed 200 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-.,()]+$",
               ErrorMessage = "Action contains invalid characters.")]
        public string Action { get; set; } = string.Empty;
        [StringLength(4000, ErrorMessage = "Content cannot exceed 4000 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}
