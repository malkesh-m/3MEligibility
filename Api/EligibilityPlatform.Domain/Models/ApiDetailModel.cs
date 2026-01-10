using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Models
{
    public class ApiDetailModel
    {
        public int ApidetailsId { get; set; }

        [Required(ErrorMessage = "FromApiId is required")]
        public int FromApiid { get; set; }

        [Required(ErrorMessage = "CallingParamName is required")]
        [StringLength(50, ErrorMessage = "CallingParamName cannot exceed 50 characters")]
        public string CallingParamName { get; set; } = string.Empty;

        public int? Apiid { get; set; }

        [StringLength(50, ErrorMessage = "SourceAPIParam cannot exceed 50 characters")]
        public string? SourceApiparam { get; set; }

        [Required(ErrorMessage = "DataTypeId is required")]
        public int DataTypeId { get; set; }

    }

    public class ApiDetailListModel : ApiDetailModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class ApiDetailCreateotUpdateModel : ApiDetailModel
    {

    }
}

