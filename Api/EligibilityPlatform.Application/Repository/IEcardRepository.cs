using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing E-Card entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IEcardRepository : IRepository<Ecard>
    {
    }
}
