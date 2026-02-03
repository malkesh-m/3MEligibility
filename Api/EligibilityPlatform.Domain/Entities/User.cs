using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MEligibilityPlatform.Domain.Entities;

public partial class User : ITenantEntity
{
    [Key]
    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string KeycloakUserId { get; set; } = string.Empty;

    //public string UserPassword { get; set; } = null!;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public DateOnly CreatedAt { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int? StatusId { get; set; }

    //public int EntityId { get; set; }
    public int TenantId { get; set; }

    //public virtual Entity? Entity { get; set; }

    public virtual ICollection<HistoryPc> HistoryPcs { get; set; } = [];

    public virtual ICollection<MakerChecker> MakerCheckerCheckers { get; set; } = [];

    public virtual ICollection<MakerChecker> MakerCheckerMakers { get; set; } = [];

    public virtual UserStatus? Status { get; set; }

    //public virtual ICollection<UserGroup> UserGroups { get; set; } = [];

    public virtual ICollection<SecurityGroup> Groups { get; set; } = [];

    [NotMapped]
    public virtual SecurityGroup? SecurityGroup { get; set; }

   
}
