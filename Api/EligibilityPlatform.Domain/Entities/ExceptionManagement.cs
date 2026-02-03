namespace MEligibilityPlatform.Domain.Entities;

public partial class ExceptionManagement : ITenantEntity
{
    public int ExceptionManagementId { get; set; }

    public string ExceptionName { get; set; } = null!;

    public bool IsTemporary { get; set; }

    public string Scope { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? StartDate { get; set; }

    public decimal FixedPercentage { get; set; }

    public decimal VariationPercentage { get; set; }

    public string? Description { get; set; }

    public string? CreatedBy { get; set; }

    public decimal LimitAmount { get; set; }

    public string? ExpShown { get; set; }

    public string Expression { get; set; } = null!;

    public int TenantId { get; set; }

    public string? AmountType { get; set; }

    //public virtual Entity? Entity { get; set; }

    public virtual ICollection<ExceptionProduct> ExceptionProducts { get; set; } = [];
}
