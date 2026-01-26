using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing managed list entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IManagedListRepository : IRepository<ManagedList>
    {
    }
}
