using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="MakerChecker"/> entities.
    /// Provides data access logic for maker-checker workflow records using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MakerCheckerRepository"/> class.
    /// </remarks>
    /// <param name="dbContext">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class MakerCheckerRepository(
        EligibilityDbContext dbContext,
        IHttpContextAccessor httpContext) : Repository<MakerChecker>(dbContext, httpContext), IMakerCheckerRepository
    {
    }
}
