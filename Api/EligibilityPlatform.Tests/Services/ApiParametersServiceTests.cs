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
    public class ApiParametersServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ApiParametersService _service;

        public ApiParametersServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new ApiParametersService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Add_ShouldAddEntityAndReturnIt()
        {
            var model = new ApiParametersCreateUpdateModel { ApiId = 1 };
            var entity = new ApiParameter { ApiId = 1 };

            _mockMapper.Setup(m => m.Map<ApiParameter>(model)).Returns(entity);
            _mockUow.Setup(u => u.ApiParametersRepository.Add(entity, false));
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            var result = await _service.Add(model);

            _mockUow.Verify(u => u.ApiParametersRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(1, result.ApiId);
        }

        [Fact]
        public async Task AddRange_ShouldAddEntitiesAndComplete()
        {
            var models = new List<ApiParametersCreateUpdateModel> { new ApiParametersCreateUpdateModel { ApiId = 1 } };
            var entities = new List<ApiParameter> { new ApiParameter { ApiId = 1 } };

            _mockMapper.Setup(m => m.Map<List<ApiParameter>>(models)).Returns(entities);
            _mockUow.Setup(u => u.ApiParametersRepository.AddRange(entities));
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            await _service.AddRange(models);

            _mockUow.Verify(u => u.ApiParametersRepository.AddRange(entities), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteByApiIdAsync_ShouldRemoveRangeAndComplete()
        {
            var apiId = 1;
            var data = new List<ApiParameter>
            {
                new ApiParameter { ApiId = apiId },
                new ApiParameter { ApiId = 2 }
            }.AsQueryable();

            var mockSet = data.BuildMock();
            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(mockSet);
            _mockUow.Setup(u => u.ApiParametersRepository.RemoveRange(It.IsAny<IQueryable<ApiParameter>>()));
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            await _service.DeleteByApiIdAsync(apiId);

            _mockUow.Verify(u => u.ApiParametersRepository.RemoveRange(It.IsAny<IQueryable<ApiParameter>>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnListOfModels()
        {
            var tenantId = 1;
            var entities = new List<ApiParameter> { new ApiParameter { ApiId = 1 } };
            var models = new List<ApiParametersListModel> { new ApiParametersListModel { ApiId = 1 } };

            _mockUow.Setup(u => u.ApiParametersRepository.GetAllByTenantId(tenantId, false)).Returns(entities.AsQueryable());
            _mockMapper.Setup(m => m.Map<List<ApiParametersListModel>>(entities)).Returns(models);

            var result = _service.GetAll(tenantId);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void GetById_ShouldReturnModel()
        {
            var id = 1;
            var entity = new ApiParameter { ApiParamterId = id };
            var model = new ApiParametersListModel { ApiParamterId = id }; // Note: Assuming model has Id based on your code

            _mockUow.Setup(u => u.ApiParametersRepository.GetById(id)).Returns(entity);
            _mockMapper.Setup(m => m.Map<ApiParametersListModel>(entity)).Returns(model);

            var result = _service.GetById(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.ApiParamterId);
        }

        [Fact]
        public async Task GetByApiId_ShouldReturnModels()
        {
            var apiId = 1;
            var data = new List<ApiParameter> { new ApiParameter { ApiId = apiId } }.BuildMock();

            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(data);
            var expectedModels = new List<ApiParametersListModel> { new ApiParametersListModel { ApiId = apiId } };
            _mockMapper.Setup(m => m.Map<List<ApiParametersListModel>>(It.IsAny<List<ApiParameter>>())).Returns(expectedModels);

            var result = await _service.GetByApiId(apiId);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task Remove_ShouldRemoveAndComplete()
        {
            var id = 1;
            var entity = new ApiParameter { ApiParamterId = id };

            _mockUow.Setup(u => u.ApiParametersRepository.GetById(id)).Returns(entity);
            _mockUow.Setup(u => u.ApiParametersRepository.Remove(entity));

            await _service.Remove(id);

            _mockUow.Verify(u => u.ApiParametersRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveMultiple_ShouldRemoveValidEntitiesAndComplete()
        {
            var ids = new List<int> { 1, 2, 3 };
            var entity1 = new ApiParameter { ApiParamterId = 1 };
            ApiParameter entity2 = null;
            var entity3 = new ApiParameter { ApiParamterId = 3 };

            _mockUow.Setup(u => u.ApiParametersRepository.GetById(1)).Returns(entity1);
            _mockUow.Setup(u => u.ApiParametersRepository.GetById(2)).Returns(entity2);
            _mockUow.Setup(u => u.ApiParametersRepository.GetById(3)).Returns(entity3);

            await _service.RemoveMultiple(ids);

            _mockUow.Verify(u => u.ApiParametersRepository.Remove(entity1), Times.Once);
            _mockUow.Verify(u => u.ApiParametersRepository.Remove(It.IsAny<ApiParameter>()), Times.Exactly(2));
            _mockUow.Verify(u => u.ApiParametersRepository.Remove(entity3), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldUpdateAndComplete()
        {
            var model = new ApiParametersCreateUpdateModel { ApiId = 1 };
            var entity = new ApiParameter { ApiId = 1 };

            _mockMapper.Setup(m => m.Map<ApiParameter>(model)).Returns(entity);
            _mockUow.Setup(u => u.ApiParametersRepository.Update(entity));

            await _service.Update(model);

            _mockUow.Verify(u => u.ApiParametersRepository.Update(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            Assert.NotEqual(default, entity.UpdatedByDateTime);
        }
    }
}
