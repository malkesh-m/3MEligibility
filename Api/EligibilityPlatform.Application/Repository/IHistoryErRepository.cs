using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing history ER entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IHistoryErRepository : IRepository<HistoryEr>
    {
    }
}
