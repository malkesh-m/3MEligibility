using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Entities;

public partial class ListItem : ITenantEntity
{
    [Key]
    public int ItemId { get; set; }
    [Required]
    public string ItemName { get; set; } = string.Empty;
    [Required]
    public string Code { get; set; } = string.Empty;
    [Required]
    public int ListId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsImport { get; set; }
    public int TenantId { get; set; }


    public virtual ManagedList? List { get; set; }
}
