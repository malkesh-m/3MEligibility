using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class DataTypeModel
    {
        public int DataTypeId { get; set; }
        [Required(ErrorMessage = "DataType name is required.")]
        [StringLength(20, ErrorMessage = "DataType name cannot exceed 20 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "DataType name contains invalid characters.")]
        public string? DataTypeName { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
    }
}
