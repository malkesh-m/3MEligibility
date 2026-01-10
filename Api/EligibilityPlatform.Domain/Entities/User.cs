using System.ComponentModel.DataAnnotations.Schema;

namespace EligibilityPlatform.Domain.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string LoginId { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public DateOnly? CreationDate { get; set; }

    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public bool Issuspended { get; set; }

    public DateTime? SuspentionDate { get; set; }

    public int NoOfTrials { get; set; }

    public bool ForcePasswordChange { get; set; }

    public int? StatusId { get; set; }

    public int EntityId { get; set; }

    public byte[]? UserPicture { get; set; }

    public string? MimeType { get; set; }

    public DateTime? ResetPasswordExpires { get; set; }

    public string? ResetPasswordToken { get; set; }

    public DateTime UpdatedByDateTime { get; set; }
    public DateTime LastPasswordUpdate { get; set; }

    public virtual Entity? Entity { get; set; }

    public virtual ICollection<HistoryPc> HistoryPcs { get; set; } = [];

    public virtual ICollection<MakerChecker> MakerCheckerCheckers { get; set; } = [];

    public virtual ICollection<MakerChecker> MakerCheckerMakers { get; set; } = [];

    public virtual UserStatus? Status { get; set; }

    public virtual ICollection<UserGroup> UserGroups { get; set; } = [];

    public virtual ICollection<SecurityGroup> Groups { get; set; } = [];

    [NotMapped]
    public virtual SecurityGroup? SecurityGroup { get; set; }

}
