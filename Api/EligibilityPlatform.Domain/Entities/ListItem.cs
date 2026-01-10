using System.ComponentModel.DataAnnotations;

namespace EligibilityPlatform.Domain.Entities;

public partial class ListItem
{
    [Key]
    public int ItemId { get; set; }
    [Required]
    public string ItemName { get; set; }
    [Required]
    public string Code { get; set; }
    [Required]
    public int ListId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsImport { get; set; }

    public virtual ManagedList? List { get; set; }
}
