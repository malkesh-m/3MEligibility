namespace MEligibilityPlatform.Domain.Entities;

public partial class Country
{
    public int CountryId { get; set; }

    public string? CountryName { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual ICollection<City> Cities { get; set; } = [];

    //public virtual ICollection<Entity> Entities { get; set; } = [];
}
