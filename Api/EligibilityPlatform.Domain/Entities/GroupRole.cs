using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace EligibilityPlatform.Domain.Entities;

public partial class GroupRole
{
    public int RoleId { get; set; }

    public int GroupId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual SecurityGroup Group { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
