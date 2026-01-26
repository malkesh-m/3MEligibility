namespace MEligibilityPlatform.Domain.Entities;

public partial class ExceptionProduct
{
    public int ExceptionProductId { get; set; }

    public int ExceptionManagementId { get; set; }

    public int ProductId { get; set; }

    public virtual ExceptionManagement ExceptionManagement { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
