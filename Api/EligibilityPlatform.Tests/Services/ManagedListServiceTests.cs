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

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(new List<ManagedList>().BuildMock());
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
            var existingData = new List<ManagedList> { new() { TenantId = 1, ListName = "List 1" } }.BuildMock();

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
            var data = new List<ManagedList> { entity }.BuildMock();

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);
            _mockUow.Setup(u => u.ManagedListRepository.Remove(entity));

            await _service.Delete(tenantId, id);

            _mockUow.Verify(u => u.ManagedListRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedModels()
        {
            var data = new List<ManagedList> { new() { TenantId = 1, ListId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);

            var expected = new List<ManagedListGetModel> { new() { ListId = 1 } };
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
            var data = new List<ManagedList> { entity }.BuildMock();
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
            var model = new ManagedListUpdateModel { TenantId = 1, ListId = 1, ListName = "Updated List 1" };
            var existingEntity = new ManagedList { TenantId = 1, ListId = 1, ListName = "List 1" };
            var data = new List<ManagedList> { existingEntity }.BuildMock();
            var mappedEntity = new ManagedList { TenantId = 1, ListId = 1, ListName = "Updated List 1" };

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<ManagedListModel, ManagedList>(model, existingEntity)).Returns(mappedEntity);
            _mockUow.Setup(u => u.ManagedListRepository.Update(mappedEntity));

            await _service.Update(model);

            _mockUow.Verify(u => u.ManagedListRepository.Update(mappedEntity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task MultipleDelete_ShouldRemoveEntitiesAndComplete()
        {
            var tenantId = 1;
            var ids = new List<int> { 1, 3 };
            var entity1 = new ManagedList { TenantId = tenantId, ListId = 1 };
            var entity3 = new ManagedList { TenantId = tenantId, ListId = 3 };
            var data = new List<ManagedList> { entity1, entity3 }.BuildMock();

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);

            await _service.MultipleDelete(tenantId, ids);

            _mockUow.Verify(u => u.ManagedListRepository.Remove(entity1), Times.Once);
            _mockUow.Verify(u => u.ManagedListRepository.Remove(entity3), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ExportLists_WithSearchTerm_ShouldFilterResults()
        {
            var request = new ExportRequestModel { SearchTerm = "Match" };
            var data = new List<ManagedList> 
            { 
                new() { ListId = 1, ListName = "matchthis", TenantId = 1 },
                new() { ListId = 2, ListName = "other", TenantId = 1 }
            };

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data.BuildMock());
            _mockMapper.Setup(m => m.Map<List<ManagedListModelDescription>>(It.IsAny<List<ManagedListModelDescription>>()))
                .Returns((List<ManagedListModelDescription> src) => src);
            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<ManagedListModelDescription>>(), "Lists", It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var result = await _service.ExportLists(1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(e => e.ExportToExcel(It.Is<List<ManagedListModelDescription>>(l => l.Count == 1), "Lists", It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ImportList_ValidExcel_ShouldReturnSuccessMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Lists");
            string[] headers = ["ListName*", "Description"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            
            sheet.Cells[2, 1].Value = "L1";

            var stream = new MemoryStream(package.GetAsByteArray());
            
            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(new List<ManagedList>().BuildMock());
            _mockMapper.Setup(m => m.Map<ManagedList>(It.IsAny<ManagedList>())).Returns(new ManagedList());

            var result = await _service.ImportList(1, stream, "Tester");

            Assert.Contains("1 Created successfully.", result);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DownloadTemplate_ShouldReturnBytes()
        {
            var result = await _service.DownloadTemplate(1);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
        [Fact]
        public async Task Update_ShouldThrowException_WhenListNameIsDuplicate()
        {
            var model = new ManagedListUpdateModel { TenantId = 1, ListId = 1, ListName = "List 2" };
            var existingEntity = new ManagedList { TenantId = 1, ListId = 2, ListName = "List 2" };
            var data = new List<ManagedList> { existingEntity }.BuildMock();

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("List name already exists in this entity", exception.Message);
        }

        [Fact]
        public async Task ExportLists_WithSelection_ShouldFilterResults()
        {
            var request = new ExportRequestModel { SelectedIds = [1] };
            var data = new List<ManagedList> 
            { 
                new() { ListId = 1, ListName = "List1", TenantId = 1 },
                new() { ListId = 2, ListName = "List2", TenantId = 1 }
            };

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(data.BuildMock());
            _mockMapper.Setup(m => m.Map<List<ManagedListModelDescription>>(It.IsAny<List<ManagedListModelDescription>>()))
                .Returns((List<ManagedListModelDescription> src) => src);
            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<ManagedListModelDescription>>(), "Lists", It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var result = await _service.ExportLists(1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(e => e.ExportToExcel(It.Is<List<ManagedListModelDescription>>(l => l.Count == 1), "Lists", It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ImportList_EmptyExcel_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Lists");
            string[] headers = ["ListName*", "Description"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            
            var stream = new MemoryStream(package.GetAsByteArray());

            var result = await _service.ImportList(1, stream, "Tester");

            Assert.Equal("Uploaded File Is Empty", result);
        }

        [Fact]
        public async Task ImportList_InvalidHeader_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Lists");
            string[] headers = ["WrongName*", "Description"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            
            var stream = new MemoryStream(package.GetAsByteArray());

            var result = await _service.ImportList(1, stream, "Tester");

            Assert.Contains("Incorrect file format at Column 1", result);
        }
        [Fact]
        public async Task Add_ShouldThrowException_WhenListNameAlreadyExists()
        {
            var model = new ManagedListAddUpdateModel { ListName = "Existing", TenantId = 1 };
            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(new List<ManagedList> { new() { ListName = "Existing", TenantId = 1 } }.BuildMock());

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Add(model));
            Assert.Equal("List name already exists in this entity", ex.Message);
        }

        [Fact]
        public async Task MultipleDelete_ShouldHandleDbUpdateException()
        {
            var tenantId = 1;
            var ids = new List<int> { 1 };
            var item = new ManagedList { ListId = 1, TenantId = tenantId };
            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(new List<ManagedList> { item }.BuildMock());
            _mockUow.Setup(u => u.CompleteAsync()).ThrowsAsync(new DbUpdateException("Concurrency error"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.MultipleDelete(tenantId, ids));
            Assert.Contains("Concurrency error", ex.Message);
        }

        [Fact]
        public async Task ImportList_DuplicateRecords_ShouldSkipThem()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Lists");
            sheet.Cells[1, 1].Value = "ListName*";
            sheet.Cells[2, 1].Value = "Existing";
            var stream = new MemoryStream(package.GetAsByteArray());

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(new List<ManagedList> { new() { ListName = "Existing", TenantId = 1 } }.BuildMock());

            var result = await _service.ImportList(1, stream, "Tester");

            Assert.Contains("1 duplicated records skipped", result);
        }

        [Fact]
        public async Task ImportList_AllRowsSkipped_ShouldReturnNoNewRecords()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Lists");
            sheet.Cells[1, 1].Value = "ListName*";
            sheet.Cells[2, 2].Value = "DescOnly";
            var stream = new MemoryStream(package.GetAsByteArray());

            var result = await _service.ImportList(1, stream, "Tester");

            Assert.Equal("No new records to insert.", result);
        }

        [Fact]
        public async Task ImportList_WhenException_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Lists");
            sheet.Cells[1, 1].Value = "ListName*";
            sheet.Cells[2, 1].Value = "L1";
            var stream = new MemoryStream(package.GetAsByteArray());

            _mockUow.Setup(u => u.ManagedListRepository.Query()).Throws(new Exception("DB fail"));

            var result = await _service.ImportList(1, stream, "Tester");

            Assert.Contains("Something went wrong", result);
        }

        [Fact]
        public void PrivateHelpers_ShouldPopulateDropdownAndFormula()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Lists");
            sheet.Cells[1, 1].Value = "H";
            sheet.Cells[2, 1].Value = "X";

            var populate = typeof(ManagedListService).GetMethod("PopulateColumn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var applyDropdown = typeof(ManagedListService).GetMethod("ApplyDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var addFormula = typeof(ManagedListService).GetMethod("AddFormula", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

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
