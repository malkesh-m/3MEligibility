using System.Diagnostics.Metrics;

namespace EligibilityPlatform.Domain.Entities;

public partial class City
{
    public int CityId { get; set; }

    public string? CityName { get; set; }

    public int? CountryId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual Country? Country { get; set; }

    //public virtual ICollection<Entity> Entities { get; set; } = [];
}
