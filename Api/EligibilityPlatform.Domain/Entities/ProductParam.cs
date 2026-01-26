namespace MEligibilityPlatform.Domain.Entities;

public partial class ProductParam
{
    public int ProductId { get; set; }

    public int ParameterId { get; set; }

    public int? DisplayOrder { get; set; }

    public int TenantId { get; set; }

    public string? ParamValue { get; set; }

    public bool IsRequired { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsImport { get; set; }

    public virtual Parameter Parameter { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
