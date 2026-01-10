using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="ApiParameterMap"/> entities.
    /// Provides data access logic using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiParameterMapsRepository"/> class.
    /// </remarks>
    /// <param name="dbContext">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class ApiParameterMapsRepository(
        EligibilityDbContext dbContext,
        IHttpContextAccessor httpContext) : Repository<ApiParameterMap>(dbContext, httpContext), IApiParameterMapsRepository
    {
    }
}
