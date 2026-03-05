using System;
using System.Collections.Generic;
using System.IO;
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
using OfficeOpenXml;

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
            var existingData = new List<Pcard>().BuildMock();
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
            var existingData = new List<Pcard> { new() { TenantId = 1, ProductId = 1 } }.BuildMock();

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
            var data = new List<Pcard> { entity }.BuildMock();

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(data);
            _mockUow.Setup(u => u.PcardRepository.Remove(entity));

            await _service.Delete(tenantId, id);

            _mockUow.Verify(u => u.PcardRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnMappedModels()
        {
            var pcardData = new List<Pcard> { new() { TenantId = 1, PcardId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.PcardRepository.GetAllByTenantId(1, false)).Returns(pcardData);

            var expected = new List<PcardListModel> { new() { PcardId = 1 } };
            _mockMapper.Setup(m => m.Map<List<PcardListModel>>(pcardData)).Returns(expected);

            var result = _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
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
        public async Task RemoveMultiple_ShouldRemoveEntitiesAndComplete()
        {
            var tenantId = 1;
            var ids = new List<int> { 1, 3 };
            var entity1 = new Pcard { TenantId = tenantId, PcardId = 1 };
            var entity3 = new Pcard { TenantId = tenantId, PcardId = 3 };
            var data = new List<Pcard> { entity1, entity3 }.BuildMock();

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(data);

            await _service.RemoveMultiple(tenantId, ids);

            _mockUow.Verify(u => u.PcardRepository.Remove(entity1), Times.Once);
            _mockUow.Verify(u => u.PcardRepository.Remove(entity3), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ExportPCards_WithSearchTerm_ShouldFilterResults()
        {
            var request = new ExportRequestModel { SearchTerm = "Match" };
            var data = new List<Pcard> 
            { 
                new() { PcardId = 1, TenantId = 1, ProductId = 1 },
                new() { PcardId = 2, TenantId = 1, ProductId = 2 }
            };
            var product = new Product { ProductId = 1, TenantId = 1, ProductName = "matchthis" };

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(data.BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockMapper.Setup(m => m.Map<List<PcardCsvModel>>(It.IsAny<List<PcardCsvModel>>()))
                .Returns((List<PcardCsvModel> src) => src);

            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<PcardCsvModel>>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var result = await _service.ExportPCards(1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(e => e.ExportToExcel(It.Is<List<PcardCsvModel>>(l => l.Count == 1), It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ImportEntities_ShouldAddMappedEntities()
        {
            _mockUow.Setup(u => u.PcardRepository.Add(It.IsAny<Pcard>(), false));
            _mockMapper.Setup(m => m.Map<Pcard>(It.IsAny<PcardModel>())).Returns(new Pcard());

            await _service.ImportEntities(1, new MemoryStream());

            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ImportPCards_ValidExcel_ShouldReturnSuccessMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("PCard");
            // Fill headers
            sheet.Cells[1, 1].Value = "StreamCardName*";
            sheet.Cells[1, 2].Value = "StreamCardDescription";
            sheet.Cells[1, 3].Value = "StreamName*";
            sheet.Cells[1, 4].Value = "StreamId*";
            sheet.Cells[1, 5].Value = "ExpressionShown*";
            
            // Fill data
            sheet.Cells[2, 1].Value = "PC1";
            sheet.Cells[2, 3].Value = "P1";
            sheet.Cells[2, 4].Value = "1";
            sheet.Cells[2, 5].Value = "1";
            
            // Padding for GetRowCount(sheet) = lastNonEmptyRow - 1
            sheet.Cells[3, 1].Value = "dummy";
            sheet.Cells[3, 4].Value = "0";

            var stream = new MemoryStream(package.GetAsByteArray());
            
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard>().BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { new() { EcardId = 1, EcardName = "1" } }.BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { new() { ProductId = 1, ProductName = "P1" } }.BuildMock());
            _mockMapper.Setup(m => m.Map<Pcard>(It.IsAny<PcardListModel>())).Returns(new Pcard());

            var result = await _service.ImportPCards(1, stream, "Tester");

            Assert.Contains("Created successfully", result);
        }

        [Fact]
        public async Task DownloadTemplate_ShouldReturnBytes()
        {
            _mockEcardService.Setup(e => e.GetAll(1)).ReturnsAsync(new List<EcardListModel>());
            _mockProductService.Setup(p => p.GetAll(1)).ReturnsAsync(new List<ProductListModel>());

            var result = await _service.DownloadTemplate(1);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
        [Fact]
        public async Task ImportPCards_InvalidHeader_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("PCard");
            sheet.Cells[1, 1].Value = "WrongHeader";

            var stream = new MemoryStream(package.GetAsByteArray());
            var result = await _service.ImportPCards(1, stream, "Tester");

            Assert.Contains("Incorrect file format", result);
        }

        [Fact]
        public async Task ImportPCards_EmptyFile_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("PCard");
            // Add headers but no data to trigger "Empty" check
            sheet.Cells[1, 1].Value = "StreamCardName*";
            sheet.Cells[1, 2].Value = "StreamCardDescription";
            sheet.Cells[1, 3].Value = "StreamName*";
            sheet.Cells[1, 4].Value = "StreamId*";
            sheet.Cells[1, 5].Value = "ExpressionShown*";

            var stream = new MemoryStream(package.GetAsByteArray());
            var result = await _service.ImportPCards(1, stream, "Tester");

            Assert.Contains("Uploaded File PCards sheet is empty", result);
        }

        [Fact]
        public async Task ImportPCards_ProductNotFound_ShouldSkip()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("PCard");
            sheet.Cells[1, 1].Value = "StreamCardName*";
            sheet.Cells[1, 2].Value = "StreamCardDescription";
            sheet.Cells[1, 3].Value = "StreamName*";
            sheet.Cells[1, 4].Value = "StreamId*";
            sheet.Cells[1, 5].Value = "ExpressionShown*";
            sheet.Cells[2, 1].Value = "PC1";
            sheet.Cells[2, 2].Value = "D1";
            sheet.Cells[2, 3].Value = "NonExistentProduct";
            sheet.Cells[2, 4].Value = "999";
            sheet.Cells[2, 5].Value = "1";
            
            sheet.Cells[3, 1].Value = "dummy"; 
            sheet.Cells[3, 3].Value = "dummy";
            sheet.Cells[3, 4].Value = "0";
            sheet.Cells[3, 5].Value = "dummy";
            
            sheet.Cells[4, 1].Value = "dummy2";
            sheet.Cells[4, 3].Value = "dummy2";
            sheet.Cells[4, 4].Value = "0";
            sheet.Cells[4, 5].Value = "dummy2";

            var stream = new MemoryStream(package.GetAsByteArray());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product>().BuildMock());

            var result = await _service.ImportPCards(1, stream, "Tester");

            Assert.Contains("invalid rows", result);
        }

        [Fact]
        public async Task ImportPCards_ProductAlreadyAssociated_ShouldSkip()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("PCard");
            sheet.Cells[1, 1].Value = "StreamCardName*";
            sheet.Cells[1, 2].Value = "StreamCardDescription";
            sheet.Cells[1, 3].Value = "StreamName*";
            sheet.Cells[1, 4].Value = "StreamId*";
            sheet.Cells[1, 5].Value = "ExpressionShown*";
            
            sheet.Cells[2, 1].Value = "PC1";
            sheet.Cells[2, 3].Value = "P1";
            sheet.Cells[2, 4].Value = "10";
            sheet.Cells[2, 5].Value = "E1"; // Expression referring to Ecard E1
            
            sheet.Cells[3, 1].Value = "PC2";
            sheet.Cells[3, 3].Value = "P2";
            sheet.Cells[3, 4].Value = "11";
            sheet.Cells[3, 5].Value = "E1";

            var stream = new MemoryStream(package.GetAsByteArray());
            
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> 
            { 
                new() { ProductId = 10, ProductName = "P1", TenantId = 1 },
                new() { ProductId = 11, ProductName = "P2", TenantId = 1 }
            }.AsQueryable().BuildMock());

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { new() { ProductId = 10, TenantId = 1 } }.AsQueryable().BuildMock());
            
            // Mock dependencies for the second loop (BuildPCardExpressionFromShown)
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { new() { EcardId = 1, EcardName = "E1" } }.AsQueryable().BuildMock());
            _mockMapper.Setup(m => m.Map<Pcard>(It.IsAny<PcardListModel>())).Returns(new Pcard());

            var result = await _service.ImportPCards(1, stream, "Tester");

            Assert.Contains("duplicates skipped", result);
            Assert.Contains("already associated", result);
        }

        [Fact]
        public async Task ImportPCards_InvalidExpression_ShouldSkip()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("PCard");
            sheet.Cells[1, 1].Value = "StreamCardName*";
            sheet.Cells[1, 2].Value = "StreamCardDescription";
            sheet.Cells[1, 3].Value = "StreamName*";
            sheet.Cells[1, 4].Value = "StreamId*";
            sheet.Cells[1, 5].Value = "ExpressionShown*";
            sheet.Cells[2, 1].Value = "PC1";
            sheet.Cells[2, 2].Value = "D1";
            sheet.Cells[2, 3].Value = "P1";
            sheet.Cells[2, 4].Value = "1";
            sheet.Cells[2, 5].Value = "InvalidECard";
            
            sheet.Cells[3, 1].Value = "dummy";
            sheet.Cells[3, 3].Value = "dummy";
            sheet.Cells[3, 4].Value = "0";
            sheet.Cells[3, 5].Value = "dummy";
            
            sheet.Cells[4, 1].Value = "dummy2";
            sheet.Cells[4, 3].Value = "dummy2";
            sheet.Cells[4, 4].Value = "0";
            sheet.Cells[4, 5].Value = "dummy2";

            var stream = new MemoryStream(package.GetAsByteArray());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { new() { ProductId = 1, ProductName = "P1" } }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard>().BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard>().BuildMock());

            var result = await _service.ImportPCards(1, stream, "Tester");

            Assert.Contains("invalid rows", result);
        }

        [Fact]
        public async Task ImportPCards_RepoException_ShouldReturnGeneralErrorMessage()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("PCard");
            sheet.Cells[1, 1].Value = "StreamCardName*";
            sheet.Cells[1, 2].Value = "StreamCardDescription";
            sheet.Cells[1, 3].Value = "StreamName*";
            sheet.Cells[1, 4].Value = "StreamId*";
            sheet.Cells[1, 5].Value = "ExpressionShown*";
            sheet.Cells[2, 1].Value = "PC1";
            sheet.Cells[2, 2].Value = "D1";
            sheet.Cells[2, 3].Value = "P1";
            sheet.Cells[2, 4].Value = "1";
            sheet.Cells[2, 5].Value = "E1";
            
            sheet.Cells[3, 1].Value = "dummy";
            sheet.Cells[3, 3].Value = "dummy";
            sheet.Cells[3, 4].Value = "0";
            sheet.Cells[3, 5].Value = "dummy";
            
            sheet.Cells[4, 1].Value = "dummy2";
            sheet.Cells[4, 3].Value = "dummy2";
            sheet.Cells[4, 4].Value = "0";
            sheet.Cells[4, 5].Value = "dummy2";

            var stream = new MemoryStream(package.GetAsByteArray());
            _mockUow.Setup(u => u.ProductRepository.Query()).Throws(new Exception("Database connection failed"));

            var result = await _service.ImportPCards(1, stream, "Tester");

            Assert.Contains("Database connection failed", result);
        }
    }
}
