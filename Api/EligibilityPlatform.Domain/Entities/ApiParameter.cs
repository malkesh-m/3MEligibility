using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Entities;

public partial class ApiParameter
{
    [Key]
    public int ApiParamterId { get; set; }

    public string ParameterDirection { get; set; } = null!;

    public string ParameterName { get; set; } = null!;

    public bool IsRequired { get; set; }

    public string? DefaultValue { get; set; }

    public int? ApiId { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string? ParameterType { get; set; }

    public virtual NodeApi? Api { get; set; }

    public virtual ICollection<ApiParameterMap> ApisParameterMaps { get; set; } = [];
}
