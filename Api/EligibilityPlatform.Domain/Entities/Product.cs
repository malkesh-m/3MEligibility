namespace MEligibilityPlatform.Domain.Entities;

public partial class Product : ITenantEntity
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int? CategoryId { get; set; }

    public int TenantId { get; set; }

    public string? Code { get; set; }

    public byte[]? ProductImage { get; set; }

    public string? Narrative { get; set; }

    public string? Description { get; set; }

    public string? MimeType { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedByDateTime { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsImport { get; set; }

    public decimal MaxEligibleAmount { get; set; }

    public virtual Category? Category { get; set; }

    //public virtual Entity? Entity { get; set; }

    public virtual ICollection<ExceptionProduct> ExceptionProducts { get; set; } = [];

    public virtual ICollection<HistoryPc> HistoryPcs { get; set; } = [];

    public virtual Pcard? Pcard { get; set; }

    public virtual ICollection<ProductCap> ProductCaps { get; set; } = [];

    public virtual ICollection<ProductParam> ProductParams { get; set; } = [];
    public virtual ICollection<ProductCapAmount> ProductCapAmounts { get; set; } = [];

}
