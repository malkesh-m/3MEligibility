namespace MEligibilityPlatform.Domain.Entities;

public partial class UserGroup : ITenantEntity
{
    public int UserId { get; set; }

    public int GroupId { get; set; }

    public int TenantId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual SecurityGroup Group { get; set; } = null!;

    //public virtual User User { get; set; } = null!;
}
