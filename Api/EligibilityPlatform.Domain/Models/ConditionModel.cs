using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class ConditionModel
    {
        public int ConditionId { get; set; }
        [Required(ErrorMessage = "Condition value is required.")]
        [StringLength(20, ErrorMessage = "Condition value cannot exceed 20 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Condition value contains invalid characters.")]
        public string? ConditionValue { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
    }
}
