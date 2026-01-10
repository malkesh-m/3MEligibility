namespace EligibilityPlatform.Domain.Entities;

public partial class SecurityGroup
{
    public int GroupId { get; set; }

    public string? GroupName { get; set; }

    public string? GroupDesc { get; set; }


    public virtual ICollection<User> Users { get; set; } = [];
    public virtual ICollection<UserGroup> UserGroups { get; set; } = [];
    public virtual ICollection<GroupRole> GroupRoles { get; set; } = [];
    public DateTime UpdatedByDateTime { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; }
    public DateTime CreatedByDateTime { get; set; }
    public string? UpdatedBy { get; set; }
}
