using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    public class RejectionReasonRepository(EligibilityDbContext context, IUserContextService userContext) : Repository<RejectionReasons>(context, userContext), IRejectionReasonRepository
    {
    }
}


