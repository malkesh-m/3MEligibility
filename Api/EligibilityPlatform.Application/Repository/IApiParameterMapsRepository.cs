using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing API parameter mapping entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IApiParameterMapsRepository : IRepository<ApiParameterMap>
    {
    }
}
