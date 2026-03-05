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
using Moq;
using OfficeOpenXml;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class BulkImportServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICountryService> _mockCountryService;
        private readonly Mock<ICityService> _mockCityService;
        private readonly Mock<IEruleService> _mockEruleService;
        private readonly Mock<IParameterService> _mockParameterService;
        private readonly Mock<IConditionService> _mockConditionService;
        private readonly Mock<IFactorService> _mockFactorService;
        private readonly Mock<IManagedListService> _mockManagedListService;
        private readonly Mock<IDataTypeService> _mockDataTypeService;
        private readonly Mock<IEcardService> _mockEcardService;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly BulkImportService _service;

        public BulkImportServiceTests()
        {
            _mockUow = new();
            _mockMapper = new();
            _mockCountryService = new();
            _mockCityService = new();
            _mockEruleService = new();
            _mockParameterService = new();
            _mockConditionService = new();
            _mockFactorService = new();
            _mockManagedListService = new();
            _mockDataTypeService = new();
            _mockEcardService = new();
            _mockProductService = new();
            _mockCategoryService = new();

            _service = new(
                _mockUow.Object,
                _mockMapper.Object,
                _mockCountryService.Object,
                _mockCityService.Object,
                _mockEruleService.Object,
                _mockParameterService.Object,
                _mockConditionService.Object,
                _mockFactorService.Object,
                _mockManagedListService.Object,
                _mockDataTypeService.Object,
                _mockEcardService.Object,
                _mockProductService.Object,
                _mockCategoryService.Object
            );

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [Fact]
        public async Task BulkImport_WithEmptyExcel_ShouldReturnEmptyMessage()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                package.Workbook.Worksheets.Add("EmptySheet");
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>()))
                .ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("Skipped unrecognized sheet", result);
        }

        [Fact]
        public async Task ImportList_WithValidData_ShouldInsertRecords()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("lists");
                sheet.Cells[1, 1].Value = "ListName*";
                sheet.Cells[2, 1].Value = "Test List";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(new List<ManagedList>().BuildMock());
            _mockUow.Setup(u => u.ManagedListRepository.Add(It.IsAny<ManagedList>(), false));
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>()))
                .ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 Created", result);
        }

        [Fact]
        public async Task ImportList_WithMissingFields_ShouldSkipRows()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("lists");
                sheet.Cells[1, 1].Value = "ListName*";
                // Row 2: Col 1 is empty (invalid), but Col 3 has data (so row is not considered empty by GetRowCount)
                sheet.Cells[2, 1].Value = "";
                sheet.Cells[2, 3].Value = "SomeData";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>()))
                .ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 invalid rows skipped", result);
        }

        [Fact]
        public async Task ImportListItem_WithValidData_ShouldInsertRecords()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("listitem");
                sheet.Cells[1, 1].Value = "ListName*";
                sheet.Cells[1, 2].Value = "ItemName*";
                sheet.Cells[1, 3].Value = "ListId*";

                sheet.Cells[2, 1].Value = "Test List";
                sheet.Cells[2, 2].Value = "Test Item";
                sheet.Cells[2, 3].Value = "1";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.ListItemRepository.Query()).Returns(new List<ListItem>().BuildMock());
            _mockUow.Setup(u => u.ListItemRepository.Add(It.IsAny<ListItem>(), false));
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>())).ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 Created", result);
        }

        [Fact]
        public void GetAllImportHistory_ShouldReturnRecords()
        {
            List<ImportDocument> data = [new() { Id = 1 }];
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.GetAllImportHistory()).Returns(data);

            var result = _service.GetAllImportHistory();

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }
        [Fact]
        public async Task ImportParameterProduct_WithValidData_ShouldInsertRecords()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("parameter");
                sheet.Cells[1, 1].Value = "ParameterName*";
                sheet.Cells[1, 2].Value = "ParameterType*";
                sheet.Cells[1, 3].Value = "ParameterTypeId*";
                sheet.Cells[1, 4].Value = "IsMandatory";

                sheet.Cells[2, 1].Value = "Test Param";
                sheet.Cells[2, 2].Value = "String";
                sheet.Cells[2, 3].Value = "1";
                sheet.Cells[2, 4].Value = "true";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>().BuildMock());
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>())).ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 Created", result);
        }

        [Fact]
        public async Task ImportFactor_WithValidData_ShouldInsertRecords()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("factors");
                string[] headers = ["factorName*", "parameter*", "parameterId*", "condition*", "conditionId*", "value1*", "value2"];
                for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];

                sheet.Cells[2, 1].Value = "F1";
                sheet.Cells[2, 3].Value = "1";
                sheet.Cells[2, 5].Value = "1";
                sheet.Cells[2, 6].Value = "V1";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor>().BuildMock());
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>())).ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 Created", result);
        }

        [Fact]
        public async Task ImportCategory_WithValidData_ShouldInsertRecords()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("category");
                string[] headers = ["CategoryName*", "CatDescription*", "EntityName*", "TenantId*"];
                for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];

                sheet.Cells[2, 1].Value = "C1";
                sheet.Cells[2, 2].Value = "D1";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(new List<Category>().BuildMock());
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>())).ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 Created", result);
        }

        [Fact]
        public async Task ImportProduct_WithValidData_ShouldInsertRecords()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("product");
                string[] headers = ["Code*", "ProductName*", "Category*", "CategoryId*", "Entity*", "Entity*", "ProductImage", "Narrative", "Description"];
                for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];

                sheet.Cells[2, 1].Value = "P1";
                sheet.Cells[2, 2].Value = "Prod1";
                sheet.Cells[2, 4].Value = "1";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product>().BuildMock());
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>())).ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 Created", result);
        }

        [Fact]
        public async Task ImportEruleMaster_WithValidData_ShouldInsertRecords()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("erules");
                string[] headers = ["RuleName*", "RuleDescription*", "IsActive*"];
                for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];

                sheet.Cells[2, 1].Value = "R1";
                sheet.Cells[2, 2].Value = "D1";
                sheet.Cells[2, 3].Value = "true";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster>().BuildMock());
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>())).ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 Created", result);
        }

        [Fact]
        public async Task ImportECard_WithValidData_ShouldInsertRecords()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("ecards");
                string[] headers = ["CardName*", "CardDescription*", "ExpressionShown*"];
                for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];

                sheet.Cells[2, 1].Value = "EC1";
                sheet.Cells[2, 2].Value = "D1";
                sheet.Cells[2, 3].Value = "Rule1";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard>().BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { new() { EruleName = "Rule1", Id = 1, TenantId = 1 } }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { new() { EruleMasterId = 1, EruleId = 10, Version = 1 } }.BuildMock());
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>())).ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 Created", result);
        }

        [Fact]
        public async Task ImportPCard_WithValidData_ShouldInsertRecords()
        {
            using MemoryStream stream = new();
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.Add("productcards");
                string[] headers = ["ProductCardName*", "ProductCardDescription*", "ProductName*", "ProductId*", "ExpressionShown*"];
                for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];

                sheet.Cells[2, 1].Value = "PC1";
                sheet.Cells[2, 2].Value = "D1";
                sheet.Cells[2, 3].Value = "Prod1";
                sheet.Cells[2, 4].Value = "1";
                sheet.Cells[2, 5].Value = "Card1";
                package.Save();
            }
            stream.Position = 0;

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard>().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { new() { ProductName = "Prod1", TenantId = 1 } }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { new() { EcardName = "Card1", EcardId = 1, TenantId = 1 } }.BuildMock());
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.AddAsync(It.IsAny<ImportDocument>())).ReturnsAsync(new ImportDocument());

            var result = await _service.BulkImport(stream, "test.xlsx", "Admin", 1);

            Assert.Contains("1 Created", result);
        }
    }
}
