using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="Factor"/> entities.
    /// Provides data access logic for factors using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FactorRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class FactorRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<Factor>(context, userContext), IFactorRepository
    {
    }
}


