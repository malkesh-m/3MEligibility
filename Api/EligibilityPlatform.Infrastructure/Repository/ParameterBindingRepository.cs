using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing ParameterBinding entities.
    /// </summary>
    public class ParameterBindingRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<ParameterBinding>(context, httpContext), IParameterBindingRepository
    {
    }
}
