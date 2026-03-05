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
using Microsoft.EntityFrameworkCore;
using Moq;
using OfficeOpenXml;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class EruleServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IParameterService> _mockParameterService;
        private readonly Mock<IConditionService> _mockConditionService;
        private readonly Mock<IFactorService> _mockFactorService;
        private readonly Mock<IExportService> _mockExportService;
        private readonly Mock<IEruleRepository> _mockEruleRepo;
        private readonly Mock<IEruleMasterRepository> _mockEruleMasterRepo;
        private readonly EruleService _service;

        public EruleServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockParameterService = new Mock<IParameterService>();
            _mockConditionService = new Mock<IConditionService>();
            _mockFactorService = new Mock<IFactorService>();
            _mockExportService = new Mock<IExportService>();
            _mockEruleRepo = new Mock<IEruleRepository>();
            _mockEruleMasterRepo = new Mock<IEruleMasterRepository>();

            _mockUow.Setup(u => u.EruleRepository).Returns(_mockEruleRepo.Object);
            _mockUow.Setup(u => u.EruleMasterRepository).Returns(_mockEruleMasterRepo.Object);
            // Also need EcardRepository for RemoveMultiple check
            _mockUow.Setup(u => u.EcardRepository).Returns(new Mock<IEcardRepository>().Object);

            _service = new EruleService(
                _mockUow.Object,
                _mockMapper.Object,
                _mockParameterService.Object,
                _mockConditionService.Object,
                _mockFactorService.Object,
                _mockExportService.Object);
        }

        [Fact]
        public async Task Add_ShouldCreateMasterAndErule()
        {
            var model = new EruleCreateOrUpdateModel { EruleName = "R1", TenantId = 1, Expression = "EXP" };
            var erule = new Erule { TenantId = 1, Expression = "EXP" };
            _mockMapper.Setup(m => m.Map<Erule>(model)).Returns(erule);

            await _service.Add(model);

            _mockEruleMasterRepo.Verify(u => u.Add(It.IsAny<EruleMaster>(), It.IsAny<bool>()), Times.Once);
            _mockEruleRepo.Verify(u => u.Add(It.IsAny<Erule>(), It.IsAny<bool>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Exactly(2));
        }


        [Fact]
        public async Task Create_ValidModel_ShouldAssignNextVersion()
        {
            var master = new EruleMaster { Id = 10, EruleName = "Master1" };
            var existingRules = new List<Erule> 
            { 
                new Erule { EruleMasterId = 10, Version = 1 },
                new Erule { EruleMasterId = 10, Version = 2 }
            }.AsQueryable().BuildMock();

            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { master }.AsQueryable().BuildMock());
            _mockEruleRepo.Setup(u => u.Query()).Returns(existingRules);
            
            var model = new EruleCreateModel { EruleMasterId = 10, EruleName = "R1", Expression = "EXP" };
            var newErule = new Erule { EruleMasterId = 10 };
            _mockMapper.Setup(m => m.Map<Erule>(model)).Returns(newErule);

            await _service.Create(model);

            Assert.Equal(3, newErule.Version);
            _mockEruleRepo.Verify(u => u.Add(newErule, It.IsAny<bool>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_PublishedRule_ShouldThrowException()
        {
            var rule = new Erule { EruleId = 1, TenantId = 1, IsPublished = true };
            _mockEruleRepo.Setup(u => u.Query()).Returns(new List<Erule> { rule }.AsQueryable().BuildMock());

            var model = new EruleUpdateModel { EruleId = 1, TenantId = 1, EruleName = "R1", Expression = "EXP" };

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("Cannot update published rule.", ex.Message);
        }

        [Fact]
        public async Task Delete_InUseByEcard_ShouldReturnErrorMessage()
        {
            var ecards = new List<Ecard> { new Ecard { Expression = "1 AND 2" } }.AsQueryable().BuildMock();
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(ecards);

            var result = await _service.Delete(1, 1);

            Assert.Contains("used in one or more ECards", result);
        }

        [Fact]
        public async Task GetAll_ShouldReturnJoinedList()
        {
            var masters = new List<EruleMaster> { new EruleMaster { Id = 1, TenantId = 1, EruleName = "M1" } }.AsQueryable().BuildMock();
            var rules = new List<Erule> { new Erule { EruleMasterId = 1, TenantId = 1, EruleId = 100 } }.AsQueryable().BuildMock();

            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(masters);
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(rules);

            var result = await _service.GetAll(1);

            Assert.Single(result);
            Assert.Equal("M1", result[0].EruleName);
            Assert.Equal(100, result[0].EruleId);
        }

        [Fact]
        public async Task PublishDraftAsync_ShouldAssignNextVersion()
        {
            var draft = new Erule { EruleId = 1, TenantId = 1, IsPublished = true, Version = 0 };
            var existing = new List<Erule> 
            { 
                new Erule { TenantId = 1, Version = 5 } 
            }.AsQueryable().BuildMock();

            var data = new List<Erule> { draft }.Concat(existing).ToList();
            _mockEruleRepo.Setup(u => u.Query()).Returns(data.AsQueryable().BuildMock());

            await _service.PublishDraftAsync(1, 1);

            Assert.Equal(6, draft.Version);
            Assert.False(draft.IsPublished);
            _mockEruleRepo.Verify(u => u.Update(draft), Times.Once);
        }

        [Fact]
        public async Task ImportErule_ValidExcel_ShouldInsertRecords()
        {
            var master = new EruleMaster { Id = 1, EruleName = "R1", TenantId = 1 };
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { master }.AsQueryable().BuildMock());
            _mockEruleRepo.Setup(u => u.Query()).Returns(new List<Erule>().AsQueryable().BuildMock());

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Rules");
                ws.Cells[1, 1].Value = "RuleName";
                ws.Cells[1, 2].Value = "Description";
                ws.Cells[1, 9].Value = "Expression";
                ws.Cells[2, 1].Value = "R1";
                ws.Cells[2, 2].Value = "D1";
                ws.Cells[2, 9].Value = "EXP1";
                package.Save();
            }
            stream.Position = 0;

            _mockMapper.Setup(m => m.Map<Erule>(It.IsAny<EruleListModel>())).Returns(new Erule());

            var result = await _service.ImportErule(1, stream, "user");

            Assert.Contains("1 Created successfully", result);
            _mockEruleRepo.Verify(u => u.Add(It.IsAny<Erule>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task DownloadTemplate_ShouldReturnExcelFile()
        {
            _mockParameterService.Setup(s => s.GetAll(1)).ReturnsAsync(new List<ParameterListModel>());
            _mockConditionService.Setup(s => s.GetAll()).Returns(new List<ConditionModel>());
            _mockFactorService.Setup(s => s.GetAll(1)).ReturnsAsync(new List<FactorListModel>());

            var result = await _service.DownloadTemplate(1);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ImportEruleMaster_ValidExcel_ShouldInsertRecords()
        {
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster>().AsQueryable().BuildMock());

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Master");
                ws.Cells[1, 1].Value = "RuleName*";
                ws.Cells[1, 2].Value = "RuleDescription";
                ws.Cells[1, 3].Value = "IsActive*";
                ws.Cells[2, 1].Value = "NewMaster";
                ws.Cells[2, 2].Value = "Desc";
                ws.Cells[2, 3].Value = "True";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportEruleMaster(1, stream, "user");

            Assert.Contains("1 Created successfully", result);
            _mockUow.Verify(u => u.EruleMasterRepository.Add(It.IsAny<EruleMaster>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldUpdateTimestamp()
        {
            var rule = new Erule { EruleId = 1, TenantId = 1 };
            _mockEruleRepo.Setup(u => u.Query()).Returns(new List<Erule> { rule }.AsQueryable().BuildMock());

            await _service.UpdateStatusAsync(1, 1, false);

            _mockEruleRepo.Verify(u => u.Update(rule), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveMultiple_ShouldDeleteNonInUseRules()
        {
            var rules = new List<Erule> 
            { 
                new Erule { EruleId = 1, TenantId = 1 },
                new Erule { EruleId = 2, TenantId = 1 }
            }.AsQueryable().BuildMock();

            _mockEruleRepo.Setup(u => u.Query()).Returns(rules);
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard>().AsQueryable().BuildMock());

            var result = await _service.RemoveMultiple(1, new List<int> { 1, 2 });

            Assert.Contains("Rules Deleted successfully", result);
            _mockEruleRepo.Verify(u => u.Remove(It.IsAny<Erule>()), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateErule_ShouldCreateNewVersion()
        {
            var master = new EruleMaster { Id = 1, EruleName = "R1", TenantId = 1 };
            var existing = new List<Erule> { new Erule { EruleId = 10, EruleMasterId = 1, Version = 1, TenantId = 1 } }.AsQueryable().BuildMock();

            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { master }.AsQueryable().BuildMock());
            _mockEruleRepo.Setup(u => u.Query()).Returns(existing);
            
            var model = new EruleCreateOrUpdateModel { EruleId = 10, TenantId = 1, EruleMasterId = 1, EruleName = "R1", Expression = "NEW", IsPublished = true };
            _mockMapper.Setup(m => m.Map<Erule>(model)).Returns(new Erule { EruleMasterId = 1, Expression = "NEW" });

            await _service.UpdateErule(model);

            _mockEruleRepo.Verify(u => u.Add(It.Is<Erule>(e => e.Version == 2), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task PublishDraftAsync_ShouldPromoteVersion()
        {
            var draft = new Erule { EruleId = 1, TenantId = 1, Version = 0, EruleMasterId = 10, IsPublished = true, Expression = "EXP" };
            var existing = new List<Erule> 
            { 
                new Erule { EruleId = 2, TenantId = 1, Version = 1, EruleMasterId = 10 } 
            }.AsQueryable().BuildMock();

            var data = new List<Erule> { draft };
            data.AddRange(existing);
            _mockEruleRepo.Setup(u => u.Query()).Returns(data.AsQueryable().BuildMock());

            await _service.PublishDraftAsync(1, 1);

            Assert.Equal(2, draft.Version);
            Assert.False(draft.IsPublished);
            _mockEruleRepo.Verify(u => u.Update(draft), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllByEruleMasterId_ShouldReturnList()
        {
            var data = new List<Erule> { new Erule { EruleMasterId = 1, TenantId = 1 } }.AsQueryable().BuildMock();
            _mockEruleRepo.Setup(u => u.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<List<EruleListModel>>(It.IsAny<List<Erule>>())).Returns(new List<EruleListModel> { new() { Expression = "EXP" } });

            var result = await _service.GetAllByEruleMasterId(1, 1);

            Assert.Single(result);
        }

        [Fact]
        public async Task ImportErule_DuplicateRecords_ShouldIncrementCounter()
        {
            var master = new EruleMaster { Id = 1, EruleName = "R1", TenantId = 1 };
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { master }.AsQueryable().BuildMock());
            
            var existing = new List<Erule> { new Erule { TenantId = 1, EruleMasterId = 1, Expression = "DUPE" } }.AsQueryable().BuildMock();
            _mockEruleRepo.Setup(u => u.Query()).Returns(existing);

            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Rules");
                ws.Cells[1, 1].Value = "RuleName";
                ws.Cells[1, 2].Value = "Description";
                ws.Cells[1, 9].Value = "Expression";
                ws.Cells[2, 1].Value = "R1";
                ws.Cells[2, 2].Value = "DUPE";
                ws.Cells[2, 9].Value = "DUPE";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportErule(1, stream, "user");
            Assert.Contains("1 duplicates skipped", result);
        }

        [Fact]
        public async Task RemoveMultiple_NoIds_ShouldReturnNoDeletedMessage()
        {
            var result = await _service.RemoveMultiple(1, new List<int>());
            Assert.Equal("No rules were deleted.", result);
        }

        [Fact]
        public async Task ImportErule_EmptyFile_ShouldReturnError()
        {
            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Empty");
                // Add something to dimension to avoid NullRef in GetRowCount if it depends on it
                ws.Cells[1, 1].Value = "Header"; 
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportErule(1, stream, "user");
            Assert.Equal("Uploaded File Is Empty", result);
        }

        [Fact]
        public async Task DownloadTemplateEruleMaster_ShouldReturnBytes()
        {
            _mockParameterService.Setup(s => s.GetAll(1)).ReturnsAsync(new List<ParameterListModel>());
            _mockConditionService.Setup(s => s.GetAll()).Returns(new List<ConditionModel>());
            _mockFactorService.Setup(s => s.GetAll(1)).ReturnsAsync(new List<FactorListModel>());

            var result = await _service.DownloadTemplateEruleMaster(1);
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task UpdateErule_OriginalNotFound_ShouldThrowException()
        {
            _mockEruleRepo.Setup(u => u.Query()).Returns(new List<Erule>().AsQueryable().BuildMock());
            var model = new EruleCreateOrUpdateModel { EruleId = 999, TenantId = 1, Expression = "EXP", EruleName = "R1" };

            await Assert.ThrowsAsync<Exception>(() => _service.UpdateErule(model));
        }

        [Fact]
        public async Task DownloadTemplate_WithData_ShouldReturnBytes()
        {
            var parameters = new List<ParameterListModel> { new() { ParameterId = 1, ParameterName = "P1" } };
            var conditions = new List<ConditionModel> { new() { ConditionValue = "C1" } };
            var factors = new List<FactorListModel> { new() { ParameterId = 1, Value1 = "V1", Value2 = "V2" }, new() { ParameterId = 1, Value1 = "V3", Value2 = "" } };

            _mockParameterService.Setup(s => s.GetAll(1)).ReturnsAsync(parameters);
            _mockConditionService.Setup(s => s.GetAll()).Returns(conditions);
            _mockFactorService.Setup(s => s.GetAll(1)).ReturnsAsync(factors);

            var result = await _service.DownloadTemplate(1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ExportErule_WithSelection_ShouldReturnStream()
        {
            var request = new ExportRequestModel { SelectedIds = new List<int> { 1 } };
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { new EruleMaster { Id = 1, EruleName = "N", TenantId = 1 } }.AsQueryable().BuildMock());
            _mockExportService.Setup(s => s.ExportToExcel(It.IsAny<List<RuleExportRow>>(), It.IsAny<string>(), null)).ReturnsAsync(new MemoryStream());

            var result = await _service.ExportErule(1, request);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ExportErule_WithSearch_ShouldReturnStream()
        {
            var request = new ExportRequestModel { SearchTerm = "search" };
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster> { new EruleMaster { Id = 1, EruleName = "search", TenantId = 1 } }.AsQueryable().BuildMock());
            _mockExportService.Setup(s => s.ExportToExcel(It.IsAny<List<RuleExportRow>>(), It.IsAny<string>(), null)).ReturnsAsync(new MemoryStream());

            var result = await _service.ExportErule(1, request);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ImportEruleMaster_IncorrectHeader_ShouldReturnErrorMessage()
        {
            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Master");
                ws.Cells[1, 1].Value = "WrongHeader";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportEruleMaster(1, stream, "user");
            Assert.Contains("Incorrect file format", result);
        }

        [Fact]
        public async Task ImportEruleMaster_VariousIsActive_ShouldSucceed()
        {
            _mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster>().AsQueryable().BuildMock());
            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Master");
                ws.Cells[1, 1].Value = "RuleName*"; ws.Cells[1, 2].Value = "RuleDescription"; ws.Cells[1, 3].Value = "IsActive*";
                ws.Cells[2, 1].Value = "R1"; ws.Cells[2, 2].Value = "D1"; ws.Cells[2, 3].Value = "yes";
                ws.Cells[3, 1].Value = "R2"; ws.Cells[3, 2].Value = "D2"; ws.Cells[3, 3].Value = "1";
                ws.Cells[4, 1].Value = "R3"; ws.Cells[4, 2].Value = "D3"; ws.Cells[4, 3].Value = "no";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportEruleMaster(1, stream, "user");
            Assert.Contains("3 Created successfully", result);
        }

        [Fact]
        public async Task ImportEruleMaster_RepoThrows_ShouldReturnErrorMessage()
        {
            _mockEruleMasterRepo.Setup(u => u.Query()).Throws(new Exception("DB Error"));
            using var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var ws = package.Workbook.Worksheets.Add("Master");
                ws.Cells[1, 1].Value = "RuleName*"; ws.Cells[1, 2].Value = "RuleDescription"; ws.Cells[1, 3].Value = "IsActive*";
                ws.Cells[2, 1].Value = "R1"; ws.Cells[2, 2].Value = "D1"; ws.Cells[2, 3].Value = "True";
                package.Save();
            }
            stream.Position = 0;

            var result = await _service.ImportEruleMaster(1, stream, "user");
            Assert.Contains("DB Error", result);
        }

        [Fact]
        public async Task UpdateStatusAsync_ValidModel_ShouldUpdateTimestamp()
        {
            var erule = new Erule { EruleId = 1, TenantId = 1 };
            _mockEruleRepo.Setup(u => u.Query()).Returns(new List<Erule> { erule }.AsQueryable().BuildMock());

            await _service.UpdateStatusAsync(1, 1, false);

            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ValidModel_ShouldUpdate()
        {
            var model = new EruleUpdateModel { EruleId = 1, TenantId = 1, EruleName = "N", Expression = "E" };
            var erule = new Erule { EruleId = 1, TenantId = 1 };
            _mockEruleRepo.Setup(u => u.Query()).Returns(new List<Erule> { erule }.AsQueryable().BuildMock());
            _mockMapper.Setup(m => m.Map(model, erule)).Returns(erule);

            await _service.Update(model);

            _mockEruleRepo.Verify(u => u.Update(It.IsAny<Erule>()), Times.Once);
        }
    }
}
