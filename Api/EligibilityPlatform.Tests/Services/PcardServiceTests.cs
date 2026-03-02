using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class PcardServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IExportService> _mockExportService;
        private readonly Mock<IEcardService> _mockEcardService;
        private readonly Mock<IProductService> _mockProductService;
        private readonly PcardService _service;

        public PcardServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockExportService = new Mock<IExportService>();
            _mockEcardService = new Mock<IEcardService>();
            _mockProductService = new Mock<IProductService>();
            _service = new PcardService(_mockUow.Object, _mockMapper.Object, _mockExportService.Object, _mockEcardService.Object, _mockProductService.Object);
        }

        [Fact]
        public async Task Add_ShouldAddEntityAndReturnSuccess_WhenProductNotAssociated()
        {
            var model = new PcardAddUpdateModel { TenantId = 1, ProductId = 1 };
            var existingData = new List<Pcard>().AsQueryable();
            var mappedEntity = new Pcard { TenantId = 1, ProductId = 1 };

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(existingData);
            _mockMapper.Setup(m => m.Map<Pcard>(model)).Returns(mappedEntity);
            _mockUow.Setup(u => u.PcardRepository.Add(mappedEntity, false));

            var result = await _service.Add(model);

            Assert.Equal("Success", result);
            _mockUow.Verify(u => u.PcardRepository.Add(mappedEntity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldReturnErrorMessage_WhenProductAlreadyAssociated()
        {
            var model = new PcardAddUpdateModel { TenantId = 1, ProductId = 1 };
            var existingData = new List<Pcard> { new Pcard { TenantId = 1, ProductId = 1 } }.AsQueryable();

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(existingData);

            var result = await _service.Add(model);

            Assert.Equal("This product is already associated with another Pcards record. Please select a different product.", result);
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntityAndComplete()
        {
            var tenantId = 1;
            var id = 1;
            var entity = new Pcard { TenantId = tenantId, PcardId = id };
            var data = new List<Pcard> { entity }.AsQueryable();

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(data);
            _mockUow.Setup(u => u.PcardRepository.Remove(entity));

            await _service.Delete(tenantId, id);

            _mockUow.Verify(u => u.PcardRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnMappedModels()
        {
            var pcardData = new List<Pcard> { new Pcard { TenantId = 1, PcardId = 1 } }.AsQueryable();
            _mockUow.Setup(u => u.PcardRepository.GetAllByTenantId(1, false)).Returns(pcardData);

            var expected = new List<PcardListModel> { new PcardListModel { PcardId = 1 } };
            _mockMapper.Setup(m => m.Map<List<PcardListModel>>(pcardData)).Returns(expected);

            var result = _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void GetAll_ShouldUseTenantIsolation()
        {
            var pcardData = new List<Pcard>
            {
                new Pcard { TenantId = 1, PcardId = 1 },
                new Pcard { TenantId = 2, PcardId = 2 }
            }.AsQueryable();

            _mockUow.Setup(u => u.PcardRepository.GetAllByTenantId(1, false))
                .Returns(pcardData.Where(p => p.TenantId == 1).AsQueryable());

            _mockMapper.Setup(m => m.Map<List<PcardListModel>>(It.IsAny<IQueryable<Pcard>>()))
                .Returns(new List<PcardListModel> { new PcardListModel { PcardId = 1 } });

            var result = _service.GetAll(1);

            Assert.NotNull(result);
            _mockUow.Verify(u => u.PcardRepository.GetAllByTenantId(1, false), Times.Once);
        }

        [Fact]
        public void GetById_ShouldReturnMappedModel()
        {
            var tenantId = 1;
            var id = 1;
            var entity = new Pcard { TenantId = tenantId, PcardId = id };
            var data = new List<Pcard> { entity }.AsQueryable();
            var expected = new PcardListModel { PcardId = id };

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<PcardListModel>(entity)).Returns(expected);

            var result = _service.GetById(tenantId, id);

            Assert.NotNull(result);
            Assert.Equal(id, result.PcardId);
        }

        [Fact]
        public async Task Update_ShouldUpdateEntityAndComplete_WhenNoConflictsExist()
        {
            var model = new PcardUpdateModel { TenantId = 1, PcardId = 1, ProductId = 1 };
            var existingEntity = new Pcard { TenantId = 1, PcardId = 1, ProductId = 1 };
            var data = new List<Pcard> { existingEntity }.BuildMock();
            var mappedEntity = new Pcard { TenantId = 1, PcardId = 1, ProductId = 1 };

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<PcardUpdateModel, Pcard>(model, existingEntity)).Returns(mappedEntity);
            _mockUow.Setup(u => u.PcardRepository.Update(mappedEntity));

            await _service.Update(model);

            _mockUow.Verify(u => u.PcardRepository.Update(mappedEntity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenProductIsAlreadyAssociatedWithAnotherPcard()
        {
            var model = new PcardUpdateModel { TenantId = 1, PcardId = 1, ProductId = 2 };
            var existingData = new List<Pcard> { 
                new Pcard { TenantId = 1, PcardId = 1, ProductId = 1 },
                new Pcard { TenantId = 1, PcardId = 2, ProductId = 2 }
            }.BuildMock();

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(existingData);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("This product is already associated with another Pcards record. Please select a different product.", exception.Message);
        }

        [Fact]
        public async Task Update_ShouldThrow_WhenEntityNotFound()
        {
            var model = new PcardUpdateModel { TenantId = 1, PcardId = 99, ProductId = 1 };
            var data = new List<Pcard>().BuildMock();

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(data);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Update(model));
            Assert.Equal("Pcard entity not found in database", ex.Message);
        }

        [Fact]
        public async Task RemoveMultiple_ShouldRemoveEntitiesAndComplete()
        {
            var tenantId = 1;
            var ids = new List<int> { 1, 3 };
            var entity1 = new Pcard { TenantId = tenantId, PcardId = 1 };
            var entity3 = new Pcard { TenantId = tenantId, PcardId = 3 };
            var data = new List<Pcard> { entity1, entity3 }.AsQueryable();

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(data);

            await _service.RemoveMultiple(tenantId, ids);

            _mockUow.Verify(u => u.PcardRepository.Remove(entity1), Times.Once);
            _mockUow.Verify(u => u.PcardRepository.Remove(It.IsAny<Pcard>()), Times.Exactly(2));
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
