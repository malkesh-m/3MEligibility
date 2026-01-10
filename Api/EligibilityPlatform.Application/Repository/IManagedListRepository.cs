using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing managed list entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IManagedListRepository : IRepository<ManagedList>
    {
    }
}
