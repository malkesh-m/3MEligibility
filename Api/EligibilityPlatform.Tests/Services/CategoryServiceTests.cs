using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Repository;
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
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockExportService = new Mock<IExportService>();
            
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockProductRepo = new Mock<IProductRepository>();

            // Default mock setups
            _mockCategoryRepo.Setup(r => r.Query()).Returns(new List<Category>().BuildMock());
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>().BuildMock());

            _mockUow.Setup(u => u.CategoryRepository).Returns(_mockCategoryRepo.Object);
            _mockUow.Setup(u => u.ProductRepository).Returns(_mockProductRepo.Object);

            _service = new CategoryService(_mockUow.Object, _mockMapper.Object, _mockExportService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnModels()
        {
            var data = new List<Category> { new() { TenantId = 1, CategoryId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(data);
            var expectedModels = new List<CategoryListModel> { new() { CategoryId = 1 } };
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
                new() { TenantId = 1, CategoryId = 1 },
                new() { TenantId = 2, CategoryId = 2 }
            }.BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(data);

            List<Category>? captured = null;
            _mockMapper.Setup(m => m.Map<List<CategoryListModel>>(It.IsAny<object>()))
                .Callback<object>(obj => captured = obj as List<Category>)
                .Returns([new CategoryListModel { CategoryId = 1 }]);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.NotNull(captured);
            Assert.All(captured!, c => Assert.Equal(1, c.TenantId));
        }

        [Fact]
        public void GetById_ShouldReturnModel()
        {
            var data = new List<Category> { new() { TenantId = 1, CategoryId = 2 } }.BuildMock();
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
            var data = new List<Category> { new() { TenantId = 1, CategoryName = "Cat1" } }.BuildMock();
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
            var catData = new List<Category> { new() { TenantId = 1, CategoryId = 1 } }.BuildMock();
            var prodData = new List<Product> { new() { CategoryId = 1 } }.BuildMock();

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
        [Fact]
        public async Task Update_WithExistingName_ShouldThrowException()
        {
            var data = new List<Category> { new() { TenantId = 1, CategoryId = 2, CategoryName = "Cat1" } }.BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(data);

            var model = new CategoryUpdateModel { TenantId = 1, CategoryId = 1, CategoryName = "Cat1" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Update(model));
        }

        [Fact]
        public async Task Update_WithValidData_ShouldUpdateAndComplete()
        {
            var data = new List<Category>().BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(data);

            var model = new CategoryUpdateModel { TenantId = 1, CategoryId = 1, CategoryName = "Cat1" };
            var existingEntity = new Category { TenantId = 1, CategoryId = 1 };
            _mockUow.Setup(u => u.CategoryRepository.GetById(1)).Returns(existingEntity);
            _mockMapper.Setup(m => m.Map<CategoryUpdateModel, Category>(model, existingEntity)).Returns(existingEntity);

            await _service.Update(model);

            _mockUow.Verify(u => u.CategoryRepository.Update(existingEntity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveMultiple_ShouldRemoveUnusedCategoriesAndReturnMessage()
        {
            var catData = new List<Category>
            {
                new() { TenantId = 1, CategoryId = 1, CategoryName = "Cat1" },
                new() { TenantId = 1, CategoryId = 2, CategoryName = "Cat2" }
            }.BuildMock();

            var prodData = new List<Product> { new() { CategoryId = 2, TenantId = 1 } }.BuildMock();

            _mockCategoryRepo.Setup(u => u.Query()).Returns(catData);
            _mockProductRepo.Setup(u => u.Query()).Returns(prodData);

            var result = await _service.RemoveMultiple(1, [1, 2]);

            Assert.Contains("Cat2", result); // Because it is used by a product
            Assert.Contains("1 Categories deleted successfully", result); // Changed from Product to Categories
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ExportCategory_WithSearch_ShouldReturnExcelStream()
        {
            var catData = new List<Category> { new() { TenantId = 1, CategoryId = 1, CategoryName = "Cat1" } }.AsQueryable().BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(catData);

            var request = new ExportRequestModel { SearchTerm = "Cat" };
            var mockStream = new System.IO.MemoryStream();
            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<IEnumerable<CategoryCsvModel>>(), "Categories", It.IsAny<string[]>())).ReturnsAsync(mockStream);

            var result = await _service.ExportCategory(1, request);

            Assert.NotNull(result);
            Assert.Same(mockStream, result);
        }

        [Fact]
        public async Task ExportCategory_WithSelection_ShouldReturnExcelStream()
        {
            var catData = new List<Category> { new() { TenantId = 1, CategoryId = 1, CategoryName = "Cat1" } }.AsQueryable().BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(catData);

            var request = new ExportRequestModel { SelectedIds = [1] };
            var mockStream = new System.IO.MemoryStream();
            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<IEnumerable<CategoryCsvModel>>(), "Categories", It.IsAny<string[]>())).ReturnsAsync(mockStream);

            var result = await _service.ExportCategory(1, request);

            Assert.NotNull(result);
            Assert.Same(mockStream, result);
        }

        [Fact]
        public async Task DownloadTemplate_ShouldReturnExcelByteArray()
        {
            var result = await _service.DownloadTemplate(1);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
        [Fact]
        public async Task ImportCategory_WithValidExcel_ShouldReturnSuccessMessage()
        {
            using var stream = new System.IO.MemoryStream();
            using (var package = new OfficeOpenXml.ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("Category");
                sheet.Cells[1, 1].Value = "CategoryName*";
                sheet.Cells[1, 2].Value = "CatDescription";
                sheet.Cells[2, 1].Value = "NewCat";
                sheet.Cells[2, 2].Value = "Desc";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(new List<Category>().BuildMock());
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            var result = await _service.ImportCategory(1, stream, "Admin");

            Assert.Contains("1 Created", result);
            _mockUow.Verify(u => u.CategoryRepository.Add(It.IsAny<Category>(), false), Times.Once);
        }

        [Fact]
        public async Task ImportCategory_WithDuplicateName_ShouldSkipAndReturnMessage()
        {
            using var stream = new System.IO.MemoryStream();
            using (var package = new OfficeOpenXml.ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("Category");
                sheet.Cells[1, 1].Value = "CategoryName*";
                sheet.Cells[2, 1].Value = "ExistingCat";
                package.Save();
            }
            stream.Position = 0;

            var existing = new List<Category> { new() { CategoryName = "ExistingCat", TenantId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(existing);

            var result = await _service.ImportCategory(1, stream, "Admin");

            Assert.Contains("1 duplicates skipped", result);
            _mockUow.Verify(u => u.CategoryRepository.Add(It.IsAny<Category>(), false), Times.Never);
        }

        [Fact]
        public async Task ImportCategory_EmptyFile_ShouldReturnErrorMessage()
        {
            using var stream = new System.IO.MemoryStream();
            using (var package = new OfficeOpenXml.ExcelPackage(stream))
            {
                package.Workbook.Worksheets.Add("Category");
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportCategory(1, stream, "Admin");

            Assert.Equal("Uploaded File Is Empty", result);
        }
        [Fact]
        public async Task Update_ShouldUpdateAndComplete()
        {
            var model = new CategoryUpdateModel { CategoryId = 1, CategoryName = "Updated", TenantId = 1 };
            var entity = new Category { CategoryId = 1, CategoryName = "Old", TenantId = 1 };

            _mockCategoryRepo.Setup(u => u.GetById(1)).Returns(entity);
            _mockCategoryRepo.Setup(u => u.Update(entity));
            _mockMapper.Setup(m => m.Map<CategoryUpdateModel, Category>(model, entity))
                .Callback<CategoryUpdateModel, Category>((src, dest) => dest.CategoryName = src.CategoryName??"")
                .Returns(entity);

            await _service.Update(model);

            Assert.Equal("Updated", entity.CategoryName);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Remove_ShouldRemoveAndComplete()
        {
            var entity = new Category { CategoryId = 1, TenantId = 1 };
            _mockCategoryRepo.Setup(u => u.Query()).Returns(new List<Category> { entity }.BuildMock());
            _mockProductRepo.Setup(u => u.Query()).Returns(new List<Product>().BuildMock());

            await _service.Remove(1, 1);

            _mockUow.Verify(u => u.CategoryRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedList()
        {
            var data = new List<Category> { new() { TenantId = 1, CategoryName = "Cat1" } }.BuildMock();
            _mockCategoryRepo.Setup(u => u.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<List<CategoryListModel>>(It.IsAny<List<Category>>())).Returns([new CategoryListModel { CategoryName = "Cat1" }]);

            var result = await _service.GetAll(1);

            Assert.Single(result);
            Assert.Equal("Cat1", result[0].CategoryName);
        }

        [Fact]
        public async Task DownloadTemplate_ShouldReturnBytes()
        {
            var result = await _service.DownloadTemplate(1);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ImportCategory_AllRowsSkipped_ShouldReturnNoNewRecords()
        {
            using var stream = new System.IO.MemoryStream();
            using (var package = new OfficeOpenXml.ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("Category");
                sheet.Cells[1, 1].Value = "CategoryName*";
                sheet.Cells[1, 2].Value = "CatDescription";
                sheet.Cells[2, 2].Value = "DescOnly";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportCategory(1, stream, "Admin");

            Assert.Equal("No new records to insert.", result);
        }

        [Fact]
        public async Task RemoveMultiple_NoMatchingIds_ShouldReturnNoCategoriesDeleted()
        {
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product>().BuildMock());
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(new List<Category>().BuildMock());

            var result = await _service.RemoveMultiple(1, [999]);

            Assert.Equal("No Categories were deleted.", result);
        }

        [Fact]
        public async Task RemoveMultiple_WhenException_ShouldReturnExceptionMessage()
        {
            _mockUow.Setup(u => u.ProductRepository.Query()).Throws(new Exception("Failure"));

            var result = await _service.RemoveMultiple(1, [1]);

            Assert.Equal("Failure", result);
        }

        [Fact]
        public void PrivateHelpers_ShouldPopulateAndAddFormula()
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Sheet1");
            sheet.Cells[1, 1].Value = "H";
            sheet.Cells[2, 1].Value = "X";

            var populate = typeof(CategoryService).GetMethod("PopulateColumn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var applyDropdown = typeof(CategoryService).GetMethod("ApplyDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var addFormula = typeof(CategoryService).GetMethod("AddFormula", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(populate);
            Assert.NotNull(applyDropdown);
            Assert.NotNull(addFormula);

            populate!.Invoke(null, new object[] { sheet, new[] { "A", "B" }, 1 });
            applyDropdown!.Invoke(null, new object[] { sheet, "TestRange", "A", 1, 3 });
            addFormula!.Invoke(null, new object[] { sheet, "B", "A", 1, 2, 2 });

            Assert.Equal("A", sheet.Cells[2, 1].Text);
            Assert.NotNull(package.Workbook.Names["TestRange"]);
            Assert.Contains("VLOOKUP", sheet.Cells[2, 2].Formula);
        }
    }
}
