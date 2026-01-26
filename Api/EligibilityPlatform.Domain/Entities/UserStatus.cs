namespace MEligibilityPlatform.Domain.Entities;

public partial class UserStatus
{
    public int StatusId { get; set; }

    public string? StatusName { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual ICollection<User> Users { get; set; } = [];
}
