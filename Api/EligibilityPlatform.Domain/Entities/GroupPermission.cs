using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace MEligibilityPlatform.Domain.Entities;

public partial class GroupPermission : ITenantEntity
{
    public int PermissionId { get; set; }

    public int GroupId { get; set; }

    public int TenantId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual SecurityGroup Group { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;
}
