using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class ParameterBindingServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ParameterBindingService _service;

        public ParameterBindingServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new ParameterBindingService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public void GetAllBindings_ShouldReturnMappedModels()
        {
            var entities = new List<ParameterBinding> { new ParameterBinding { Id = 1, TenantId = 1 } }.AsQueryable();
            
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(entities);
            _mockMapper.Setup(m => m.Map<List<ParameterBindingModel>>(It.IsAny<IQueryable<ParameterBinding>>()))
                .Returns(new List<ParameterBindingModel> { new ParameterBindingModel { Id = 1 } });

            var result = _service.GetAllBindings(1);

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task SaveBinding_ShouldUpdateExistingBindingAndComplete()
        {
            var model = new ParameterBindingAddModel { SystemParameterId = 1, MappedParameterId = 2 };
            var existing = new ParameterBinding { SystemParameterId = 1, TenantId = 1, MappedParameterId = 0 };

            _mockUow.Setup(u => u.ParameterBindingRepository.Query())
                .Returns(new List<ParameterBinding> { existing }.AsQueryable());

            await _service.SaveBinding(1, model);

            Assert.Equal(2, existing.MappedParameterId);
            _mockUow.Verify(u => u.ParameterBindingRepository.Update(existing), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task SaveBinding_ShouldAddNewBindingAndComplete_WhenNotExists()
        {
            var model = new ParameterBindingAddModel { SystemParameterId = 1, MappedParameterId = 2 };
            var newEntity = new ParameterBinding { SystemParameterId = 1, MappedParameterId = 2 };

            _mockUow.Setup(u => u.ParameterBindingRepository.Query())
                .Returns(new List<ParameterBinding>().AsQueryable());
            _mockMapper.Setup(m => m.Map<ParameterBinding>(model)).Returns(newEntity);

            await _service.SaveBinding(1, model);

            Assert.Equal(1, newEntity.TenantId);
            _mockUow.Verify(u => u.ParameterBindingRepository.Add(newEntity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
