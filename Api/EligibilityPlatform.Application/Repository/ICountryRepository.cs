using System.Diagnostics.Metrics;
using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing country entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface ICountryRepository : IRepository<Country>
    {
    }
}
