using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class ApiParameterMapsModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "ApiParameterId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ApiParameterId must be greater than 0")]
        public int ApiParameterId { get; set; }

        [Required(ErrorMessage = "ParameterId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ParameterId must be greater than 0")]
        public int ParameterId { get; set; }
    }

    public class ApiParameterListMapModel : ApiParameterMapsModel
    {
        public DateTime LastModificationDate { get; set; }
    }
    public class ApiParameterCreateUpdateMapModel : ApiParameterMapsModel
    {

    }
    public class ApiParameterMapName : ApiParameterMapsModel
    {
        public string? ApiParameterName { get; set; }
        public string? ParameterName { get; set; }
    }
}
