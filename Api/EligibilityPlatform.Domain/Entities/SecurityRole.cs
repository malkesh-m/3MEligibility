using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MEligibilityPlatform.Domain.Entities;

public partial class SecurityRole : ITenantEntity
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? RoleDesc { get; set; }


    public virtual ICollection<User> Users { get; set; } = [];
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
    public DateTime UpdatedByDateTime { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; }
    public DateTime CreatedByDateTime { get; set; }
    public string? UpdatedBy { get; set; }
    public int TenantId { get; set; }

}
