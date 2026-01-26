using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing product cap entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IProductCapRepository : IRepository<ProductCap>
    {
    }
}
