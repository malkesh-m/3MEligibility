using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class ProductParamModel
    {
        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "ParameterId is required.")]
        public int ParameterId { get; set; }

        // Optional, but if provided should be greater than 0
        [Range(1, int.MaxValue, ErrorMessage = "DisplayOrder must be greater than 0.")]
        public int? DisplayOrder { get; set; }

        [JsonIgnore]
        public int EntityId { get; set; }

        // Optional string but max length 50 characters
        [StringLength(50, ErrorMessage = "ParamValue cannot exceed 50 characters.")]
        public string? ParamValue { get; set; }

        public bool IsRequired { get; set; }


    }
    public class ProductParamListModel : ProductParamModel
    {
        public string? ProductName { get; set; }
        public string? ParameterName { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public new int EntityId { get; set; }


    }
    public class ProductParamAddUpdateModel : ProductParamModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class ProductParamDescription
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int EntityId { get; set; }
        public string? EntityName { get; set; }
        public int? ParameterId { get; set; }
        public string? ParameterName { get; set; }
        public string? ParamValue { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
    }
    public class ProductParamList : ProductParamModel
    {

    }


}
