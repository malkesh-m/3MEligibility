using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<IDriveService> _mockDrive;
        private readonly Mock<IParameterService> _mockParameterService;
        private readonly Mock<IFactorService> _mockFactorService;
        private readonly Mock<IExportService> _mockExportService;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockDrive = new Mock<IDriveService>();
            _mockParameterService = new Mock<IParameterService>();
            _mockFactorService = new Mock<IFactorService>();
            _mockExportService = new Mock<IExportService>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();

            _mockUow.Setup(u => u.ProductRepository).Returns(_mockProductRepo.Object);
            _mockUow.Setup(u => u.CategoryRepository).Returns(_mockCategoryRepo.Object);
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>().BuildMock());
            _mockCategoryRepo.Setup(r => r.Query()).Returns(new List<Category>().BuildMock());

            _service = new ProductService(
                _mockUow.Object,
                _mockMapper.Object,
                _mockCategoryService.Object,
                _mockEnv.Object,
                _mockDrive.Object,
                _mockParameterService.Object,
                _mockFactorService.Object,
                _mockExportService.Object);
        }

        [Fact]
        public async Task Add_WhenCategoryInvalid_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Add(new ProductAddUpdateModel { CategoryId = 0 }, "t"));
        }

        [Fact]
        public async Task Add_WhenCodeDuplicate_Throws()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>
            {
                new() { TenantId = 1, Code = "C1" }
            }.BuildMock());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Add(new ProductAddUpdateModel { TenantId = 1, CategoryId = 1, Code = "C1" }, "t"));
        }

        [Fact]
        public async Task Add_WhenNameDuplicate_Throws()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>
            {
                new() { TenantId = 1, ProductName = "P1" }
            }.BuildMock());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Add(new ProductAddUpdateModel { TenantId = 1, CategoryId = 1, ProductName = "P1" }, "t"));
        }

        [Fact]
        public async Task Add_WhenUploadSucceeds_AddsAndCompletes()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>().BuildMock());
            var file = CreateFormFile("file");
            var model = new ProductAddUpdateModel { TenantId = 1, CategoryId = 1, ProductName = "P1", Code = "C1", ProductImageFile = file };
            _mockDrive.Setup(d => d.UploadAsync(file, "t", default)).ReturnsAsync(new ApiResponse<FileUploadResponse>
            {
                Succeeded = true,
                Data = new FileUploadResponse { Id = 9, Path = "/p" }
            });
            var entity = new Product { TenantId = 1, CategoryId = 1, ProductName = "P1", Code = "C1" };
            _mockMapper.Setup(m => m.Map<Product>(model)).Returns(entity);

            await _service.Add(model, "t");

            _mockProductRepo.Verify(r => r.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            Assert.Equal(9, model.ProductImageId);
            Assert.Equal("/p", model.ProductImagePath);
        }

        [Fact]
        public async Task Add_WhenUploadFails_Throws()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>().BuildMock());
            var file = CreateFormFile("file");
            var model = new ProductAddUpdateModel { TenantId = 1, CategoryId = 1, ProductName = "P1", Code = "C1", ProductImageFile = file };
            _mockDrive.Setup(d => d.UploadAsync(file, "t", default)).ReturnsAsync(new ApiResponse<FileUploadResponse> { Succeeded = false });

            await Assert.ThrowsAsync<Exception>(() => _service.Add(model, "t"));
        }

        [Fact]
        public async Task Delete_RemovesAndDeletesImage()
        {
            var product = new Product { ProductId = 1, TenantId = 2, ProductImageId = 3 };
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product> { product }.BuildMock());

            await _service.Delete(2, 1, "t");

            _mockDrive.Verify(d => d.DeleteAsync(3, "t", default), Times.Once);
            _mockProductRepo.Verify(r => r.Remove(product), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>
            {
                new() { ProductId = 1, TenantId = 2, ProductName = "P1" }
            }.BuildMock());

            var result = await _service.GetAll(2);

            Assert.Single(result);
            Assert.Equal(1, result[0].ProductId);
        }

        [Fact]
        public void GetProductIAndName_ReturnsList()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>
            {
                new() { ProductId = 1, TenantId = 2, ProductName = "P1" }
            }.BuildMock());

            var result = _service.GetProductIAndName(2);

            Assert.Single(result);
            Assert.Equal(1, result[0].ProductId);
        }

        [Fact]
        public void GetProductName_ReturnsList()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>
            {
                new() { ProductId = 1, TenantId = 2, ProductName = "P1" }
            }.BuildMock());

            var result = _service.GetProductName(2);

            Assert.Single(result);
            Assert.Equal("P1", result[0].ProductName);
        }

        [Fact]
        public void GetProductsByCategory_Maps()
        {
            var products = new List<Product> { new() { TenantId = 1, CategoryId = 2, ProductId = 3 } }.BuildMock();
            _mockProductRepo.Setup(r => r.Query()).Returns(products);
            _mockMapper.Setup(m => m.Map<List<ProductModel>>(products)).Returns([new ProductModel { ProductId = 3 }]);

            var result = _service.GetProductsByCategory(1, 2);

            Assert.Single(result);
            Assert.Equal(3, result[0].ProductId);
        }

        [Fact]
        public void GetById_WhenNotFound_Throws()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>().BuildMock());

            Assert.Throws<Exception>(() => _service.GetById(1, 9));
        }

        [Fact]
        public void GetById_ReturnsMapped()
        {
            var product = new Product { ProductId = 1, TenantId = 2 };
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockMapper.Setup(m => m.Map<ProductListModel>(product)).Returns(new ProductListModel { ProductId = 1 });

            var result = _service.GetById(2, 1);

            Assert.Equal(1, result.ProductId);
        }

        [Fact]
        public async Task Update_WhenNotFound_Throws()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>().BuildMock());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Update(new ProductAddUpdateModel { ProductId = 1, TenantId = 2, CategoryId = 1 }, "t"));
        }

        [Fact]
        public async Task Update_WhenDuplicateCode_Throws()
        {
            var existing = new Product { ProductId = 1, TenantId = 2, Code = "C1", ProductName = "P1" };
            var data = new List<Product>
            {
                existing,
                new() { ProductId = 2, TenantId = 2, Code = "C2", ProductName = "P2" }
            }.BuildMock();
            _mockProductRepo.Setup(r => r.Query()).Returns(data);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Update(new ProductAddUpdateModel { ProductId = 1, TenantId = 2, CategoryId = 1, Code = "C2", ProductName = "P1" }, "t"));
        }

        [Fact]
        public async Task Update_WhenDuplicateName_Throws()
        {
            var existing = new Product { ProductId = 1, TenantId = 2, Code = "C1", ProductName = "P1" };
            var data = new List<Product>
            {
                existing,
                new() { ProductId = 2, TenantId = 2, Code = "C2", ProductName = "P2" }
            }.BuildMock();
            _mockProductRepo.Setup(r => r.Query()).Returns(data);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Update(new ProductAddUpdateModel { ProductId = 1, TenantId = 2, CategoryId = 1, Code = "C1", ProductName = "P2" }, "t"));
        }

        [Fact]
        public async Task Update_WhenUploadAndExistingImage_DeletesOld()
        {
            var existing = new Product { ProductId = 1, TenantId = 2, ProductImageId = 4, ProductImagePath = "/old", Code = "C1", ProductName = "P1" };
            var other = new Product { ProductId = 2, TenantId = 2, Code = "C2", ProductName = "P2" };
            var data = new List<Product> { existing, other }.BuildMock();
            _mockProductRepo.Setup(r => r.Query()).Returns(data);
            var file = CreateFormFile("file");
            var model = new ProductAddUpdateModel { ProductId = 1, TenantId = 2, CategoryId = 1, ProductImageFile = file, Code = "C1", ProductName = "P1" };
            _mockDrive.Setup(d => d.UploadAsync(file, "t", default)).ReturnsAsync(new ApiResponse<FileUploadResponse>
            {
                Succeeded = true,
                Data = new FileUploadResponse { Id = 8, Path = "/new" }
            });
            _mockMapper.Setup(m => m.Map(model, existing)).Callback<ProductAddUpdateModel, Product>((src, dest) =>
            {
                dest.ProductImageId = src.ProductImageId;
                dest.ProductImagePath = src.ProductImagePath;
            }).Returns(existing);

            await _service.Update(model, "t");

            _mockDrive.Verify(d => d.DeleteAsync(4, "t", default), Times.Once);
            _mockProductRepo.Verify(r => r.Update(existing), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            Assert.Equal(8, existing.ProductImageId);
        }

        [Fact]
        public async Task Update_WhenRemoveOldImage_Clears()
        {
            var existing = new Product { ProductId = 1, TenantId = 2, ProductImageId = 4, ProductImagePath = "/old", Code = "C1", ProductName = "P1" };
            var other = new Product { ProductId = 2, TenantId = 2, Code = "C2", ProductName = "P2" };
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product> { existing, other }.BuildMock());
            var model = new ProductAddUpdateModel { ProductId = 1, TenantId = 2, CategoryId = 1, RemoveOldImage = true, Code = "C1", ProductName = "P1" };
            _mockMapper.Setup(m => m.Map(model, existing)).Callback<ProductAddUpdateModel, Product>((src, dest) =>
            {
                dest.ProductImageId = src.ProductImageId;
                dest.ProductImagePath = src.ProductImagePath;
            }).Returns(existing);

            await _service.Update(model, "t");

            _mockDrive.Verify(d => d.DeleteAsync(4, "t", default), Times.Once);
            Assert.Null(existing.ProductImageId);
            Assert.Null(existing.ProductImagePath);
        }

        [Fact]
        public async Task Update_WhenNoImage_UsesExisting()
        {
            var existing = new Product { ProductId = 1, TenantId = 2, ProductImageId = 4, ProductImagePath = "/old", Code = "C1", ProductName = "P1" };
            var other = new Product { ProductId = 2, TenantId = 2, Code = "C2", ProductName = "P2" };
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product> { existing, other }.BuildMock());
            var model = new ProductAddUpdateModel { ProductId = 1, TenantId = 2, CategoryId = 1, Code = "C1", ProductName = "P1" };
            _mockMapper.Setup(m => m.Map(model, existing)).Callback<ProductAddUpdateModel, Product>((src, dest) =>
            {
                dest.ProductImageId = src.ProductImageId;
                dest.ProductImagePath = src.ProductImagePath;
            }).Returns(existing);

            await _service.Update(model, "t");

            Assert.Equal(4, existing.ProductImageId);
            Assert.Equal("/old", existing.ProductImagePath);
        }

        [Fact]
        public async Task RemoveMultiple_RemovesAndCompletes()
        {
            var product1 = new Product { ProductId = 1, TenantId = 2, ProductImageId = 3 };
            var product2 = new Product { ProductId = 2, TenantId = 2 };
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product> { product1, product2 }.BuildMock());

            await _service.RemoveMultiple(2, [1, 2], "t");

            _mockDrive.Verify(d => d.DeleteAsync(3, "t", default), Times.Once);
            _mockProductRepo.Verify(r => r.Remove(product1), Times.Once);
            _mockProductRepo.Verify(r => r.Remove(product2), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ExportInfo_WithSearch_ReturnsStream()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>
            {
                new() { ProductId = 1, TenantId = 2, ProductName = "Alpha", CategoryId = 1 }
            }.BuildMock());
            _mockCategoryRepo.Setup(r => r.Query()).Returns(new List<Category>
            {
                new() { CategoryId = 1, TenantId = 2, CategoryName = "Cat" }
            }.BuildMock());
            var stream = new MemoryStream();
            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<IEnumerable<ProductDescription>>(), "Products", It.IsAny<string[]>())).ReturnsAsync(stream);

            var result = await _service.ExportInfo(2, new ExportRequestModel { SearchTerm = "Alpha" });

            Assert.Same(stream, result);
        }

        [Fact]
        public async Task ExportInfo_WithSelection_ReturnsStream()
        {
            _mockProductRepo.Setup(r => r.Query()).Returns(new List<Product>
            {
                new() { ProductId = 2, TenantId = 2, ProductName = "Beta", CategoryId = 1 }
            }.BuildMock());
            _mockCategoryRepo.Setup(r => r.Query()).Returns(new List<Category>
            {
                new() { CategoryId = 1, TenantId = 2, CategoryName = "Cat" }
            }.BuildMock());
            var stream = new MemoryStream();
            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<IEnumerable<ProductDescription>>(), "Products", It.IsAny<string[]>())).ReturnsAsync(stream);

            var result = await _service.ExportInfo(2, new ExportRequestModel { SelectedIds = [2] });

            Assert.Same(stream, result);
        }

        [Fact]
        public async Task ImportEntities_AddsAndCompletes()
        {
            var csv = "ProductId,ProductName,CategoryId,CategoryName,TenantId,Code,ProductImagePath,Narrative,Description,ExceptionId,MaxEligibleAmount,ProductImageId\n" +
                      "0,P1,1,,2,C1,,,,,0,\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            _mockMapper.Setup(m => m.Map<Product>(It.IsAny<ProductModel>())).Returns(new Product { ProductName = "P1" });

            await _service.ImportEntities(2, stream);

            _mockProductRepo.Verify(r => r.Add(It.IsAny<Product>(), false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DownloadTemplate_ReturnsBytes()
        {
            _mockCategoryService.Setup(s => s.GetAll(2)).ReturnsAsync(new List<CategoryListModel>());
            _mockParameterService.Setup(s => s.GetAll(2)).ReturnsAsync(new List<ParameterListModel>());
            _mockFactorService.Setup(s => s.GetAll(2)).ReturnsAsync(new List<FactorListModel>());

            var result = await _service.DownloadTemplate(2);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetImagePath_ReturnsFullPath()
        {
            _mockEnv.Setup(e => e.WebRootPath).Returns("C:\\root");

            var result = _service.GetImagePath("images\\x.png");

            Assert.Contains("images\\x.png", result);
        }

        private static IFormFile CreateFormFile(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", "file.txt");
        }
    }
}
