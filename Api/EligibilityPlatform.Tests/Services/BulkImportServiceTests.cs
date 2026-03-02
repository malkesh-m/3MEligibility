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
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockCountryService = new Mock<ICountryService>();
            _mockCityService = new Mock<ICityService>();
            _mockEruleService = new Mock<IEruleService>();
            _mockParameterService = new Mock<IParameterService>();
            _mockConditionService = new Mock<IConditionService>();
            _mockFactorService = new Mock<IFactorService>();
            _mockManagedListService = new Mock<IManagedListService>();
            _mockDataTypeService = new Mock<IDataTypeService>();
            _mockEcardService = new Mock<IEcardService>();
            _mockProductService = new Mock<IProductService>();
            _mockCategoryService = new Mock<ICategoryService>();
            
            _service = new BulkImportService(
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
            using var stream = new MemoryStream();
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
            using var stream = new MemoryStream();
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
            using var stream = new MemoryStream();
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
            using var stream = new MemoryStream();
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
            var data = new List<ImportDocument> { new ImportDocument { Id = 1 } };
            _mockUow.Setup(u => u.ImportDocumentHistoryRepository.GetAllImportHistory()).Returns(data);

            var result = _service.GetAllImportHistory();

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }
    }
}
