using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing audit entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IAuditRepository : IRepository<Audit>
    {
    }
}
