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
    public class EruleMasterServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly EruleMasterService _service;

        public EruleMasterServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new EruleMasterService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Add_ExistingName_ShouldThrowException()
        {
            var data = new List<EruleMaster> { new EruleMaster { TenantId = 1, EruleName = "R1" } }.BuildMock();
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(data);

            var model = new EruleMasterCreateUpodateModel { TenantId = 1, EruleName = "R1" };

            await Assert.ThrowsAsync<Exception>(() => _service.Add(model, 1));
        }

        [Fact]
        public async Task Add_ValidModel_ShouldAddAndComplete()
        {
            var data = new List<EruleMaster>().BuildMock();
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(data);

            var model = new EruleMasterCreateUpodateModel { TenantId = 1, EruleName = "R1" };
            var entity = new EruleMaster { TenantId = 1, EruleName = "R1" };
            _mockMapper.Setup(m => m.Map<EruleMaster>(model)).Returns(entity);

            await _service.Add(model, 1);

            _mockUow.Verify(u => u.EruleMasterRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Edit_ExistingNameDifferentId_ShouldThrowException()
        {
            var data = new List<EruleMaster> { new EruleMaster { TenantId = 1, EruleName = "R1", Id = 2 } }.BuildMock();
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(data);

            var model = new EruleMasterCreateUpodateModel { EruleId = 1, EruleName = "R1" };

            await Assert.ThrowsAsync<Exception>(() => _service.Edit(model, 1));
        }

        [Fact]
        public async Task GetAll_ShouldReturnModels()
        {
            var data = new List<EruleMaster> { new EruleMaster { TenantId = 1, Id = 1, EruleName = "Test" } }.BuildMock();
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(data);

            var expected = new List<EruleMasterListModel> { new EruleMasterListModel { EruleId = 1 } };
            _mockMapper.Setup(m => m.Map<List<EruleMasterListModel>>(It.IsAny<List<EruleMaster>>())).Returns(expected);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task Delete_ExistingId_ShouldDeleteAndComplete()
        {
            var data = new List<EruleMaster> { new EruleMaster { Id = 1, EruleName = "Test" } }.BuildMock();
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(data);

            var result = await _service.Delete(1);

            Assert.Equal("Deleted successfully.", result);
            _mockUow.Verify(u => u.EruleMasterRepository.Remove(It.IsAny<EruleMaster>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
