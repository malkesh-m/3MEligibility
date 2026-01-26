using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    public class RejectionReasonRepository(EligibilityDbContext context, IHttpContextAccessor httpContext) : Repository<RejectionReasons>(context, httpContext), IRejectionReasonRepository
    {
        private readonly IHttpContextAccessor _httpContext = httpContext;
    }
}
