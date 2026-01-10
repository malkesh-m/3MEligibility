using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class NodeApiModel
    {
        public int Apiid { get; set; }

        [StringLength(250, ErrorMessage = "API Name cannot exceed 250 characters.")]
        public string? Apiname { get; set; }

        public string EndpointPath { get; set; } = "";

        [StringLength(4000, ErrorMessage = "RequestBody cannot exceed 4000 characters.")]
        public string RequestBody { get; set; } = "";

        [StringLength(2000, ErrorMessage = "RequestParameters cannot exceed 2000 characters.")]
        public string RequestParameters { get; set; } = "";

        [StringLength(2000, ErrorMessage = "ResponseRootPath cannot exceed 2000 characters.")]
        public string ResponseRootPath { get; set; } = "";

        [StringLength(250, ErrorMessage = "TargetTable cannot exceed 250 characters.")]
        public string TargetTable { get; set; } = "";

        public bool? IsActive { get; set; } = true;
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "API Desc contains invalid characters.")]

        [StringLength(2000, ErrorMessage = "API Description cannot exceed 2000 characters.")]
        public string? Apidesc { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "NodeId must be greater than 0.")]
        public int? NodeId { get; set; }

        [StringLength(20, ErrorMessage = "HttpMethodType cannot exceed 20 characters.")]
        public string? HttpMethodType { get; set; }

        [StringLength(4000, ErrorMessage = "BinaryXml cannot exceed 4000 characters.")]
        public string? BinaryXml { get; set; }

        [StringLength(60, ErrorMessage = "XmlfileName cannot exceed 60 characters.")]
        public string? XmlfileName { get; set; }

        [StringLength(2000, ErrorMessage = "Header cannot exceed 2000 characters.")]
        public string? Header { get; set; }

        public string ResponseFormate { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "ExecutionOrder must be greater than 0.")]
        public int? ExecutionOrder { get; set; }


    }

    public class NodeApiListModel : NodeApiModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class NodeApiCreateOrUpdateModel : NodeApiModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]

        public string? UpdatedBy { get; set; }

    }
}
