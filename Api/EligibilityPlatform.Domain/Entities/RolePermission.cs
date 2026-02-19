using System.ComponentModel.DataAnnotations.Schema;

namespace MEligibilityPlatform.Domain.Entities;

public partial class RolePermission : ITenantEntity
{
    public int PermissionId { get; set; }

    public int RoleId { get; set; }

    public int TenantId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual SecurityRole Role { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;
}
