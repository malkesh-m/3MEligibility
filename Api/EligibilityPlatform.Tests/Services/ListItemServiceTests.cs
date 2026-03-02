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
    public class ListItemServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IExportService> _mockExportService;
        private readonly Mock<IManagedListService> _mockManagedListService;
        private readonly ListItemService _service;

        public ListItemServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockExportService = new Mock<IExportService>();
            _mockManagedListService = new Mock<IManagedListService>();
            _service = new ListItemService(_mockUow.Object, _mockMapper.Object, _mockExportService.Object, _mockManagedListService.Object);
        }

        [Fact]
        public async Task Add_ShouldAddEntityAndComplete_WhenNoDuplicatesExist()
        {
            var model = new ListItemCreateUpdateModel { TenantId = 1, ListId = 1, Code = "L1", ItemName = "Item 1" };
            var entity = new ListItem { TenantId = 1, ListId = 1, Code = "L1", ItemName = "Item 1" };

            _mockUow.Setup(u => u.ListItemRepository.Query()).Returns(new List<ListItem>().AsQueryable());
            _mockMapper.Setup(m => m.Map<ListItem>(model)).Returns(entity);
            _mockUow.Setup(u => u.ListItemRepository.Add(entity, false));

            await _service.Add(model);

            _mockUow.Verify(u => u.ListItemRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldThrowException_WhenCodeIsDuplicate()
        {
            var model = new ListItemCreateUpdateModel { TenantId = 1, ListId = 1, Code = "L1", ItemName = "Item 1" };
            var existingData = new List<ListItem> { new ListItem { TenantId = 1, ListId = 1, Code = "L1", ItemName = "Other Item" } }.AsQueryable();

            _mockUow.Setup(u => u.ListItemRepository.Query()).Returns(existingData);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Add(model));
            Assert.Equal("Code already exists in this List", exception.Message);
        }

        [Fact]
        public async Task Add_ShouldThrowException_WhenItemNameIsDuplicate()
        {
            var model = new ListItemCreateUpdateModel { TenantId = 1, ListId = 1, Code = "L1", ItemName = "Item 1" };
            var existingData = new List<ListItem> { new ListItem { TenantId = 1, ListId = 1, Code = "Other Code", ItemName = "Item 1" } }.AsQueryable();

            _mockUow.Setup(u => u.ListItemRepository.Query()).Returns(existingData);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Add(model));
            Assert.Equal("Item already exists in this List", exception.Message);
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntityAndComplete()
        {
            var id = 1;
            var entity = new ListItem { ItemId = id };

            _mockUow.Setup(u => u.ListItemRepository.GetById(id)).Returns(entity);
            _mockUow.Setup(u => u.ListItemRepository.Remove(entity));

            await _service.Delete(id);

            _mockUow.Verify(u => u.ListItemRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedModels()
        {
            var data = new List<ListItem> { new ListItem { TenantId = 1, ItemId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.ListItemRepository.Query()).Returns(data);

            var expected = new List<ListItemModel> { new ListItemModel { ItemId = 1 } };
            _mockMapper.Setup(m => m.Map<List<ListItemModel>>(It.IsAny<List<ListItem>>())).Returns(expected);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void GetById_ShouldReturnMappedModel()
        {
            var id = 1;
            var tenantId = 1;
            var data = new List<ListItem> { new ListItem { TenantId = tenantId, ItemId = id } }.AsQueryable();
            var expected = new ListItemModel { ItemId = id };

            _mockUow.Setup(u => u.ListItemRepository.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<ListItemModel>(It.IsAny<IQueryable<ListItem>>())).Returns(expected);

            var result = _service.GetById(id, tenantId);

            Assert.NotNull(result);
            Assert.Equal(id, result.ItemId);
        }

        [Fact]
        public async Task Update_ShouldUpdateEntityAndComplete_WhenNoDuplicatesExist()
        {
            var model = new ListItemCreateUpdateModel { TenantId = 1, ListId = 1, ItemId = 1, Code = "L1", ItemName = "Item 1" };
            var existingEntity = new ListItem { TenantId = 1, ListId = 1, ItemId = 1, Code = "Old L1", ItemName = "Old Item 1" };
            var mappedEntity = new ListItem { TenantId = 1, ListId = 1, ItemId = 1, Code = "L1", ItemName = "Item 1" };

            _mockUow.Setup(u => u.ListItemRepository.Query()).Returns(new List<ListItem>().AsQueryable());
            _mockUow.Setup(u => u.ListItemRepository.GetById(model.ItemId)).Returns(existingEntity);
            _mockMapper.Setup(m => m.Map<ListItemCreateUpdateModel, ListItem>(model, existingEntity)).Returns(mappedEntity);
            _mockUow.Setup(u => u.ListItemRepository.Update(mappedEntity));

            await _service.Update(model);

            _mockUow.Verify(u => u.ListItemRepository.Update(mappedEntity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenCodeIsDuplicate()
        {
            var model = new ListItemCreateUpdateModel { TenantId = 1, ListId = 1, ItemId = 1, Code = "L1", ItemName = "Item 1" };
            var existingData = new List<ListItem> { new ListItem { TenantId = 1, ListId = 1, ItemId = 2, Code = "L1", ItemName = "Other Item" } }.AsQueryable();

            _mockUow.Setup(u => u.ListItemRepository.Query()).Returns(existingData);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("Code already exists in this List", exception.Message);
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenItemNameIsDuplicate()
        {
            var model = new ListItemCreateUpdateModel { TenantId = 1, ListId = 1, ItemId = 1, Code = "L1", ItemName = "Item 1" };
            var existingData = new List<ListItem> { new ListItem { TenantId = 1, ListId = 1, ItemId = 2, Code = "Other Code", ItemName = "Item 1" } }.AsQueryable();

            _mockUow.Setup(u => u.ListItemRepository.Query()).Returns(existingData);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("Item already exists in this List", exception.Message);
        }

        [Fact]
        public async Task MultipleDelete_ShouldRemoveEntitiesAndComplete()
        {
            var ids = new List<int> { 1, 2, 3 };
            var entity1 = new ListItem { ItemId = 1 };
            ListItem entity2 = null; // Simulate missing
            var entity3 = new ListItem { ItemId = 3 };

            _mockUow.Setup(u => u.ListItemRepository.GetById(1)).Returns(entity1);
            _mockUow.Setup(u => u.ListItemRepository.GetById(2)).Returns(entity2);
            _mockUow.Setup(u => u.ListItemRepository.GetById(3)).Returns(entity3);

            await _service.MultipleDelete(ids);

            _mockUow.Verify(u => u.ListItemRepository.Remove(entity1), Times.Once);
            _mockUow.Verify(u => u.ListItemRepository.Remove(It.IsAny<ListItem>()), Times.Exactly(2));
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
