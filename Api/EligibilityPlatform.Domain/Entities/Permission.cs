namespace MEligibilityPlatform.Domain.Entities;

public partial class Permission
{
    public int PermissionId { get; set; }

    public int? ScreenId { get; set; }

    public string? PermissionAction { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
