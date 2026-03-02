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
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IExportService> _mockExportService;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockExportService = new Mock<IExportService>();
            _service = new CategoryService(_mockUow.Object, _mockMapper.Object, _mockExportService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnModels()
        {
            var data = new List<Category> { new Category { TenantId = 1, CategoryId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(data);
            var expectedModels = new List<CategoryListModel> { new CategoryListModel { CategoryId = 1 } };
            _mockMapper.Setup(m => m.Map<List<CategoryListModel>>(It.IsAny<List<Category>>())).Returns(expectedModels);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAll_ShouldFilterByTenant()
        {
            var data = new List<Category>
            {
                new Category { TenantId = 1, CategoryId = 1 },
                new Category { TenantId = 2, CategoryId = 2 }
            }.BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(data);

            List<Category>? captured = null;
            _mockMapper.Setup(m => m.Map<List<CategoryListModel>>(It.IsAny<object>()))
                .Callback<object>(obj => captured = obj as List<Category>)
                .Returns(new List<CategoryListModel> { new CategoryListModel { CategoryId = 1 } });

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.NotNull(captured);
            Assert.All(captured!, c => Assert.Equal(1, c.TenantId));
        }

        [Fact]
        public void GetById_ShouldReturnModel()
        {
            var data = new List<Category> { new Category { TenantId = 1, CategoryId = 2 } }.BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(data);
            var expectedModel = new CategoryListModel { CategoryId = 2 };
            _mockMapper.Setup(m => m.Map<CategoryListModel>(It.IsAny<Category>())).Returns(expectedModel);

            var result = _service.GetById(1, 2);

            Assert.NotNull(result);
            Assert.Equal(2, result.CategoryId);
        }

        [Fact]
        public async Task Add_WithExistingName_ShouldThrowException()
        {
            var data = new List<Category> { new Category { TenantId = 1, CategoryName = "Cat1" } }.BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(data);

            var model = new CategoryCreateUpdateModel { TenantId = 1, CategoryName = "Cat1" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Add(model));
        }

        [Fact]
        public async Task Add_WithValidData_ShouldAddAndComplete()
        {
            var data = new List<Category>().BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(data);

            var model = new CategoryCreateUpdateModel { TenantId = 1, CategoryName = "Cat1" };
            var entity = new Category { TenantId = 1, CategoryName = "Cat1" };
            _mockMapper.Setup(m => m.Map<Category>(model)).Returns(entity);

            await _service.Add(model);

            _mockUow.Verify(u => u.CategoryRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Remove_WhenReferencedByProduct_ShouldReturnErrorMessage()
        {
            var catData = new List<Category> { new Category { TenantId = 1, CategoryId = 1 } }.BuildMock();
            var prodData = new List<Product> { new Product { CategoryId = 1 } }.BuildMock();

            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(catData);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(prodData);

            var result = await _service.Remove(1, 1);

            Assert.Contains("Cannot delete", result);
            _mockUow.Verify(u => u.CategoryRepository.Remove(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task Remove_WhenNotReferenced_ShouldDeleteAndComplete()
        {
            var category = new Category { TenantId = 1, CategoryId = 1 };
            var catData = new List<Category> { category }.BuildMock();
            var prodData = new List<Product>().BuildMock();

            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(catData);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(prodData);

            var result = await _service.Remove(1, 1);

            Assert.Equal("Deleted Successfully", result);
            _mockUow.Verify(u => u.CategoryRepository.Remove(category), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
