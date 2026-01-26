using System.Diagnostics.Metrics;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing country entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface ICountryRepository : IRepository<Country>
    {
    }
}
