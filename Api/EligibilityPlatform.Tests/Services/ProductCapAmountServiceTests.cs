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
    public class ProductCapAmountServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IProductCapAmountRepository> _mockRepo;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly ProductCapAmountService _service;

        public ProductCapAmountServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockRepo = new Mock<IProductCapAmountRepository>();
            _mockProductRepo = new Mock<IProductRepository>();

            _mockUow.Setup(u => u.ProductCapAmountRepository).Returns(_mockRepo.Object);
            _mockUow.Setup(u => u.ProductRepository).Returns(_mockProductRepo.Object);
            _mockRepo.Setup(r => r.Query()).Returns(new List<ProductCapAmount>().BuildMock());
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>().BuildMock());

            _service = new ProductCapAmountService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Add_WhenProductMissing_Throws()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>().BuildMock());

            await Assert.ThrowsAsync<Exception>(() => _service.Add(new ProductCapAmountAddModel { ProductId = 1 }));
        }

        [Fact]
        public async Task Add_AddsAndCompletes()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product> { new() { ProductId = 1 } }.BuildMock());
            var model = new ProductCapAmountAddModel { ProductId = 1, TenantId = 2 };
            var entity = new ProductCapAmount { ProductId = 1, TenantId = 2 };
            _mockMapper.Setup(m => m.Map<ProductCapAmount>(model)).Returns(entity);

            await _service.Add(model);

            _mockRepo.Verify(r => r.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_ReturnsMapped()
        {
            var entities = new List<ProductCapAmount> { new() { Id = 1, TenantId = 2 } };
            _mockRepo.Setup(r => r.GetAllByTenantId(2, false)).Returns(entities.BuildMock());
            _mockMapper.Setup(m => m.Map<List<ProductCapAmountModel>>(It.IsAny<IEnumerable<ProductCapAmount>>()))
                .Returns([new ProductCapAmountModel { Id = 1 }]);

            var result = _service.GetAll(2);

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task Update_WhenProductMissing_Throws()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>().BuildMock());

            await Assert.ThrowsAsync<Exception>(() => _service.Update(new ProductCapAmountUpdateModel { ProductId = 1, Id = 2 }));
        }

        [Fact]
        public async Task Update_WhenCapMissing_Throws()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product> { new() { ProductId = 1 } }.BuildMock());
            _mockRepo.Setup(r => r.GetById(2)).Returns((ProductCapAmount?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.Update(new ProductCapAmountUpdateModel { ProductId = 1, Id = 2 }));
        }

        [Fact]
        public async Task Update_UpdatesAndCompletes()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product> { new() { ProductId = 1 } }.BuildMock());
            var entity = new ProductCapAmount { Id = 2, ProductId = 1 };
            _mockRepo.Setup(r => r.GetById(2)).Returns(entity);

            await _service.Update(new ProductCapAmountUpdateModel { ProductId = 1, Id = 2 });

            _mockRepo.Verify(r => r.Update(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenMissing_Throws()
        {
            _mockRepo.Setup(r => r.GetById(9)).Returns((ProductCapAmount?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.Delete(9));
        }

        [Fact]
        public async Task Delete_RemovesAndCompletes()
        {
            var entity = new ProductCapAmount { Id = 3 };
            _mockRepo.Setup(r => r.GetById(3)).Returns(entity);

            await _service.Delete(3);

            _mockRepo.Verify(r => r.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByProductId_WhenMissing_Throws()
        {
            _mockRepo.Setup(r => r.Query()).Returns(new List<ProductCapAmount>().BuildMock());

            await Assert.ThrowsAsync<Exception>(() => _service.GetByProductId(1));
        }

        [Fact]
        public async Task GetByProductId_ReturnsMapped()
        {
            var entities = new List<ProductCapAmount> { new() { ProductId = 1 } }.BuildMock();
            _mockRepo.Setup(r => r.Query()).Returns(entities);
            _mockMapper.Setup(m => m.Map<List<ProductCapAmountModel>>(It.IsAny<List<ProductCapAmount>>()))
                .Returns([new ProductCapAmountModel { ProductId = 1 }]);

            var result = await _service.GetByProductId(1);

            Assert.Single(result);
            Assert.Equal(1, result[0].ProductId);
        }
    }
}
