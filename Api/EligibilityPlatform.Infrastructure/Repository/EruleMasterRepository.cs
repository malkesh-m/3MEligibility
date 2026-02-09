using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="EruleMaster"/> entities.
    /// Provides data access logic for erule masters using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EruleMasterRepository"/> class.
    /// </remarks>
    /// <param name="eligibilityDbContext">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class EruleMasterRepository(
        EligibilityDbContext eligibilityDbContext,
        IUserContextService userContext) : Repository<EruleMaster>(eligibilityDbContext, userContext), IEruleMasterRepository
    {
    }
}


