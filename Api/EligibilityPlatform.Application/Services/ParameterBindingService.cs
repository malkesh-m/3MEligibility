using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Application.Services
{
    public class ParameterBindingService(IUnitOfWork uow,IMapper mapper) : IParameterBindingService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        public List<ParameterBindingModel> GetAllBindings(int tenantId)
        {
            var data = _uow.ParameterBindingRepository.Query()
                .Include(p => p.SystemParameter)
                .Where(p => p.TenantId == tenantId);
            return _mapper.Map<List<ParameterBindingModel>>(data);
        }

        public async Task SaveBinding(int tenantId, ParameterBindingAddModel model)
        {
            var existing = _uow.ParameterBindingRepository.Query()
                .FirstOrDefault(x => x.TenantId == tenantId && x.SystemParameterId == model.SystemParameterId);

            if (existing != null)
            {
                existing.MappedParameterId = model.MappedParameterId;
                _uow.ParameterBindingRepository.Update(existing);
            }
            else
            {
                var data = _mapper.Map<ParameterBinding>(model);
                data.TenantId = tenantId;
           
                _uow.ParameterBindingRepository.Add(data);
            }

            await _uow.CompleteAsync();
        }
    }
}
