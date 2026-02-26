namespace MEligibilityPlatform.Domain.Entities;

public partial class Pcard : ITenantEntity
{
    public int PcardId { get; set; }

    public string PcardName { get; set; } = null!;

    public string? PcardDesc { get; set; }

    public string Expression { get; set; } = null!;

    public int TenantId { get; set; }

    public int ProductId { get; set; }

    public string? Expshown { get; set; }

    public string? Pstatus { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    //public decimal Amount { get; set; }

    public bool IsImport { get; set; }

    public virtual ICollection<AmountEligibility> AmountEligibilities { get; set; } = [];

    //public virtual Entity? Entity { get; set; }

    public virtual ICollection<HistoryPc> HistoryPcs { get; set; } = [];

    public virtual Product? Product { get; set; }

    public virtual ICollection<Ecard> Ecards { get; set; } = [];
}
