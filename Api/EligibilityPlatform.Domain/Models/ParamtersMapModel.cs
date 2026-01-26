using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Models
{
    public class ParamtersMapModel
    {
        public int Apiid { get; set; }

        [Required(ErrorMessage = "Node ID is required.")]
        public int NodeId { get; set; }

        [Required(ErrorMessage = "Parameter ID is required.")]
        public int ParameterId { get; set; }

        public int? MapFunctionId { get; set; }

        [MaxLength(50, ErrorMessage = "XMLParent cannot exceed 50 characters.")]
        public string? Xmlparent { get; set; }

        [MaxLength(50, ErrorMessage = "XMLNode cannot exceed 50 characters.")]
        public string? Xmlnode { get; set; }

        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
