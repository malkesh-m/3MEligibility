using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing API response entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IApiResponsesRepository : IRepository<ApiResponse>
    {
    }
}
