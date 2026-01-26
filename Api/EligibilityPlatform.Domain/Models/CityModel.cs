using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class CityModel
    {
        public int CityId { get; set; }

        [Required(ErrorMessage = "City name is required.")]
        [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "City name contains invalid characters.")]
        public string? CityName { get; set; }

        [Required(ErrorMessage = "CountryId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CountryId must be greater than 0.")]
        public int? CountryId { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }

    }
}
