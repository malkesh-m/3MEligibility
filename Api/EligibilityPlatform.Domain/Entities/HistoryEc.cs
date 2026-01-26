namespace MEligibilityPlatform.Domain.Entities;

public partial class HistoryEc
{
    public int Seq { get; set; }

    public int? TranId { get; set; }

    public string? Expression { get; set; }

    public int? EcardId { get; set; }

    public bool Result { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual Ecard? Ecard { get; set; }

    public virtual HistoryPc? Tran { get; set; }
}
