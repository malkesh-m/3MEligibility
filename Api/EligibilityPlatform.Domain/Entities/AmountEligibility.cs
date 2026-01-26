namespace MEligibilityPlatform.Domain.Entities;

public partial class AmountEligibility
{
    public int Id { get; set; }

    public int PcardId { get; set; }

    public int AmountPrcentage { get; set; }

    public int EligiblePercentage { get; set; }

    public virtual Pcard Pcard { get; set; } = null!;
}
