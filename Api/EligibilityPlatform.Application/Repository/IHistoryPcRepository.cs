using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing history PC entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IHistoryPcRepository : IRepository<HistoryPc>
    {
    }
}
