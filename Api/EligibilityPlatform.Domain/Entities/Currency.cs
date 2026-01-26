namespace MEligibilityPlatform.Domain.Entities;

public partial class Currency
{
    public int CurrencyId { get; set; }

    public string? CurrencyName { get; set; }

    public int? DecimalDigits { get; set; }

    public string? Isocode { get; set; }

    public int? IsoNumber { get; set; }

    public string? MinorUnitsName { get; set; }

    public decimal? MidRate { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    //public virtual ICollection<Entity> Entities { get; set; } = new List<Entity>();
}
