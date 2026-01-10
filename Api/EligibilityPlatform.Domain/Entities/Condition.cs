using System.Numerics;
using System.Reflection.Metadata;

namespace EligibilityPlatform.Domain.Entities;

public partial class Condition
{
    public int ConditionId { get; set; }

    public string? ConditionValue { get; set; }
    public DateTime UpdatedByDateTime { get; set; }

    public virtual ICollection<Factor> Factors { get; set; } = [];

    public virtual ICollection<Parameter> Parameters { get; set; } = [];
}
