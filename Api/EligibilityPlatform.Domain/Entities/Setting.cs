namespace MEligibilityPlatform.Domain.Entities;

public partial class Setting : ITenantEntity
{
    public int SettingId { get; set; }

    public bool IsMakerCheckerEnable { get; set; }

    public int TenantId { get; set; }

    //public virtual Entity? Entity { get; set; }
}
