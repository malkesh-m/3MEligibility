using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing ParameterBinding entities.
    /// </summary>
    public class ParameterBindingRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<ParameterBinding>(context, userContext), IParameterBindingRepository
    {
    }
}


