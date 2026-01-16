using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace EligibilityPlatform.Domain.Entities;

public partial class Erule
{
    public int EruleId { get; set; }

    public int EruleMasterId { get; set; }

    public string Expression { get; set; } = null!;

    public int TenantId { get; set; }

    public string? ExpShown { get; set; }

    public bool IsImport { get; set; }

    public int Version { get; set; }

    //public bool IsActive { get; set; }

    public bool IsPublished { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public string? Comment { get; set; }

    //public virtual Entity? Entity { get; set; }

    public virtual EruleMaster? EruleMaster { get; set; }

    public virtual ICollection<HistoryEr> HistoryErs { get; set; } = [];

    public virtual ICollection<HistoryParameter> HistoryParameters { get; set; } = [];

    public virtual ICollection<Ecard> Ecards { get; set; } = [];

    public virtual ICollection<Parameter> Parameters { get; set; } = [];
}
