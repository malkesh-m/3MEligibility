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
    /// Repository implementation for managing <see cref="EvaluationHistory"/> entities.
    /// Provides data access logic for evaluation history records using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EvaluationHistoryRepository"/> class.
    /// </remarks>
    /// <param name="eligibilityDbContext">The database context used for data operations.</param>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context for user-related data.</param>
    public class EvaluationHistoryRepository(
        EligibilityDbContext eligibilityDbContext,
        IHttpContextAccessor httpContextAccessor) : Repository<EvaluationHistory>(eligibilityDbContext, httpContextAccessor), IEvaluationHistoryRepository
    {
    }
}
