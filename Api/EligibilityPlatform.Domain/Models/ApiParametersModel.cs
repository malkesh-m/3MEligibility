using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Models
{
    public class ApiParametersModel
    {
        public int ApiParamterId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "ParameterName cannot exceed 100 characters")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
            ErrorMessage = "ParameterName allows only letters, numbers, and underscore")]
        public string ParameterName { get; set; } = null!;

        [StringLength(50)]
        public string? ParameterType { get; set; }

        [Required]
        [StringLength(20)]
        public string ParameterDirection { get; set; } = null!;

        public bool IsRequired { get; set; }

        [StringLength(200)]
        public string? DefaultValue { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ApiId must be greater than 0")]
        public int? ApiId { get; set; }
        public int TenantId { get; set; }
    }

    public class ApiParametersListModel : ApiParametersModel
    {
        public DateTime CreatedByDateTime { get; set; }
        public DateTime UpdatedByDateTime { get; set; }
    }

    public class ApiParametersCreateUpdateModel : ApiParametersModel
    {

    }
}
