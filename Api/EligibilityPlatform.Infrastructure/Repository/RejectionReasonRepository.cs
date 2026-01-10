using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace EligibilityPlatform.Infrastructure.Repository
{
    public class RejectionReasonRepository(EligibilityDbContext context, IHttpContextAccessor httpContext) : Repository<RejectionReasons>(context, httpContext), IRejectionReasonRepository
    {
        private readonly IHttpContextAccessor _httpContext = httpContext;
    }
}
