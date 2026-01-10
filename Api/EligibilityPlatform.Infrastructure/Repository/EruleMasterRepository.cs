using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="EruleMaster"/> entities.
    /// Provides data access logic for erule masters using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EruleMasterRepository"/> class.
    /// </remarks>
    /// <param name="eligibilityDbContext">The database context used for data operations.</param>
    /// <param name="contextAccessor">Provides access to the current HTTP context for user-related data.</param>
    public class EruleMasterRepository(
        EligibilityDbContext eligibilityDbContext,
        IHttpContextAccessor contextAccessor) : Repository<EruleMaster>(eligibilityDbContext, contextAccessor), IEruleMasterRepository
    {
    }
}
