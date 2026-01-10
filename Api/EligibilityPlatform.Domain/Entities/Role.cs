namespace EligibilityPlatform.Domain.Entities;

public partial class Role
{
    public int RoleId { get; set; }

    public int? ScreenId { get; set; }

    public string? RoleAction { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<GroupRole> GroupRoles { get; set; } = [];
}
