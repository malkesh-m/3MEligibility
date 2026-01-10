namespace EligibilityPlatform.Domain.Entities;

public partial class HistoryPc
{
    public int TranId { get; set; }

    public int? PcardId { get; set; }

    public string? Expression { get; set; }

    public int? ProductId { get; set; }

    public int EntityId { get; set; }

    public string? CustomerId { get; set; }

    public string? TransReference { get; set; }

    public DateTime? TransactionDate { get; set; }

    public int? UserId { get; set; }

    public bool Result { get; set; }

    public string? Type { get; set; }

    public int? TranRef { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual Entity? Entity { get; set; }

    public virtual ICollection<HistoryEc> HistoryEcs { get; set; } = [];

    public virtual ICollection<HistoryEr> HistoryErs { get; set; } = [];

    public virtual ICollection<HistoryParameter> HistoryParameters { get; set; } = [];

    public virtual Pcard? Pcard { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
