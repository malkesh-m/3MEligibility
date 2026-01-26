using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class CountryModel
    {
        public int CountryId { get; set; }
        [Required(ErrorMessage = "Country name is required.")]
        [StringLength(50, ErrorMessage = "Country name cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Country name contains invalid characters.")]
        public string? CountryName { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
    }
}
