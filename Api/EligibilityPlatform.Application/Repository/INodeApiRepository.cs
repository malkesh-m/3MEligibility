using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing node API entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface INodeApiRepository : IRepository<NodeApi>
    {
    }
}
