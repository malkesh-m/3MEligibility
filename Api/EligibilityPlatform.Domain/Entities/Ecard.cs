namespace EligibilityPlatform.Domain.Entities;

public partial class Ecard
{
    public int EcardId { get; set; }

    public string EcardName { get; set; } = null!;

    public string? EcardDesc { get; set; }

    public string Expression { get; set; } = null!;

    public int TenantId { get; set; }

    public string? Expshown { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsImport { get; set; }

    //public virtual Entity? Entity { get; set; }

    public virtual ICollection<HistoryEc> HistoryEcs { get; set; } = [];

    public virtual ICollection<HistoryEr> HistoryErs { get; set; } = [];

    public virtual ICollection<Pcard> Pcards { get; set; } = [];

    public virtual ICollection<Erule> Rules { get; set; } = [];
}
