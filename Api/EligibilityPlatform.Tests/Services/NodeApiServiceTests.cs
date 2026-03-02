using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using MEligibilityPlatform.Application.Repository;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class NodeApiServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly NodeApiService _service;

        public NodeApiServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            var mockRepo = new Mock<INodeApiRepository>();
            _mockUow.SetupGet(u => u.NodeApiRepository).Returns(mockRepo.Object);

            _mockMapper = new Mock<IMapper>();
            _service = new NodeApiService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Add_ShouldAddEntityAndComplete()
        {
            var model = new NodeApiCreateOrUpdateModel { Apiid = 1 };
            var entity = new NodeApi { Apiid = 1 };

            // Setup mock mapper to return a valid entity
            _mockMapper.Setup(m => m.Map<NodeApi>(model)).Returns(entity);

            await _service.Add(model);

            _mockUow.Verify(u => u.NodeApiRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddRange_ShouldAddMultipleEntitiesAndComplete()
        {
            var models = new List<NodeApiCreateOrUpdateModel> { new NodeApiCreateOrUpdateModel { Apiid = 1 } };
            var entities = new List<NodeApi> { new NodeApi { Apiid = 1 } };

            // Setup mock mapper to return a valid list of entities
            _mockMapper.Setup(m => m.Map<List<NodeApi>>(models)).Returns(entities);

            await _service.AddRange(models);

            _mockUow.Verify(u => u.NodeApiRepository.AddRange(entities), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntityAndComplete()
        {
            var entity = new NodeApi { Apiid = 1 };
            _mockUow.Setup(u => u.NodeApiRepository.GetById(1)).Returns(entity);

            await _service.Delete(1);

            _mockUow.Verify(u => u.NodeApiRepository.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnMappedModels()
        {
            var entities = new List<NodeApi> { new NodeApi { Apiid = 1 } }.AsQueryable();
            _mockUow.Setup(u => u.NodeApiRepository.GetAllByTenantId(1, false)).Returns(entities);
            _mockMapper.Setup(m => m.Map<List<NodeApiListModel>>(It.IsAny<List<NodeApi>>())).Returns(new List<NodeApiListModel> { new NodeApiListModel { Apiid = 1 } });

            var result = _service.GetAll(1);

            Assert.Single(result);
            Assert.Equal(1, result[0].Apiid);
        }

        [Fact]
        public void GetById_ShouldReturnMappedModel()
        {
            var entity = new NodeApi { Apiid = 1, TenantId = 1 };
            _mockUow.Setup(u => u.NodeApiRepository.Query()).Returns(new List<NodeApi> { entity }.AsQueryable());
            _mockMapper.Setup(m => m.Map<NodeApiListModel>(It.IsAny<IQueryable<NodeApi>>())).Returns(new NodeApiListModel { Apiid = 1 });

            var result = _service.GetById(1, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Apiid);
        }

        [Fact]
        public async Task Update_ShouldUpdateEntityAndComplete()
        {
            var model = new NodeApiCreateOrUpdateModel { Apiid = 1 };
            var entity = new NodeApi { Apiid = 1, CreatedBy = "Admin" };

            _mockUow.Setup(u => u.NodeApiRepository.GetById(1)).Returns(entity);
            _mockMapper.Setup(m => m.Map<NodeApiCreateOrUpdateModel, NodeApi>(model, entity)).Returns(entity);

            await _service.Update(model);

            _mockUow.Verify(u => u.NodeApiRepository.Update(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_ShouldUpdateStatusAndComplete()
        {
            var entity = new NodeApi { Apiid = 1, IsActive = false };
            _mockUow.Setup(u => u.NodeApiRepository.GetById(1)).Returns(entity);

            await _service.UpdateStatus(1, true);

            Assert.True(entity.IsActive);
            _mockUow.Verify(u => u.NodeApiRepository.Update(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task MultipleDelete_ShouldRemoveEntitiesAndComplete()
        {
            var entity1 = new NodeApi { Apiid = 1 };
            
            _mockUow.Setup(u => u.NodeApiRepository.Query()).Returns(new List<NodeApi> { entity1 }.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.NodeApiRepository.GetById(1)).Returns(entity1);

            await _service.MultipleDelete(new List<int> { 1 });

            _mockUow.Verify(u => u.NodeApiRepository.Remove(entity1), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetBinaryXmlById_ShouldReturnString_WhenRecordExists()
        {
            var xml = "<xml></xml>";
            var entity = new NodeApi { Apiid = 1, NodeId = 1, BinaryXml = xml };
            _mockUow.Setup(u => u.NodeApiRepository.Query()).Returns(new List<NodeApi> { entity }.AsQueryable());

            var result = _service.GetBinaryXmlById(1, 1);

            Assert.Equal(xml, result);
        }
    }
}
