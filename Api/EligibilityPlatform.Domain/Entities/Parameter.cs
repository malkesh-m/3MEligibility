using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Entities;

public partial class Parameter : ITenantEntity
{
    public int ParameterId { get; set; }

    public string? ParameterName { get; set; }

    public bool HasFactors { get; set; }

    public int? Identifier { get; set; }

    public bool IsKyc { get; set; }

    public bool IsRequired { get; set; }

    public int TenantId { get; set; }
    public bool IsMandatory { get; set; }

    public int? DataTypeId { get; set; }

    public int? ConditionId { get; set; }

    public string? FactorOrder { get; set; }
    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsImport { get; set; }
    [Required]
    public string RejectionReason { get; set; } = string.Empty;
    [Required]
    public string RejectionReasonCode { get; set; } = string.Empty;
    public string? ValueSource { get; set; }

    public string? StaticValue { get; set; }

    public virtual ICollection<ApiParameterMap> ApisParameterMaps { get; set; } = [];

    public virtual Condition? Condition { get; set; }

    public virtual DataType? DataType { get; set; }

    //public virtual Entity? Entity { get; set; }

    //public virtual ICollection<ExceptionParameter> ExceptionParameters { get; set; } = new List<ExceptionParameter>();

    public virtual ICollection<Factor> Factors { get; set; } = [];

    public virtual ICollection<HistoryParameter> HistoryParameters { get; set; } = [];

    public virtual ICollection<ParamtersMap> ParamtersMaps { get; set; } = [];

    public virtual ICollection<ProductParam> ProductParams { get; set; } = [];

    public virtual ICollection<Erule> Rules { get; set; } = [];
    public virtual ICollection<ParameterComputedValue> ComputedValues { get; set; } = [];
}
