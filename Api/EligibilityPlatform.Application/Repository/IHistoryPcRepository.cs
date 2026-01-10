using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing history PC entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IHistoryPcRepository : IRepository<HistoryPc>
    {
    }
}
