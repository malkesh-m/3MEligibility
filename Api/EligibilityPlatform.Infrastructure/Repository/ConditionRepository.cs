using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="Condition"/> entities.
    /// Provides data access logic for conditions using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ConditionRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class ConditionRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<Condition>(context, userContext), IConditionRepository
    {
    }
}


