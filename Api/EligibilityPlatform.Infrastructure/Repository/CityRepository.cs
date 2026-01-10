using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="City"/> entities.
    /// Provides data access logic for cities using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CityRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class CityRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<City>(context, httpContext), ICityRepository
    {
    }
}
