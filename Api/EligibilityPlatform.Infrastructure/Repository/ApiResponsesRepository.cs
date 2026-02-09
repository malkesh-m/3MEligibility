using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="ApiResponse"/> entities.
    /// Provides data access logic using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiResponsesRepository"/> class.
    /// </remarks>
    /// <param name="dbContext">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class ApiResponsesRepository(
        EligibilityDbContext dbContext,
        IUserContextService userContext) : Repository<ApiResponse>(dbContext, userContext), IApiResponsesRepository
    {
    }
}


