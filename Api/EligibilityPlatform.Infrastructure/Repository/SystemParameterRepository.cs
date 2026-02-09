using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    public class SystemParameterRepository(EligibilityDbContext context, IUserContextService userContextAccessor) : Repository<SystemParameter>(context, userContextAccessor), ISystemParameterRepository
    {
    }
}


