using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    public class SystemParameterRepository(EligibilityDbContext context, IHttpContextAccessor httpContextAccessor) : Repository<SystemParameter>(context, httpContextAccessor), ISystemParameterRepository
    {
    }
}
