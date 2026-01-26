using System.Reflection.Metadata;

namespace MEligibilityPlatform.Domain.Entities;

public partial class HistoryParameter
{
    public int Seq { get; set; }

    public int? TranId { get; set; }

    public string? Expression { get; set; }

    public int? EruleId { get; set; }

    public int? ParameterId { get; set; }

    public string? ValueRet { get; set; }

    public string? Condition { get; set; }

    public bool Result { get; set; }

    public int? FactorId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual Erule? Erule { get; set; }

    public virtual Parameter? Parameter { get; set; }

    public virtual HistoryPc? Tran { get; set; }
}
