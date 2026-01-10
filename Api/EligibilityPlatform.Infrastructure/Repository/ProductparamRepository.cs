using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="ProductParam"/> entities.
    /// Provides data access logic for product parameters using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductparamRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class ProductparamRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<ProductParam>(context, httpContext), IProductParamRepository
    {
    }
}
