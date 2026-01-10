using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing bulk import operations.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IBulkImportRepository : IRepository<Entity>
    {
    }
}
