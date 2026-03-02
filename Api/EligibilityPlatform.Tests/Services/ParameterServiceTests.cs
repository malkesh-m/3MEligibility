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
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

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

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>().AsQueryable());
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
            var existingData = new List<Parameter> { new Parameter { TenantId = 1, ParameterName = "Param 1" } }.AsQueryable();

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
            
            var data = new List<Parameter> { entity }.AsQueryable();
            var apiParameterMapData = new List<ApiParameterMap> { new ApiParameterMap { ParameterId = id } }.AsQueryable();
            var apiParametersData = new List<ApiParameter> { new ApiParameter { ParameterName = "Param 1" } }.AsQueryable();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);
            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(apiParameterMapData);
            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(apiParametersData);
            
            _mockUow.Setup(u => u.ApiParameterMapsRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameterMap>>()));
            _mockUow.Setup(u => u.ApiParametersRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameter>>()));
            _mockUow.Setup(u => u.ParameterRepository.Remove(entity));

            await _service.Delete(tenantId, id);

            _mockUow.Verify(u => u.ApiParameterMapsRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameterMap>>()), Times.Once);
            _mockUow.Verify(u => u.ApiParametersRepository.RemoveRange(It.IsAny<IEnumerable<ApiParameter>>()), Times.Once);
            _mockUow.Verify(u => u.ParameterRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedModels()
        {
            var data = new List<Parameter> { new Parameter { TenantId = 1, ParameterId = 1 } }.BuildMock();
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            var expected = new List<ParameterListModel> { new ParameterListModel { ParameterId = 1 } };
            _mockMapper.Setup(m => m.Map<List<ParameterListModel>>(It.IsAny<List<Parameter>>())).Returns(expected);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void GetByEntityId_ShouldReturnMappedModels()
        {
            var data = new List<Parameter> 
            { 
                new Parameter 
                { 
                    TenantId = 1, 
                    ParameterId = 1,
                    DataType = new DataType { DataTypeName = "String" }
                } 
            }.BuildMock();
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            var expected = new List<ParameterListModel> { new ParameterListModel { ParameterId = 1 } };
            _mockMapper.Setup(m => m.Map<List<ParameterListModel>>(It.IsAny<List<ParameterListModel>>())).Returns(expected);

            var result = _service.GetByEntityId(1);

            Assert.NotNull(result);
        }

        [Fact]
        public void GetById_ShouldReturnMappedModel()
        {
            var tenantId = 1;
            var id = 1;
            var entity = new Parameter { TenantId = tenantId, ParameterId = id };
            var data = new List<Parameter> { entity }.AsQueryable();
            var expected = new ParameterListModel { ParameterId = id };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);
            _mockMapper.Setup(m => m.Map<ParameterListModel>(It.IsAny<Parameter>())).Returns(expected);

            var result = _service.GetById(tenantId, id);

            Assert.NotNull(result);
            Assert.Equal(id, result.ParameterId);
        }

        [Fact]
        public async Task CheckParameterComputedValue_ShouldReturnComputedValue_WhenSingleMatches()
        {
            var paramId = 1;
            var entity = new Parameter
            {
                ParameterId = paramId,
                ComputedValues = new List<ParameterComputedValue>
                {
                    new ParameterComputedValue { ComputedParameterType = ParameterComputedType.Single, ParameterExactValue = "Yes", ComputedValue = "100" }
                }
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
                ComputedValues = new List<ParameterComputedValue>
                {
                    new ParameterComputedValue { ComputedParameterType = ParameterComputedType.Range, FromValue = "10", ToValue = "20", ComputedValue = "In Range" }
                }
            };
            var data = new List<Parameter> { entity }.BuildMock();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            var result = await _service.CheckParameterComputedValue(1, paramId, "15");

            Assert.Equal("In Range", result);
        }

        [Fact]
        public async Task CheckParameterComputedValue_ShouldReturnNull_WhenNoComputedValues()
        {
            var paramId = 1;
            var entity = new Parameter
            {
                ParameterId = paramId,
                ComputedValues = []
            };
            var data = new List<Parameter> { entity }.BuildMock();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            var result = await _service.CheckParameterComputedValue(1, paramId, "Any");

            Assert.Null(result);
        }

        [Fact]
        public async Task CheckParameterComputedValue_ShouldReturnNull_WhenNoMatch()
        {
            var paramId = 1;
            var entity = new Parameter
            {
                ParameterId = paramId,
                ComputedValues = new List<ParameterComputedValue>
                {
                    new ParameterComputedValue { ComputedParameterType = ParameterComputedType.Single, ParameterExactValue = "Yes", ComputedValue = "100" }
                }
            };
            var data = new List<Parameter> { entity }.BuildMock();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            var result = await _service.CheckParameterComputedValue(1, paramId, "No");

            Assert.Null(result);
        }

        [Fact]
        public async Task Delete_ShouldThrow_WhenParameterNotFound()
        {
            var tenantId = 1;
            var id = 999;
            var data = new List<Parameter>().BuildMock();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(data);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Delete(tenantId, id));
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenParameterNameIsDuplicate()
        {
            var model = new ParameterAddUpdateModel { TenantId = 1, ParameterId = 1, ParameterName = "Param 2" };
            var existingData = new List<Parameter> { 
                new Parameter { TenantId = 1, ParameterId = 1, ParameterName = "Param 1" },
                new Parameter { TenantId = 1, ParameterId = 2, ParameterName = "Param 2" }
            }.BuildMock();

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(existingData);

            var exception = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("Parameter name already exists in this tenant", exception.Message);
        }
    }
}
