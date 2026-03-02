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
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class EcardServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IEruleService> _mockEruleService;
        private readonly Mock<IEruleMasterService> _mockEruleMasterService;
        private readonly Mock<IExportService> _mockExportService;
        private readonly EcardService _service;

        public EcardServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockEruleService = new Mock<IEruleService>();
            _mockEruleMasterService = new Mock<IEruleMasterService>();
            _mockExportService = new Mock<IExportService>();

            _service = new EcardService(
                _mockUow.Object, 
                _mockMapper.Object, 
                _mockEruleService.Object, 
                _mockEruleMasterService.Object, 
                _mockExportService.Object);
        }

        [Fact]
        public async Task Add_ExistingName_ShouldThrowException()
        {
            var data = new List<Ecard> { new Ecard { TenantId = 1, EcardName = "E1" } }.BuildMock();
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(data);

            var model = new EcardAddUpdateModel { EcardName = "E1" };

            await Assert.ThrowsAsync<Exception>(() => _service.Add(1, model));
        }

        [Fact]
        public async Task Add_ValidModel_ShouldAddAndComplete()
        {
            var data = new List<Ecard>().BuildMock();
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(data);

            var model = new EcardAddUpdateModel { EcardName = "E1" };
            var entity = new Ecard { EcardName = "E1" };
            _mockMapper.Setup(m => m.Map<Ecard>(model)).Returns(entity);

            await _service.Add(1, model);

            _mockUow.Verify(u => u.EcardRepository.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrow_WhenNameAlreadyExists()
        {
            var existing = new List<Ecard>
            {
                new Ecard { TenantId = 1, EcardId = 1, EcardName = "EC1" },
                new Ecard { TenantId = 1, EcardId = 2, EcardName = "EC2" }
            }.BuildMock();

            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(existing);

            var model = new EcardUpdateModel { TenantId = 1, EcardId = 1, EcardName = "EC2" };

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Update(model));
            Assert.Equal("Ecard with this name already exists", ex.Message);
        }

        [Fact]
        public async Task Delete_IsInUseByPcard_ShouldReturnErrorMessage()
        {
            var pcardData = new List<Pcard> { new Pcard { TenantId = 1, Expression = "1 AND 2" } }.BuildMock();
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(pcardData);

            var result = await _service.Delete(1, 1);

            Assert.Contains("cannot be deleted because it is currently being used", result);
        }

        [Fact]
        public async Task Delete_NotInUse_ShouldDeleteAndComplete()
        {
            var pcardData = new List<Pcard>().BuildMock();
            var ecardData = new List<Ecard> { new Ecard { EcardId = 1, TenantId = 1 } }.BuildMock();

            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(pcardData);
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(ecardData);

            var result = await _service.Delete(1, 1);

            Assert.Equal("Deleted successfully.", result);
            _mockUow.Verify(u => u.EcardRepository.Remove(It.IsAny<Ecard>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnModels()
        {
            var data = new List<Ecard> { new Ecard { TenantId = 1, EcardId = 1 } }.AsQueryable().BuildMock();
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(data);

            var expected = new List<EcardListModel> { new EcardListModel { EcardId = 1 } };
            _mockMapper.Setup(m => m.Map<List<EcardListModel>>(It.IsAny<List<Ecard>>())).Returns(expected);

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAll_ShouldFilterByTenant()
        {
            var data = new List<Ecard>
            {
                new Ecard { TenantId = 1, EcardId = 1 },
                new Ecard { TenantId = 2, EcardId = 2 }
            }.AsQueryable().BuildMock();
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(data);

            List<Ecard>? captured = null;
            _mockMapper.Setup(m => m.Map<List<EcardListModel>>(It.IsAny<object>()))
                .Callback<object>(obj => captured = obj as List<Ecard>)
                .Returns(new List<EcardListModel> { new EcardListModel { EcardId = 1 } });

            var result = await _service.GetAll(1);

            Assert.NotNull(result);
            Assert.NotNull(captured);
            Assert.All(captured!, c => Assert.Equal(1, c.TenantId));
        }
    }
}
