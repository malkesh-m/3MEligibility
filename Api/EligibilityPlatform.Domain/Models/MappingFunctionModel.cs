using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Models
{
    public class MappingFunctionModel
    {
        public int MapFunctionId { get; set; }

        [Required(ErrorMessage = "MapFunctionValue is required.")]
        [StringLength(20, ErrorMessage = "MapFunctionValue cannot exceed 20 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-.,()]+$", ErrorMessage = "MapFunctionValue contains invalid characters.")]
        public string? MapFunctionValue { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
