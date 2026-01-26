namespace MEligibilityPlatform.Domain.Entities;

public partial class Screen
{
    public int ScreenId { get; set; }

    public string ScreenName { get; set; } = null!;

    public DateTime UpdatedByDateTime { get; set; }
}
