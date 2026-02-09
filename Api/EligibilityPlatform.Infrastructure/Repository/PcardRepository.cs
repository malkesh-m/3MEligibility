using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="Pcard"/> entities.
    /// Provides data access logic for PCard records using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PcardRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class PcardRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<Pcard>(context, userContext), IPcardRepository
    {
    }
}


