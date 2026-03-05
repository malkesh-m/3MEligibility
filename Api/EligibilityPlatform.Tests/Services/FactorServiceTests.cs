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
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;
using OfficeOpenXml;
using System.Text;

namespace EligibilityPlatform.Tests.Services
{
    public class FactorServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IParameterService> _mockParameterService;
        private readonly Mock<IConditionService> _mockConditionService;
        private readonly Mock<IManagedListService> _mockManagedListService;
        private readonly Mock<IExportService> _mockExportService;
        private readonly FactorService _service;

        public FactorServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockParameterService = new Mock<IParameterService>();
            _mockConditionService = new Mock<IConditionService>();
            _mockManagedListService = new Mock<IManagedListService>();
            _mockExportService = new Mock<IExportService>();

            _service = new FactorService(
                _mockUow.Object,
                _mockMapper.Object,
                _mockParameterService.Object,
                _mockConditionService.Object,
                _mockManagedListService.Object,
                _mockExportService.Object);
        }

        [Fact]
        public async Task Add_ShouldAddEntity_WhenNoDuplicateExists()
        {
            var model = new FactorAddUpdateModel { TenantId = 1, ParameterId = 1, ConditionId = 1, Value1 = "V1" };
            var entity = new Factor { TenantId = 1, ParameterId = 1, ConditionId = 1, Value1 = "V1" };

            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor>().BuildMock());
            _mockMapper.Setup(m => m.Map<Factor>(model)).Returns(entity);

            await _service.Add(model);

            _mockUow.Verify(u => u.FactorRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldThrowException_WhenDuplicateExists()
        {
            var model = new FactorAddUpdateModel { TenantId = 1, ParameterId = 1, ConditionId = 1, Value1 = "V1" };
            var existing = new List<Factor> { new() { TenantId = 1, ParameterId = 1, ConditionId = 1, Value1 = "V1" } }.BuildMock();

            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(existing);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Add(model));
            Assert.Equal("Duplicate Record already exist", ex.Message);
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntity_WhenFound()
        {
            var entity = new Factor { FactorId = 1, TenantId = 1 };
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { entity }.BuildMock());

            await _service.Delete(1, 1);

            _mockUow.Verify(u => u.FactorRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedModels()
        {
            var data = new List<Factor> { new() { TenantId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(data);
            
            var mapped = new List<FactorListModel> { new() };
            _mockMapper.Setup(m => m.Map<List<FactorListModel>>(It.IsAny<List<Factor>>())).Returns(mapped);

            var result = await _service.GetAll(1);

            Assert.Single(result);
        }

        [Fact]
        public void GetById_ShouldReturnMappedModel()
        {
            var entity = new Factor { FactorId = 1, TenantId = 1 };
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { entity }.BuildMock());
            
            var mapped = new FactorListModel { FactorId = 1 };
            _mockMapper.Setup(m => m.Map<FactorListModel>(entity)).Returns(mapped);

            var result = _service.GetById(1, 1);

            Assert.Equal(1, result.FactorId);
        }

        [Fact]
        public async Task Update_ShouldUpdate_WhenNoConflict()
        {
            var model = new FactorAddUpdateModel { FactorId = 1, TenantId = 1, ParameterId = 1, ConditionId = 1, Value1 = "V2" };
            var existing = new Factor { FactorId = 1, TenantId = 1, ParameterId = 1, ConditionId = 1, Value1 = "V1" };

            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { existing }.BuildMock());
            _mockMapper.Setup(m => m.Map<FactorModel, Factor>(model, existing)).Returns(existing);

            await _service.Update(model);

            _mockUow.Verify(u => u.FactorRepository.Update(existing), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveMultiple_ShouldRemoveAll()
        {
            var ids = new List<int> { 1, 2 };
            var e1 = new Factor { FactorId = 1, TenantId = 1 };
            var e2 = new Factor { FactorId = 2, TenantId = 1 };
            
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { e1, e2 }.BuildMock());

            await _service.RemoveMultiple(1, ids);

            _mockUow.Verify(u => u.FactorRepository.Remove(e1), Times.Once);
            _mockUow.Verify(u => u.FactorRepository.Remove(e2), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetValueByParams_ShouldReturnFormattedStrings()
        {
            var data = new List<Factor> 
            { 
                new() { TenantId = 1, ParameterId = 1, Value1 = "A", Value2 = "B" },
                new() { TenantId = 1, ParameterId = 1, Value1 = "C", Value2 = null }
            }.BuildMock();

            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(data);

            var result = _service.GetValueByParams(1, 1);

            Assert.Equal(2, result.Count);
            Assert.Contains("A - B", result);
            Assert.Contains("C", result);
        }

        [Fact]
        public void GetFactorByCondition_ShouldReturnMappedModels()
        {
            var data = new List<Factor> { new() { TenantId = 1, ConditionId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(data);
            
            _mockMapper.Setup(m => m.Map<List<FactorModel>>(It.IsAny<List<Factor>>())).Returns([new FactorModel()]);

            var result = _service.GetFactorByCondition(1, 1);

            Assert.Single(result);
        }

        [Fact]
        public async Task ExportFactors_WithSearchTerm_ShouldFilterResults()
        {
            var request = new ExportRequestModel { SearchTerm = "Match" };
            var data = new List<Factor> 
            { 
                new() { FactorId = 1, FactorName = "matchthis", TenantId = 1, ParameterId = 1, ConditionId = 1 },
                new() { FactorId = 2, FactorName = "other", TenantId = 1, ParameterId = 1, ConditionId = 1 }
            };
            var param = new Parameter { ParameterId = 1, TenantId = 1, ParameterName = "P1" };
            var cond = new Condition { ConditionId = 1, ConditionValue = "C1" };

            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(data.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { param }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.Query()).Returns(new List<Condition> { cond }.BuildMock());
            _mockMapper.Setup(m => m.Map<List<FactorModelDescription>>(It.IsAny<List<FactorModelDescription>>()))
                .Returns((List<FactorModelDescription> src) => src);

            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<FactorModelDescription>>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var result = await _service.ExportFactors(1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(e => e.ExportToExcel(It.Is<List<FactorModelDescription>>(l => l.Count == 1), It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ImportEntities_ShouldAddMappedEntities()
        {
            var csvContent = "FactorName,ParameterId,ConditionId,Value1,Value2,Note,FactorId,TenantId\nf1,1,1,v1,v2,n1,0,1";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(csvContent);
            writer.Flush();
            stream.Position = 0;
            
            _mockMapper.Setup(m => m.Map<Factor>(It.IsAny<FactorModel>())).Returns(new Factor());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor>().BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Add(It.IsAny<Factor>(), It.IsAny<bool>()));
            
            await _service.ImportEntities(1, stream);

            _mockUow.Verify(u => u.FactorRepository.Add(It.IsAny<Factor>(), false), Times.AtLeastOnce);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ImportFactor_ValidExcel_ShouldReturnSuccessMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Factors");
            string[] headers = ["factorName*", "parameter*", "parameterId*", "condition*", "conditionId*", "value1*", "value2"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            
            sheet.Cells[2, 1].Value = "F1";
            sheet.Cells[2, 3].Value = "1";
            sheet.Cells[2, 5].Value = "1";
            sheet.Cells[2, 6].Value = "V1";

            var stream = new MemoryStream(package.GetAsByteArray());
            
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor>().BuildMock());
            _mockMapper.Setup(m => m.Map<Factor>(It.IsAny<Factor>())).Returns(new Factor());

            var result = await _service.ImportFactor(1, stream, "Tester");

            Assert.Contains("1 Created successfully.", result);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ImportFactor_InvalidHeader_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Factors");
            sheet.Cells[1, 1].Value = "WrongHeader";

            var stream = new MemoryStream(package.GetAsByteArray());
            var result = await _service.ImportFactor(1, stream, "Tester");

            Assert.Contains("Incorrect file format", result);
        }

        [Fact]
        public async Task ImportFactor_EmptyFile_ShouldReturnEmptyMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Factors");
            string[] headers = ["factorName*", "parameter*", "parameterId*", "condition*", "conditionId*", "value1*", "value2"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];

            var stream = new MemoryStream(package.GetAsByteArray());
            var result = await _service.ImportFactor(1, stream, "Tester");

            Assert.Equal("Uploaded File Is Empty", result);
        }

        [Fact]
        public async Task DownloadTemplate_ShouldReturnBytes()
        {
            _mockParameterService.Setup(p => p.GetAll(1)).ReturnsAsync(new List<ParameterListModel>());
            _mockConditionService.Setup(c => c.GetAll()).Returns(new List<ConditionModel>());
            _mockManagedListService.Setup(m => m.GetAll(1)).ReturnsAsync(new List<ManagedListGetModel>());

            var result = await _service.DownloadTemplate(1);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
        [Fact]
        public async Task Update_ShouldThrowException_WhenDuplicateExists()
        {
            var model = new FactorAddUpdateModel { FactorId = 2, TenantId = 1, ParameterId = 1, ConditionId = 1, Value1 = "V1" };
            var existing = new List<Factor> { new() { FactorId = 1, TenantId = 1, ParameterId = 1, ConditionId = 1, Value1 = "V1" } }.AsQueryable().BuildMock();

            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(existing);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("Duplicate Record already exist", ex.Message);
        }

        [Fact]
        public async Task ExportFactors_WithSelection_ShouldFilterByIds()
        {
            var request = new ExportRequestModel { SelectedIds = [1] };
            var data = new List<Factor> 
            { 
                new() { FactorId = 1, TenantId = 1, ParameterId = 1, ConditionId = 1 },
                new() { FactorId = 2, TenantId = 1, ParameterId = 1, ConditionId = 1 }
            }.AsQueryable().BuildMock();
            
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(data);
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new() { ParameterId = 1, TenantId = 1 } }.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.Query()).Returns(new List<Condition> { new() { ConditionId = 1 } }.AsQueryable().BuildMock());
            _mockMapper.Setup(m => m.Map<List<FactorModelDescription>>(It.IsAny<List<FactorModelDescription>>()))
                .Returns((List<FactorModelDescription> src) => src);
            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<FactorModelDescription>>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            await _service.ExportFactors(1, request);

            _mockExportService.Verify(e => e.ExportToExcel(It.Is<List<FactorModelDescription>>(l => l.Count == 1 && l[0].FactorId == 1), It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ImportFactor_DuplicateRecord_ShouldSkip()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Factors");
            string[] headers = ["factorName*", "parameter*", "parameterId*", "condition*", "conditionId*", "value1*", "value2"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            
            sheet.Cells[2, 1].Value = "F1";
            sheet.Cells[2, 3].Value = "1";
            sheet.Cells[2, 5].Value = "1";
            sheet.Cells[2, 6].Value = "V1";

            var stream = new MemoryStream(package.GetAsByteArray());
            
            // First loop find model. In second loop, existingEntity will be true.
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { new() { FactorName = "F1", ParameterId = 1, ConditionId = 1, Value1 = "V1", TenantId = 1 } }.AsQueryable().BuildMock());

            var result = await _service.ImportFactor(1, stream, "Tester");

            Assert.Contains("0 Created successfully. 1 duplicates skipped", result);
        }

        [Fact]
        public async Task ImportFactor_Condition12_ShouldUseValue2()
        {
             ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
             using var package = new ExcelPackage();
             var sheet = package.Workbook.Worksheets.Add("Factors");
             string[] headers = ["factorName*", "parameter*", "parameterId*", "condition*", "conditionId*", "value1*", "value2"];
             for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
             
             sheet.Cells[2, 1].Value = "F1";
             sheet.Cells[2, 3].Value = "1";
             sheet.Cells[2, 5].Value = "12"; // Special condition
             sheet.Cells[2, 6].Value = "V1";
             sheet.Cells[2, 7].Value = "V2_Expected";

             var stream = new MemoryStream(package.GetAsByteArray());
             _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor>().AsQueryable().BuildMock());
             
             Factor capturedFactor = null!;
             _mockMapper.Setup(m => m.Map<Factor>(It.IsAny<Factor>())).Returns((Factor f) => { capturedFactor = f; return f; });

             await _service.ImportFactor(1, stream, "Tester");

             Assert.Equal("V2_Expected", capturedFactor.Value1);
        }

        [Fact]
        public void GetFactorByparameter_ShouldReturnMappedModels()
        {
            var data = new List<Factor> { new() { TenantId = 1, ParameterId = 1 } }.AsQueryable().BuildMock();
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<List<FactorModel>>(It.IsAny<List<Factor>>())).Returns([new FactorModel()]);

            var result = _service.GetFactorByparameter(1, 1);

            Assert.Single(result);
        }
        [Fact]
        public async Task ImportFactor_Exception_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Factors");
            string[] headers = ["factorName*", "parameter*", "parameterId*", "condition*", "conditionId*", "value1*", "value2"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            
            sheet.Cells[2, 1].Value = "F1";
            sheet.Cells[2, 3].Value = "1";
            sheet.Cells[2, 5].Value = "1";
            sheet.Cells[2, 6].Value = "V1";

            var stream = new MemoryStream(package.GetAsByteArray());
            
            _mockUow.Setup(u => u.FactorRepository.Query()).Throws(new Exception("Database Error"));

            var result = await _service.ImportFactor(1, stream, "Tester");

            Assert.Contains("Error: Database Error", result);
        }

        [Fact]
        public async Task DownloadTemplate_WithInvalidParameterName_ShouldSanitizeName()
        {
            _mockParameterService.Setup(p => p.GetAll(1)).ReturnsAsync(new List<ParameterListModel> { new() { ParameterName = "1 Invalid-Name" } });
            _mockConditionService.Setup(c => c.GetAll()).Returns(new List<ConditionModel>());
            _mockManagedListService.Setup(m => m.GetAll(1)).ReturnsAsync(new List<ManagedListGetModel>());

            var result = await _service.DownloadTemplate(1);

            Assert.NotNull(result);
        }
    }
}

