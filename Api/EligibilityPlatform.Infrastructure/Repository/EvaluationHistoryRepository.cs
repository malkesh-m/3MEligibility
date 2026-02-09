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
    /// Repository implementation for managing <see cref="EvaluationHistory"/> entities.
    /// Provides data access logic for evaluation history records using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EvaluationHistoryRepository"/> class.
    /// </remarks>
    /// <param name="eligibilityDbContext">The database context used for data operations.</param>
    /// <param name="userContextAccessor">Provides access to the current HTTP context for user-related data.</param>
    public class EvaluationHistoryRepository(
        EligibilityDbContext eligibilityDbContext,
        IUserContextService userContextAccessor) : Repository<EvaluationHistory>(eligibilityDbContext, userContextAccessor), IEvaluationHistoryRepository
    {
    }
}


