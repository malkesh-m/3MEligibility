using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class ApiParameterMapserviceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ApiParameterMapservice _service;

        public ApiParameterMapserviceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new ApiParameterMapservice(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Add_ShouldAddEntityAndComplete()
        {
            var model = new ApiParameterCreateUpdateMapModel { Id = 1 };
            var entity = new ApiParameterMap { Id = 1 };

            _mockMapper.Setup(m => m.Map<ApiParameterMap>(model)).Returns(entity);
            _mockUow.Setup(u => u.ApiParameterMapsRepository.Add(entity, false));

            await _service.Add(model);

            _mockUow.Verify(u => u.ApiParameterMapsRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnMappedModels()
        {
            int tenantId = 1;
            int apiId = 1; // Assuming apiId is 1 for this test
            var entities = new List<ApiParameterMap> { new ApiParameterMap { Id = 1 } };
            var models = new List<ApiParameterListMapModel> { new ApiParameterListMapModel { Id = 1 } };

            _mockUow.Setup(u => u.ApiParameterMapsRepository.GetAllByTenantId(tenantId, false)).Returns(entities.AsQueryable());
            _mockMapper.Setup(m => m.Map<List<ApiParameterListMapModel>>(entities)).Returns(models);

            var result = _service.GetAll(tenantId);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void GetById_ShouldReturnMappedModel()
        {
            var data = new List<ApiParameterMap> { new ApiParameterMap { Id = 1, TenantId = 1 } }.BuildMock();

            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(data);
            var expectedModel = new ApiParameterListMapModel { Id = 1 };
            _mockMapper.Setup(m => m.Map<ApiParameterListMapModel>(It.IsAny<IQueryable<ApiParameterMap>>())).Returns(expectedModel);

            var result = _service.GetById(1, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task Remove_ShouldRemoveEntityAndComplete()
        {
            var entity = new ApiParameterMap { Id = 1 };
            _mockUow.Setup(u => u.ApiParameterMapsRepository.GetById(1)).Returns(entity);

            await _service.Remove(1);

            _mockUow.Verify(u => u.ApiParameterMapsRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetMappingsByApiId_ShouldReturnMappedList()
        {
            var data = new List<ApiParameterMap>
            {
                new ApiParameterMap 
                { 
                    Id = 1, ApiId = 1, ApiParameterId = 10, ParameterId = 20,
                    ApiParameter = new ApiParameter { ParameterName = "ApiP1" },
                    Parameter = new MEligibilityPlatform.Domain.Entities.Parameter { ParameterName = "SysP1" }
                }
            }.BuildMock();

            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(data);

            var result = _service.GetMappingsByApiId(1);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("ApiP1", result[0].ApiParameterName);
            Assert.Equal("SysP1", result[0].ParameterName);
        }

        [Fact]
        public async Task Update_ShouldUpdateEntityAndComplete()
        {
            var model = new ApiParameterCreateUpdateMapModel { Id = 1, ApiParameterId = 2, ParameterId = 3, ApiId = 4 };
            var existingEntity = new ApiParameterMap { Id = 1 };
            var mappedEntity = new ApiParameterMap { Id = 1 };

            _mockUow.Setup(u => u.ApiParameterMapsRepository.GetById(1)).Returns(existingEntity);
            _mockMapper.Setup(m => m.Map<ApiParameterMap>(existingEntity)).Returns(mappedEntity);

            await _service.Update(model);

            _mockUow.Verify(u => u.ApiParameterMapsRepository.Update(mappedEntity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            Assert.Equal(2, mappedEntity.ApiParameterId);
            Assert.Equal(3, mappedEntity.ParameterId);
            Assert.Equal(4, mappedEntity.ApiId);
        }
    }
}
