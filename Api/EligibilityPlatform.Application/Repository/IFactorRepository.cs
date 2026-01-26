using System.Numerics;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing factor entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IFactorRepository : IRepository<Factor>
    {
    }
}
