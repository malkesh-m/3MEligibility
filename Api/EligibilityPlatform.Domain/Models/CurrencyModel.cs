using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class CurrencyModel
    {
        [Key]
        public int CurrencyId { get; set; }
        [Required(ErrorMessage = "Currency name is required.")]
        [StringLength(250, ErrorMessage = "Currency name cannot exceed 250 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Currency name contains invalid characters.")]
        public string? CurrencyName { get; set; }

        [Required(ErrorMessage = "Decimal digits are required.")]
        [Range(0, 10, ErrorMessage = "Decimal digits must be between 0 and 10.")]
        public int? DecimalDigits { get; set; }

        [Required(ErrorMessage = "ISO code is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "ISO code must be exactly 3 characters.")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "ISO code must be 3 uppercase letters.")]
        public string? Isocode { get; set; }

        [Required(ErrorMessage = "ISO number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ISO number must be a positive integer.")]
        public int? IsoNumber { get; set; }

        [StringLength(20, ErrorMessage = "Minor units name cannot exceed 20 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Minor units name contains invalid characters.")]
        public string? MinorUnitsName { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MidRate must be a positive number.")]
        public decimal? MidRate { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
    }
}
