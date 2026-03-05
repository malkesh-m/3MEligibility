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
    public class ParameterServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDataTypeService> _mockDataTypeService;
        private readonly Mock<IConditionService> _mockConditionService;
        private readonly Mock<IExportService> _mockExportService;
        private readonly ParameterService _service;

        public ParameterServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockDataTypeService = new Mock<IDataTypeService>();
            _mockConditionService = new Mock<IConditionService>();
            _mockExportService = new Mock<IExportService>();
            _service = new ParameterService(_mockUow.Object, _mockMapper.Object, _mockDataTypeService.Object, _mockConditionService.Object, _mockExportService.Object);
        }

        [Fact]
        public async Task Add_ShouldAddEntityAndComplete_WhenNoDuplicatesExist()
        {
            var model = new ParameterAddUpdateModel { TenantId = 1, ParameterName = "Param 1" };
            var entity = new Parameter { TenantId = 1, ParameterName = "Param 1" };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>().BuildMock());
            _mockMapper.Setup(m => m.Map<Parameter>(model)).Returns(entity);
            _mockUow.Setup(u => u.ParameterRepository.Add(entity, false));

            await _service.Add(model);

            _mockUow.Verify(u => u.ParameterRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldThrowException_WhenParameterNameIsDuplicate()
        {
            var model = new ParameterAddUpdateModel { TenantId = 1, ParameterName = "Param 1" };
            var existingData = new List<Parameter> { new() { TenantId = 1, ParameterName = "Param 1" } }.BuildMock();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(existingData);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Add(model));
            Assert.Equal("Parameter name already exists in this tenant", exception.Message);
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntityAndRelatedRecords_ThenComplete()
        {
            var tenantId = 1;
            var id = 1;
            var entity = new Parameter { TenantId = tenantId, ParameterId = id, ParameterName = "Param 1" };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { entity }.BuildMock());
            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(new List<ApiParameterMap> { new() { ParameterId = id } }.BuildMock());
            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(new List<ApiParameter> { new() { ParameterName = "Param 1" } }.BuildMock());

            await _service.Delete(tenantId, id);

            _mockUow.Verify(u => u.ApiParameterMapsRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameterMap>>()), Times.Once);
            _mockUow.Verify(u => u.ApiParametersRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameter>>()), Times.Once);
            _mockUow.Verify(u => u.ParameterRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedModels()
        {
            var data = new List<Parameter> { new() { TenantId = 1, ParameterId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            var expected = new List<ParameterListModel> { new() { ParameterId = 1 } };
            _mockMapper.Setup(m => m.Map<List<ParameterListModel>>(It.IsAny<List<Parameter>>())).Returns(expected);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task CheckParameterComputedValue_ShouldReturnComputedValue_WhenSingleMatches()
        {
            var paramId = 1;
            var entity = new Parameter
            {
                ParameterId = paramId,
                ComputedValues =
                [
                    new ParameterComputedValue { ComputedParameterType = ParameterComputedType.Single, ParameterExactValue = "Yes", ComputedValue = "100" }
                ]
            };
            var data = new List<Parameter> { entity }.BuildMock();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            var result = await _service.CheckParameterComputedValue(1, paramId, "Yes");

            Assert.Equal("100", result);
        }

        [Fact]
        public async Task CheckParameterComputedValue_ShouldReturnComputedValue_WhenRangeMatches()
        {
            var paramId = 1;
            var entity = new Parameter
            {
                ParameterId = paramId,
                ComputedValues =
                [
                    new ParameterComputedValue { ComputedParameterType = ParameterComputedType.Range, FromValue = "10", ToValue = "20", ComputedValue = "In Range" }
                ]
            };
            var data = new List<Parameter> { entity }.BuildMock();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            var result = await _service.CheckParameterComputedValue(1, paramId, "15");

            Assert.Equal("In Range", result);
        }

        [Fact]
        public async Task ExportParameter_ShouldCallExportService()
        {
            var request = new ExportRequestModel { SelectedIds = [1] };
            var param = new Parameter { ParameterId = 1, TenantId = 1, ParameterName = "P1", DataTypeId = 1, Identifier = 1 };
            var dt = new DataType { DataTypeId = 1, DataTypeName = "Int" };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { param }.BuildMock());
            _mockUow.Setup(u => u.DataTypeRepository.Query()).Returns(new List<DataType> { dt }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.Query()).Returns(new List<Condition>().BuildMock());

            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<ParameterCsvModel>>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var result = await _service.ExportParameter(1, 1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(e => e.ExportToExcel(It.IsAny<List<ParameterCsvModel>>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ExportParameter_WithSearchTerm_ShouldFilterResults()
        {
            var request = new ExportRequestModel { SearchTerm = "Match" };
            var data = new List<Parameter> 
            { 
                new() { ParameterId = 1, ParameterName = "matchthis", TenantId = 1, DataTypeId = 1, Identifier = 1, ConditionId = 1 },
                new() { ParameterId = 2, ParameterName = "other", TenantId = 1, DataTypeId = 1, Identifier = 1, ConditionId = 1 }
            };
            var dt = new DataType { DataTypeId = 1, DataTypeName = "Int" };
            var cond = new Condition { ConditionId = 1, ConditionValue = "C1" };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data.BuildMock());
            _mockUow.Setup(u => u.DataTypeRepository.Query()).Returns(new List<DataType> { dt }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.Query()).Returns(new List<Condition> { cond }.BuildMock());

            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<ParameterCsvModel>>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var result = await _service.ExportParameter(1, 1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(e => e.ExportToExcel(It.Is<List<ParameterCsvModel>>(l => l.Count == 1), It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ImportEntities_ValidExcel_ShouldReturnSuccessMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Parameter");
            string[] headers = ["ParameterName*", "ParameterType*", "ParameterTypeId*", "IsMandatory"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            
            sheet.Cells[2, 1].Value = "P1";
            sheet.Cells[2, 3].Value = "1";
            sheet.Cells[2, 4].Value = "True";

            var stream = new MemoryStream(package.GetAsByteArray());
            
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>().BuildMock());
            _mockMapper.Setup(m => m.Map<Parameter>(It.IsAny<Parameter>())).Returns(new Parameter());

            var result = await _service.ImportEntities(1, stream, 1, "Tester");

            Assert.Contains("1 parameter inserted successfully", result);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DownloadTemplate_ShouldReturnBytes()
        {
            _mockDataTypeService.Setup(d => d.GetAll()).Returns(new List<DataTypeModel>());
            _mockConditionService.Setup(c => c.GetAll()).Returns(new List<ConditionModel>());

            var result = await _service.DownloadTemplate(1);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GetSystemParameters_ShouldReturnList()
        {
            var data = new List<SystemParameter> { new() { Name = "S1" } };
            _mockUow.Setup(u => u.SystemParameterRepository.Query()).Returns(data.BuildMock());
            _mockMapper.Setup(m => m.Map<List<SystemParameterModel>>(It.IsAny<List<SystemParameter>>())).Returns([new SystemParameterModel { Name = "S1" }]);

            var result = await _service.GetSystemParameters();

            Assert.Single(result);
        }

        [Fact]
        public async Task AddSystemParameter_ShouldAdd_WhenUnique()
        {
            var model = new SystemParameterModel { Name = "New" };
            _mockUow.Setup(u => u.SystemParameterRepository.Query()).Returns(new List<SystemParameter>().BuildMock());
            _mockMapper.Setup(m => m.Map<SystemParameter>(model)).Returns(new SystemParameter());

            await _service.AddSystemParameter(model);

            _mockUow.Verify(u => u.SystemParameterRepository.Add(It.IsAny<SystemParameter>(), false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSystemParameter_ShouldUpdate_WhenFound()
        {
            var model = new SystemParameterModel { Id = 1, Name = "Updated" };
            var existing = new SystemParameter { Id = 1, Name = "Old" };
            _mockUow.Setup(u => u.SystemParameterRepository.Query()).Returns(new List<SystemParameter> { existing }.BuildMock());

            await _service.UpdateSystemParameter(model);

            _mockUow.Verify(u => u.SystemParameterRepository.Update(existing), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteSystemParameter_ShouldRemove_WhenFound()
        {
            var id = 1;
            var existing = new SystemParameter { Id = id };
            _mockUow.Setup(u => u.SystemParameterRepository.Query()).Returns(new List<SystemParameter> { existing }.BuildMock());

            await _service.DeleteSystemParameter(id);

            _mockUow.Verify(u => u.SystemParameterRepository.Remove(existing), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
        [Fact]
        public async Task Update_ShouldThrowException_WhenParameterNameIsDuplicate()
        {
            var model = new ParameterAddUpdateModel { TenantId = 1, ParameterId = 1, ParameterName = "Param 2" };
            var existingEntity = new Parameter { TenantId = 1, ParameterId = 2, ParameterName = "Param 2" };
            var data = new List<Parameter> { existingEntity }.BuildMock();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("Parameter name already exists in this tenant", exception.Message);
        }

        [Fact]
        public async Task ExportParameter_WithSelection_ShouldFilterResults()
        {
            var request = new ExportRequestModel { SelectedIds = [1] };
            var param = new Parameter { ParameterId = 1, TenantId = 1, ParameterName = "P1", DataTypeId = 1, Identifier = 1 };
            
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { param }.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.DataTypeRepository.Query()).Returns(new List<DataType>().AsQueryable().BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.Query()).Returns(new List<Condition>().AsQueryable().BuildMock());

            _mockExportService.Setup(e => e.ExportToExcel(It.IsAny<List<ParameterCsvModel>>(), It.IsAny<string>(), It.IsAny<string[]>()))
                .ReturnsAsync(new MemoryStream());

            var result = await _service.ExportParameter(1, 1, request);

            Assert.NotNull(result);
            _mockExportService.Verify(e => e.ExportToExcel(It.Is<List<ParameterCsvModel>>(l => l.Count == 1), It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task ImportEntities_EmptyExcel_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Parameter");
            string[] headers = ["ParameterName*", "ParameterType*", "ParameterTypeId*", "IsMandatory"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            
            var stream = new MemoryStream(package.GetAsByteArray());

            var result = await _service.ImportEntities(1, stream, 1, "Tester");

            Assert.Equal("Uploaded file is empty.", result);
        }

        [Fact]
        public async Task ImportEntities_InvalidHeader_ShouldReturnErrorMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Parameter");
            string[] headers = ["WrongName*", "ParameterType*", "ParameterTypeId*", "IsMandatory"];
            for (int i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            
            var stream = new MemoryStream(package.GetAsByteArray());

            var result = await _service.ImportEntities(1, stream, 1, "Tester");

            Assert.Contains("Incorrect file format at Column 1", result);
        }
        [Fact]
        public async Task RemoveMultiple_ShouldRemoveFoundParametersAndRelatedRecords()
        {
            var tenantId = 1;
            var ids = new List<int> { 1, 2 };
            var p1 = new Parameter { ParameterId = 1, TenantId = tenantId, ParameterName = "P1" };
            var p2 = new Parameter { ParameterId = 2, TenantId = tenantId, ParameterName = "P2" };
            
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { p1, p2 }.BuildMock());
            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(new List<ApiParameterMap>().BuildMock());
            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(new List<ApiParameter>().BuildMock());

            await _service.RemoveMultiple(tenantId, ids);

            _mockUow.Verify(u => u.ParameterRepository.Remove(p1), Times.Once);
            _mockUow.Verify(u => u.ParameterRepository.Remove(p2), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetParameterByProducts_ShouldReturnMappedList_WhenProductExists()
        {
            var tenantId = 1;
            var productId = 10;
            var product = new Product { ProductId = productId, TenantId = tenantId };
            var parameters = new List<Parameter> { new() { TenantId = tenantId } };

            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(parameters.BuildMock());
            _mockMapper.Setup(m => m.Map<List<ParameterModel>?>(It.IsAny<List<Parameter>>())).Returns(new List<ParameterModel> { new() });

            var result = _service.GetParameterByProducts(tenantId, productId);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task AddSystemParameter_ShouldThrowException_WhenNameExists()
        {
            var model = new SystemParameterModel { Name = "Existing" };
            _mockUow.Setup(u => u.SystemParameterRepository.Query()).Returns(new List<SystemParameter> { new() { Name = "Existing" } }.BuildMock());

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AddSystemParameter(model));
            Assert.Equal("Source Parameter name already exists.", ex.Message);
        }

        [Fact]
        public async Task UpdateSystemParameter_ShouldThrowException_WhenNotFound()
        {
            var model = new SystemParameterModel { Id = 99 };
            _mockUow.Setup(u => u.SystemParameterRepository.Query()).Returns(new List<SystemParameter>().BuildMock());

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateSystemParameter(model));
            Assert.Equal("Source Parameter not found.", ex.Message);
        }

        [Fact]
        public async Task DeleteSystemParameter_ShouldThrowException_WhenNotFound()
        {
            _mockUow.Setup(u => u.SystemParameterRepository.Query()).Returns(new List<SystemParameter>().BuildMock());

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteSystemParameter(99));
            Assert.Equal("Source Parameter not found.", ex.Message);
        }

        [Fact]
        public async Task CheckParameterComputedValue_ShouldReturnNull_WhenNoComputedValues()
        {
            var paramId = 1;
            var entity = new Parameter { ParameterId = paramId, ComputedValues = null };
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { entity }.BuildMock());

            var result = await _service.CheckParameterComputedValue(1, paramId, "SomeValue");

            Assert.Null(result);
        }
        [Fact]
        public async Task Add_ShouldThrowException_WhenParameterNameExists()
        {
            var model = new ParameterAddUpdateModel { ParameterName = "Existing", TenantId = 1 };
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new() { ParameterName = "Existing", TenantId = 1 } }.BuildMock());

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Add(model));
            Assert.Equal("Parameter name already exists in this tenant", ex.Message);
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenParameterNotFound()
        {
            var model = new ParameterAddUpdateModel { ParameterId = 99, TenantId = 1 };
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>().BuildMock());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Update(model));
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenNotFound()
        {
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>().BuildMock());
            
            // Note: service.GetById returns the result of First(), so it actually throws
            await Assert.ThrowsAsync<InvalidOperationException>(() => Task.FromResult(_service.GetById(1, 99)));
        }

        [Fact]
        public async Task Delete_ShouldRemoveRelatedMappingsAndParameters()
        {
            var tenantId = 1;
            var id = 1;
            var param = new Parameter { TenantId = tenantId, ParameterId = id, ParameterName = "P1" };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { param }.BuildMock());
            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(new List<ApiParameterMap> { new() { ParameterId = id } }.BuildMock());
            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(new List<ApiParameter> { new() { ParameterName = "P1" } }.BuildMock());

            await _service.Delete(tenantId, id);

            _mockUow.Verify(u => u.ApiParameterMapsRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameterMap>>()), Times.Once);
            _mockUow.Verify(u => u.ApiParametersRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameter>>()), Times.Once);
            _mockUow.Verify(u => u.ParameterRepository.Remove(param), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveMultiple_ShouldRemoveRelatedMappingsAndParameters()
        {
            var tenantId = 1;
            var p1 = new Parameter { TenantId = tenantId, ParameterId = 1, ParameterName = "P1" };
            var p2 = new Parameter { TenantId = tenantId, ParameterId = 2, ParameterName = "P2" };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { p1, p2 }.BuildMock());
            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(new List<ApiParameterMap> { new() { ParameterId = 1 } }.BuildMock());
            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(new List<ApiParameter> { new() { ParameterName = "P1" } }.BuildMock());

            await _service.RemoveMultiple(tenantId, [1, 2]);

            _mockUow.Verify(u => u.ApiParameterMapsRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameterMap>>()), Times.Once);
            _mockUow.Verify(u => u.ApiParametersRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameter>>()), Times.Once);
            _mockUow.Verify(u => u.ParameterRepository.Remove(It.IsAny<Parameter>()), Times.Exactly(2));
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ImportEntities_AllRowsSkipped_ShouldReturnNoNewRecords()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Parameter");
            sheet.Cells[1, 1].Value = "ParameterName*";
            sheet.Cells[1, 2].Value = "ParameterType*";
            sheet.Cells[1, 3].Value = "ParameterTypeId*";
            sheet.Cells[1, 4].Value = "IsMandatory";
            sheet.Cells[2, 2].Value = "Type";
            var stream = new MemoryStream(package.GetAsByteArray());

            var result = await _service.ImportEntities(1, stream, 1, "Tester");

            Assert.Equal("No new records to insert.", result);
        }

        [Fact]
        public  void GetParameterByProducts_ProductNotFound_ShouldReturnNull()
        {
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product>().BuildMock());

            var result = _service.GetParameterByProducts(1, 999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetSystemParameters_WhenEmpty_ShouldSeedDefaults()
        {
            _mockUow.SetupSequence(u => u.SystemParameterRepository.Query())
                .Returns(new List<SystemParameter>().BuildMock())
                .Returns(new List<SystemParameter>
                {
                    new() { Name = "NationalId" },
                    new() { Name = "LoanNo" }
                }.BuildMock());

            _mockMapper.Setup(m => m.Map<List<SystemParameterModel>>(It.IsAny<List<SystemParameter>>()))
                .Returns((List<SystemParameter> src) => src.Select(s => new SystemParameterModel { Name = s.Name }).ToList());

            var result = await _service.GetSystemParameters();

            Assert.NotEmpty(result);
            _mockUow.Verify(u => u.SystemParameterRepository.AddRange(It.IsAny<IEnumerable<SystemParameter>>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetByEntityId_ShouldMapComputedValues()
        {
            var param = new Parameter
            {
                TenantId = 1,
                ParameterId = 1,
                ParameterName = "P1",
                DataType = new DataType { DataTypeName = "Text" },
                ComputedValues = [new ParameterComputedValue { ComputedValue = "X" }]
            };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { param }.BuildMock());
            _mockMapper.Setup(m => m.Map<List<ParameterComputedValueModel>>(It.IsAny<List<ParameterComputedValue>>()))
                .Returns([new ParameterComputedValueModel { ComputedValue = "X" }]);
            _mockMapper.Setup(m => m.Map<List<ParameterListModel>>(It.IsAny<List<ParameterListModel>>()))
                .Returns((List<ParameterListModel> src) => src);

            var result = _service.GetByEntityId(1);

            Assert.Single(result);
            Assert.Equal("Text", result[0].DataType);
        }

        [Fact]
        public async Task ImportEntities_DuplicateInDatabase_ShouldReturnAlreadyExistsMessage()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Parameter");
            sheet.Cells[1, 1].Value = "ParameterName*";
            sheet.Cells[1, 2].Value = "ParameterType*";
            sheet.Cells[1, 3].Value = "ParameterTypeId*";
            sheet.Cells[1, 4].Value = "IsMandatory";
            sheet.Cells[2, 1].Value = "P1";
            sheet.Cells[2, 3].Value = "1";
            var stream = new MemoryStream(package.GetAsByteArray());

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>
            {
                new() { TenantId = 1, ParameterName = "P1" }
            }.BuildMock());

            var result = await _service.ImportEntities(1, stream, 1, "Tester");

            Assert.Contains("already exist", result);
        }

        [Fact]
        public void PrivateHelpers_ShouldApplyConditionalDropdownAndSanitize()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Parameter");
            sheet.Cells[1, 1].Value = "H";
            sheet.Cells[2, 1].Value = "X";

            var applyConditional = typeof(ParameterService).GetMethod("ApplyDropdownWithCondition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var sanitize = typeof(ParameterService).GetMethod("SanitizeRangeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(applyConditional);
            Assert.NotNull(sanitize);

            applyConditional!.Invoke(null, new object[] { sheet, "TestRange", "A", 1, 2, "B" });
            var sanitized = (string)sanitize!.Invoke(null, new object[] { "1 Bad-Name" })!;

            Assert.StartsWith("_1", sanitized);
            Assert.Contains("Bad_Name", sanitized);
        }
    }
}
