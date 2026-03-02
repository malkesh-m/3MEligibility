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
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class ManagedListServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IExportService> _mockExportService;
        private readonly ManagedListService _service;

        public ManagedListServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockExportService = new Mock<IExportService>();
            _service = new ManagedListService(_mockUow.Object, _mockMapper.Object, _mockExportService.Object);
        }

        [Fact]
        public async Task Add_ShouldAddEntityAndComplete_WhenNoDuplicatesExist()
        {
            var model = new ManagedListAddUpdateModel { TenantId = 1, ListName = "List 1" };
            var entity = new ManagedList { TenantId = 1, ListName = "List 1" };

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(new List<ManagedList>().AsQueryable());
            _mockMapper.Setup(m => m.Map<ManagedList>(model)).Returns(entity);
            _mockUow.Setup(u => u.ManagedListRepository.Add(entity, false));

            await _service.Add(model);

            _mockUow.Verify(u => u.ManagedListRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldThrowException_WhenListNameIsDuplicate()
        {
            var model = new ManagedListAddUpdateModel { TenantId = 1, ListName = "List 1" };
            var existingData = new List<ManagedList> { new ManagedList { TenantId = 1, ListName = "List 1" } }.AsQueryable();

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(existingData);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Add(model));
            Assert.Equal("List name already exists in this entity", exception.Message);
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntityAndComplete()
        {
            var tenantId = 1;
            var id = 1;
            var entity = new ManagedList { TenantId = tenantId, ListId = id };
            var data = new List<ManagedList> { entity }.AsQueryable();

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);
            _mockUow.Setup(u => u.ManagedListRepository.Remove(entity));

            await _service.Delete(tenantId, id);

            _mockUow.Verify(u => u.ManagedListRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedModels()
        {
            var data = new List<ManagedList> { new ManagedList { TenantId = 1, ListId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);

            var expected = new List<ManagedListGetModel> { new ManagedListGetModel { ListId = 1 } };
            _mockMapper.Setup(m => m.Map<List<ManagedListGetModel>>(It.IsAny<List<ManagedList>>())).Returns(expected);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void GetById_ShouldReturnMappedModel()
        {
            var tenantId = 1;
            var id = 1;
            var entity = new ManagedList { TenantId = tenantId, ListId = id };
            var data = new List<ManagedList> { entity }.AsQueryable();
            var expected = new ManagedListGetModel { ListId = id };

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<ManagedListGetModel>(It.IsAny<ManagedList>())).Returns(expected);

            var result = _service.GetById(tenantId, id);

            Assert.NotNull(result);
            Assert.Equal(id, result.ListId);
        }

        [Fact]
        public async Task Update_ShouldUpdateEntityAndComplete_WhenNoDuplicatesExist()
        {
            var model = new ManagedListUpdateModel { TenantId = 1, ListId = 1, ListName = "Updated List 1", UpdatedBy = "User" };
            var existingEntity = new ManagedList { TenantId = 1, ListId = 1, ListName = "List 1" };
            var data = new List<ManagedList> { existingEntity }.AsQueryable();
            var mappedEntity = new ManagedList { TenantId = 1, ListId = 1, ListName = "Updated List 1", UpdatedBy = "User" };

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<ManagedListModel, ManagedList>(model, existingEntity)).Returns(mappedEntity);
            _mockUow.Setup(u => u.ManagedListRepository.Update(mappedEntity));

            await _service.Update(model);

            _mockUow.Verify(u => u.ManagedListRepository.Update(mappedEntity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenListNameIsDuplicate()
        {
            var model = new ManagedListUpdateModel { TenantId = 1, ListId = 1, ListName = "List 2" };
            var existingData = new List<ManagedList> { 
                new ManagedList { TenantId = 1, ListId = 1, ListName = "List 1" },
                new ManagedList { TenantId = 1, ListId = 2, ListName = "List 2" }
            }.AsQueryable();

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(existingData);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("List name already exists in this entity", exception.Message);
        }

        [Fact]
        public async Task MultipleDelete_ShouldRemoveEntitiesAndComplete()
        {
            var tenantId = 1;
            var ids = new List<int> { 1, 3 };
            var entity1 = new ManagedList { TenantId = tenantId, ListId = 1 };
            var entity3 = new ManagedList { TenantId = tenantId, ListId = 3 };
            var data = new List<ManagedList> { entity1, entity3 }.AsQueryable();

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);

            await _service.MultipleDelete(tenantId, ids);

            _mockUow.Verify(u => u.ManagedListRepository.Remove(entity1), Times.Once);
            _mockUow.Verify(u => u.ManagedListRepository.Remove(It.IsAny<ManagedList>()), Times.Exactly(2));
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
