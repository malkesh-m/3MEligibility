using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Entities;

public partial class ManagedList : ITenantEntity
{
    [Key]
    public int ListId { get; set; }

    public string? ListName { get; set; }

    public int TenantId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsImport { get; set; }

    //public virtual Entity? Entity { get; set; }

    public virtual ICollection<ListItem> ListItems { get; set; } = [];
}
