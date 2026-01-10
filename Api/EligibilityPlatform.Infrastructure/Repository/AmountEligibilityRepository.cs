using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="AmountEligibility"/> entities.
    /// Provides data access logic using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AmountEligibilityRepository"/> class.
    /// </remarks>
    /// <param name="eligibilityDbContext">The database context used for data operations.</param>
    /// <param name="contextAccessor">Provides access to the current HTTP context for user-related data.</param>
    public class AmountEligibilityRepository(EligibilityDbContext eligibilityDbContext,
        IHttpContextAccessor contextAccessor) : Repository<AmountEligibility>(eligibilityDbContext, contextAccessor), IAmountEligibilityRepository
    {
    }
}
