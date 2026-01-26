using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Repository.MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="DataType"/> entities.
    /// Provides data access logic for data types using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DataTypeRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class DataTypeRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<DataType>(context, httpContext), IDataTypeRepository
    {
    }
}
