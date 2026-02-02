
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    public interface IParameterBindingService
    {
        List<ParameterBindingModel> GetAllBindings(int tenantId);
        Task SaveBinding(int tenantId, ParameterBindingAddModel model);
    }
}
