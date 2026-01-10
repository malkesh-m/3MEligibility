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
    public class IntegrationApiEvaluationRepository(
        EligibilityDbContext dbContext,
        IHttpContextAccessor httpContext) : Repository<IntegrationApiEvaluation>(dbContext, httpContext), IIntegrationApiEvaluationRepository
    {
    }
}
