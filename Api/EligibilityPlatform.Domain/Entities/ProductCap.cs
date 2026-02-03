namespace MEligibilityPlatform.Domain.Entities;

public partial class ProductCap : ITenantEntity
{
    public int Id { get; set; }

    public int MinimumScore { get; set; }

    public int MaximumScore { get; set; }

    public int ProductId { get; set; }

    public decimal ProductCapPercentage { get; set; }
    public int TenantId { get; set; }


    public virtual Product Product { get; set; } = null!;
}
