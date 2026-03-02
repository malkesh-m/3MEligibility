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
    public class NodeServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly NodeService _service;

        public NodeServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new NodeService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Add_ShouldAddEntity_WhenCodeIsUnique()
        {
            var model = new NodeCreateUpdateModel { Code = "N1", TenantId = 1 };
            
            _mockUow.Setup(u => u.NodeModelRepository.GetAllByTenantId(1, false))
                .Returns(new List<Node>().AsQueryable());
            _mockMapper.Setup(m => m.Map<List<NodeListModel>>(It.IsAny<List<Node>>()))
                .Returns(new List<NodeListModel>());
            _mockMapper.Setup(m => m.Map<Node>(model)).Returns(new Node { Code = "N1", TenantId = 1 });

            await _service.Add(model);

            _mockUow.Verify(u => u.NodeModelRepository.Add(It.IsAny<Node>(), false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Add_ShouldThrowException_WhenCodeAlreadyExists()
        {
            var model = new NodeCreateUpdateModel { Code = "N1", TenantId = 1 };
            var existingNodes = new List<NodeListModel> { new NodeListModel { Code = "N1" } };
            
            _mockUow.Setup(u => u.NodeModelRepository.GetAllByTenantId(1, false))
                .Returns(new List<Node> { new Node { Code = "N1" } }.AsQueryable());
            _mockMapper.Setup(m => m.Map<List<NodeListModel>>(It.IsAny<List<Node>>()))
                .Returns(existingNodes);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Add(model));
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntity()
        {
            var node = new Node { NodeId = 1, TenantId = 1 };
            _mockUow.Setup(u => u.NodeModelRepository.Query()).Returns(new List<Node> { node }.AsQueryable());

            await _service.Delete(1, 1);

            _mockUow.Verify(u => u.NodeModelRepository.Remove(node), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnMappedModels()
        {
            var nodes = new List<Node> { new Node { NodeId = 1 } }.AsQueryable();
            _mockUow.Setup(u => u.NodeModelRepository.GetAllByTenantId(1, false)).Returns(nodes);
            _mockMapper.Setup(m => m.Map<List<NodeListModel>>(It.IsAny<List<Node>>())).Returns(new List<NodeListModel> { new NodeListModel { NodeId = 1 } });

            var result = _service.GetAll(1);

            Assert.Single(result);
            Assert.Equal(1, result[0].NodeId);
        }

        [Fact]
        public void GetById_ShouldReturnMappedModel()
        {
            var node = new Node { NodeId = 1, TenantId = 1 };
            _mockUow.Setup(u => u.NodeModelRepository.Query()).Returns(new List<Node> { node }.AsQueryable());
            _mockMapper.Setup(m => m.Map<NodeListModel>(node)).Returns(new NodeListModel { NodeId = 1 });

            var result = _service.GetById(1, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.NodeId);
        }

        [Fact]
        public async Task Update_ShouldUpdateEntityAndComplete()
        {
            var model = new NodeCreateUpdateModel { NodeId = 1, TenantId = 1 };
            var existingNode = new Node { NodeId = 1, TenantId = 1 };
            
            _mockUow.Setup(u => u.NodeModelRepository.Query()).Returns(new List<Node> { existingNode }.AsQueryable());
            _mockMapper.Setup(m => m.Map<NodeCreateUpdateModel, Node>(model, existingNode)).Returns(existingNode);

            await _service.Update(model);

            _mockUow.Verify(u => u.NodeModelRepository.Update(existingNode), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task MultipleDelete_ShouldRemoveEntities()
        {
            var nodes = new List<Node>
            {
                new Node { NodeId = 1, TenantId = 1 },
                new Node { NodeId = 2, TenantId = 1 }
            }.AsQueryable();

            _mockUow.Setup(u => u.NodeModelRepository.Query()).Returns(nodes.BuildMock());

            await _service.MultipleDelete(1, new List<int> { 1, 2 });

            _mockUow.Verify(u => u.NodeModelRepository.Remove(It.IsAny<Node>()), Times.Exactly(2));
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
