namespace EligibilityPlatform.Domain.Entities;

public partial class Setting
{
    public int SettingId { get; set; }

    public bool IsMakerCheckerEnable { get; set; }

    public int EntityId { get; set; }

    public virtual Entity? Entity { get; set; }
}
