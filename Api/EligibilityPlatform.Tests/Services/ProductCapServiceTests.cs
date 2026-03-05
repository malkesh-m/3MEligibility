using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class ProductCapServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IProductCapRepository> _mockRepo;
        private readonly ProductCapService _service;

        public ProductCapServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockRepo = new Mock<IProductCapRepository>();
            _mockUow.Setup(u => u.ProductCapRepository).Returns(_mockRepo.Object);
            _mockRepo.Setup(r => r.Query()).Returns(new List<ProductCap>().BuildMock());
            _service = new ProductCapService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Add_AddsAndCompletes()
        {
            var model = new ProductCapModel { ProductId = 1, TenantId = 2 };
            var entity = new ProductCap { ProductId = 1, TenantId = 2 };
            _mockMapper.Setup(m => m.Map<ProductCap>(model)).Returns(entity);

            await _service.Add(model);

            _mockRepo.Verify(r => r.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenNotFound_Throws()
        {
            _mockRepo.Setup(r => r.GetById(9)).Returns((ProductCap?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.Delete(9));
        }

        [Fact]
        public async Task Delete_RemovesAndCompletes()
        {
            var entity = new ProductCap { Id = 3 };
            _mockRepo.Setup(r => r.GetById(3)).Returns(entity);

            await _service.Delete(3);

            _mockRepo.Verify(r => r.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_ReturnsMapped()
        {
            var entities = new List<ProductCap> { new() { Id = 1, TenantId = 2 } };
            _mockRepo.Setup(r => r.GetAllByTenantId(2, false)).Returns(entities.BuildMock());
            _mockMapper.Setup(m => m.Map<List<ProductCapModel>>(It.IsAny<IEnumerable<ProductCap>>()))
                .Returns([new ProductCapModel { Id = 1 }]);

            var result = _service.GetAll(2);

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task GetByProductId_WhenMissing_Throws()
        {
            _mockRepo.Setup(r => r.Query()).Returns(new List<ProductCap>().BuildMock());

            await Assert.ThrowsAsync<Exception>(() => _service.GetByProductId(1, 2));
        }

        [Fact]
        public async Task GetByProductId_ReturnsMapped()
        {
            var entities = new List<ProductCap> { new() { ProductId = 1, TenantId = 2 } }.BuildMock();
            _mockRepo.Setup(r => r.Query()).Returns(entities);
            _mockMapper.Setup(m => m.Map<List<ProductCapModel>>(It.IsAny<List<ProductCap>>()))
                .Returns([new ProductCapModel { ProductId = 1 }]);

            var result = await _service.GetByProductId(1, 2);

            Assert.Single(result);
            Assert.Equal(1, result[0].ProductId);
        }

        [Fact]
        public void GetById_ReturnsMapped()
        {
            var entity = new ProductCap { Id = 1, TenantId = 2 };
            _mockRepo.Setup(r => r.Query()).Returns(new List<ProductCap> { entity }.BuildMock());
            _mockMapper.Setup(m => m.Map<ProductCapModel>(It.IsAny<object>())).Returns(new ProductCapModel { Id = 1 });

            var result = _service.GetById(1, 2);

            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task Update_WhenNotFound_Throws()
        {
            _mockRepo.Setup(r => r.GetById(1)).Returns((ProductCap?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.Update(new ProductCapModel { Id = 1 }));
        }

        [Fact]
        public async Task Update_UpdatesAndCompletes()
        {
            var entity = new ProductCap { Id = 1 };
            _mockRepo.Setup(r => r.GetById(1)).Returns(entity);

            await _service.Update(new ProductCapModel { Id = 1 });

            _mockRepo.Verify(r => r.Update(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
