namespace MEligibilityPlatform.Domain.Entities;

public partial class ProductCap
{
    public int Id { get; set; }

    public int MinimumScore { get; set; }

    public int MaximumScore { get; set; }

    public int ProductId { get; set; }

    public decimal ProductCapPercentage { get; set; }

    public virtual Product Product { get; set; } = null!;
}
