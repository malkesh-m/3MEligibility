using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="ApiParameter"/> entities.
    /// Provides data access logic using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiParametersRepository"/> class.
    /// </remarks>
    /// <param name="dbContext">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class ApiParametersRepository(
        EligibilityDbContext dbContext,
        IUserContextService userContext) : Repository<ApiParameter>(dbContext, userContext), IApiParametersRepository
    {
    }
}


