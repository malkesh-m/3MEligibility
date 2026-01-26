using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="HistoryParameter"/> entities.
    /// Provides data access logic for history parameter records using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HistoryParameterRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class HistoryParameterRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<HistoryParameter>(context, httpContext), IHistoryParameterRepository
    {
    }
}
