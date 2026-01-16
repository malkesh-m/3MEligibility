using System.Reflection.Metadata;

namespace EligibilityPlatform.Domain.Entities;

public partial class Factor
{
    public int FactorId { get; set; }

    public string? FactorName { get; set; }

    public string? Note { get; set; }

    public string? Value1 { get; set; }

    public string? Value2 { get; set; }

    public int? ParameterId { get; set; }

    public int? ConditionId { get; set; }

    public int TenantId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsImport { get; set; }

    public virtual Condition? Condition { get; set; }

    //public virtual Entity? Entity { get; set; }

    public virtual Parameter? Parameter { get; set; }
}
