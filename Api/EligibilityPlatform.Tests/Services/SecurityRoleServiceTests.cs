using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class SecurityRoleServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ISecurityRoleRepository> _mockRepo;
        private readonly SecurityRoleService _service;

        public SecurityRoleServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockRepo = new Mock<ISecurityRoleRepository>();
            _mockUow.Setup(u => u.SecurityRoleRepository).Returns(_mockRepo.Object);
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>().BuildMock());
            _service = new SecurityRoleService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Add_WhenNameMissing_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Add(new SecurityRoleUpdateModel { RoleName = " " }));
        }

        [Fact]
        public async Task Add_WhenRoleExists_Throws()
        {
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleName = "Admin", TenantId = 2 }
            }.BuildMock());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Add(new SecurityRoleUpdateModel { RoleName = "ADMIN", TenantId = 2 }));
        }

        [Fact]
        public async Task Add_WhenValid_AddsAndCompletes()
        {
            var model = new SecurityRoleUpdateModel { RoleName = "Editor", TenantId = 3 };
            var entity = new SecurityRole { RoleName = "Editor", TenantId = 3 };
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>().BuildMock());
            _mockMapper.Setup(m => m.Map<SecurityRole>(model)).Returns(entity);

            await _service.Add(model);

            _mockRepo.Verify(r => r.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetAll_WhenEmpty_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetAllByTenantId(2, false)).Returns(new List<SecurityRole>().BuildMock());

            var result = _service.GetAll(2);

            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WhenException_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetAllByTenantId(2, false)).Throws(new Exception("fail"));

            var result = _service.GetAll(2);

            Assert.Empty(result);
        }

        [Fact]
        public void GetById_WhenNotFound_Throws()
        {
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>().BuildMock());

            Assert.Throws<KeyNotFoundException>(() => _service.GetById(1, 2));
        }

        [Fact]
        public void GetById_ReturnsMappedModel()
        {
            var entity = new SecurityRole { RoleId = 3, TenantId = 2, RoleName = "Admin" };
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole> { entity }.BuildMock());
            _mockMapper.Setup(m => m.Map<SecurityRoleModel>(entity)).Returns(new SecurityRoleModel { RoleId = 3, RoleName = "Admin" });

            var result = _service.GetById(3, 2);

            Assert.Equal(3, result.RoleId);
        }

        [Fact]
        public async Task Remove_WhenSuperAdmin_Throws()
        {
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 1, RoleName = "Super Admin" }
            }.BuildMock());

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Remove(1));
            Assert.Equal("Super Admin role cannot be deleted.", ex.Message);
        }

        [Fact]
        public async Task Remove_WhenValid_RemovesAndCompletes()
        {
            var entity = new SecurityRole { RoleId = 2, RoleName = "Admin" };
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole> { entity }.BuildMock());
            _mockRepo.Setup(r => r.GetById(2)).Returns(entity);

            await _service.Remove(2);

            _mockRepo.Verify(r => r.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_WhenSuperAdmin_Throws()
        {
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 1, RoleName = "Super Admin" }
            }.BuildMock());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Update(new SecurityRoleUpdateModel { RoleId = 1 }));
        }

        [Fact]
        public async Task Update_PreservesCreatedFields()
        {
            var entity = new SecurityRole
            {
                RoleId = 4,
                RoleName = "Editor",
                CreatedBy = "seed",
                CreatedByDateTime = new DateTime(2020, 1, 1)
            };
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole> { entity }.BuildMock());
            _mockRepo.Setup(r => r.GetById(4)).Returns(entity);
            _mockMapper.Setup(m => m.Map<SecurityRoleUpdateModel, SecurityRole>(It.IsAny<SecurityRoleUpdateModel>(), entity)).Returns(entity);

            await _service.Update(new SecurityRoleUpdateModel { RoleId = 4, RoleName = "Editor", TenantId = 1 });

            Assert.Equal("seed", entity.CreatedBy);
            Assert.Equal(new DateTime(2020, 1, 1), entity.CreatedByDateTime);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task MultipleDelete_WhenSuperAdmin_Throws()
        {
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 1, RoleName = "Super Admin" }
            }.BuildMock());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MultipleDelete([1]));
        }

        [Fact]
        public async Task MultipleDelete_WhenMissingId_Throws()
        {
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 2, RoleName = "Admin" }
            }.BuildMock());

            await Assert.ThrowsAsync<Exception>(() => _service.MultipleDelete([3]));
        }

        [Fact]
        public async Task MultipleDelete_RemovesAll()
        {
            var entity1 = new SecurityRole { RoleId = 2, RoleName = "Admin" };
            var entity2 = new SecurityRole { RoleId = 3, RoleName = "User" };
            _mockRepo.Setup(r => r.Query()).Returns(new List<SecurityRole> { entity1, entity2 }.BuildMock());
            _mockRepo.Setup(r => r.GetById(2)).Returns(entity1);
            _mockRepo.Setup(r => r.GetById(3)).Returns(entity2);

            await _service.MultipleDelete([2, 3]);

            _mockRepo.Verify(r => r.Remove(entity1), Times.Once);
            _mockRepo.Verify(r => r.Remove(entity2), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }
    }
}
