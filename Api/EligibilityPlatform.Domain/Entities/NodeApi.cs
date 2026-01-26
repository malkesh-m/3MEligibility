using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Entities;

public partial class NodeApi
{
    [Key]
    public int Apiid { get; set; }

    public string? Apiname { get; set; }

    public string? Apidesc { get; set; }

    public int? NodeId { get; set; }

    public string? BinaryXml { get; set; }

    public string? XmlfileName { get; set; }

    public string? Header { get; set; }

    public string? HttpMethodType { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public string EndpointPath { get; set; } = null!;

    public bool IsActive { get; set; }

    public int? ExecutionOrder { get; set; }

    public string RequestBody { get; set; } = null!;


    public string RequestParameters { get; set; } = null!;
    public string ResponseFormate { get; set; } = string.Empty;
    public string ResponseRootPath { get; set; } = null!;

    public string TargetTable { get; set; } = null!;

    public virtual ICollection<ApiParameter> ApiParameters { get; set; } = [];

    public virtual ICollection<ApiResponse> ApiResponses { get; set; } = [];

    public virtual ICollection<Apidetail> Apidetails { get; set; } = [];

    public virtual Node? Node { get; set; }

    public virtual ICollection<ParamtersMap> ParamtersMaps { get; set; } = [];
    public virtual ICollection<IntegrationApiEvaluation> IntegrationApiEvaluations { get; set; } = [];

}
