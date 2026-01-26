using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    public class IntegrationApiEvaluationRepository(
        EligibilityDbContext dbContext,
        IHttpContextAccessor httpContext) : Repository<IntegrationApiEvaluation>(dbContext, httpContext), IIntegrationApiEvaluationRepository
    {
    }
}
