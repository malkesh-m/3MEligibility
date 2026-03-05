using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class RolePermissionServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IUserContextService> _mockUserContext;
        private readonly Mock<IUserRoleService> _mockUserRoleService;
        private readonly Mock<IRolePermissionRepository> _mockRolePermissionRepo;
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
        private readonly Mock<ISecurityRoleRepository> _mockSecurityRoleRepo;
        private readonly Mock<IPermissionRepository> _mockPermissionRepo;
        private readonly RolePermissionService _service;

        public RolePermissionServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockCache = new Mock<IMemoryCache>();
            _mockUserService = new Mock<IUserService>();
            _mockUserContext = new Mock<IUserContextService>();
            _mockUserRoleService = new Mock<IUserRoleService>();
            _mockRolePermissionRepo = new Mock<IRolePermissionRepository>();
            _mockUserRoleRepo = new Mock<IUserRoleRepository>();
            _mockSecurityRoleRepo = new Mock<ISecurityRoleRepository>();
            _mockPermissionRepo = new Mock<IPermissionRepository>();

            _mockUow.Setup(u => u.RolePermissionRepository).Returns(_mockRolePermissionRepo.Object);
            _mockUow.Setup(u => u.UserRoleRepository).Returns(_mockUserRoleRepo.Object);
            _mockUow.Setup(u => u.SecurityRoleRepository).Returns(_mockSecurityRoleRepo.Object);
            _mockUow.Setup(u => u.PermissionRepository).Returns(_mockPermissionRepo.Object);
            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(new List<RolePermission>().BuildMock());
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().BuildMock());
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>().BuildMock());
            _mockPermissionRepo.Setup(r => r.Query()).Returns(new List<Permission>().BuildMock());
            _mockUserContext.Setup(c => c.GetUserId()).Returns(1);

            _service = new RolePermissionService(
                _mockUow.Object,
                _mockMapper.Object,
                _mockCache.Object,
                _mockUserService.Object,
                _mockUserContext.Object,
                _mockUserRoleService.Object);
        }

        [Fact]
        public async Task Add_AddsOnlyMissingPermissionsAndClearsCache()
        {
            var model = new RolePermissionModel { RoleId = 3, TenantId = 2, PermissionIds = [1, 2, 3] };
            var existing = new List<RolePermission>
            {
                new() { RoleId = 3, TenantId = 2, PermissionId = 1 }
            }.BuildMock();
            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(existing);
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 3, TenantId = 2, RoleName = "Admin" }
            }.BuildMock());
            _mockUserRoleService.Setup(s => s.GetRoleNamesForUser(1, 2)).ReturnsAsync(["Admin"]);
            _mockUserRoleService.Setup(s => s.GetHighestRank(It.IsAny<IEnumerable<string>>())).Returns(Rank.Admin);
            _mockUserRoleService.Setup(s => s.GetRank("Admin")).Returns(Rank.Admin);
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>
            {
                new() { UserId = 5, RoleId = 3, TenantId = 2 }
            }.BuildMock());

            await _service.Add(model);

            _mockRolePermissionRepo.Verify(r => r.AddRange(It.Is<IEnumerable<RolePermission>>(p => p.Count() == 2)), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            _mockUserService.Verify(s => s.RemoveUserPermissionsCache(5), Times.Once);
        }

        [Fact]
        public async Task Add_WhenNoNewPermissions_SkipsAddRange()
        {
            var model = new RolePermissionModel { RoleId = 3, TenantId = 2, PermissionIds = [1] };
            var existing = new List<RolePermission>
            {
                new() { RoleId = 3, TenantId = 2, PermissionId = 1 }
            }.BuildMock();
            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(existing);
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 3, TenantId = 2, RoleName = "Admin" }
            }.BuildMock());
            _mockUserRoleService.Setup(s => s.GetRoleNamesForUser(1, 2)).ReturnsAsync(["Admin"]);
            _mockUserRoleService.Setup(s => s.GetHighestRank(It.IsAny<IEnumerable<string>>())).Returns(Rank.Admin);
            _mockUserRoleService.Setup(s => s.GetRank("Admin")).Returns(Rank.Admin);
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().BuildMock());

            await _service.Add(model);

            _mockRolePermissionRepo.Verify(r => r.AddRange(It.IsAny<IEnumerable<RolePermission>>()), Times.Never);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public void GetAll_ReturnsMappedList()
        {
            var data = new List<RolePermission> { new() { RoleId = 1, PermissionId = 2, TenantId = 3 } };
            _mockRolePermissionRepo.Setup(r => r.GetAll()).Returns(data);
            _mockMapper.Setup(m => m.Map<List<RolePermissionModel>>(data)).Returns([new RolePermissionModel { RoleId = 1, PermissionIds = [2] }]);

            var result = _service.GetAll();

            Assert.Single(result);
            Assert.Equal(1, result[0].RoleId);
        }

        [Fact]
        public async Task GetBySecurityRoleId_ReturnsValue()
        {
            _mockRolePermissionRepo.Setup(r => r.GetBySecurityRoleId(3)).ReturnsAsync(true);

            var result = await _service.GetBySecurityRoleId(3);

            Assert.True(result);
        }

        [Fact]
        public async Task Remove_WhenSuperAdminRole_Throws()
        {
            var model = new RolePermissionModel { RoleId = 1, TenantId = 2, PermissionIds = [1] };
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 1, TenantId = 2, RoleName = "Super Admin" }
            }.BuildMock());
            _mockUserRoleService.Setup(s => s.GetRoleNamesForUser(1, 2)).ReturnsAsync(["Super Admin"]);
            _mockUserRoleService.Setup(s => s.GetHighestRank(It.IsAny<IEnumerable<string>>())).Returns(Rank.SuperAdmin);
            _mockUserRoleService.Setup(s => s.GetRank("Super Admin")).Returns(Rank.SuperAdmin);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Remove(model));
        }

        [Fact]
        public async Task Remove_RemovesExistingAndClearsCache()
        {
            var model = new RolePermissionModel { RoleId = 2, TenantId = 2, PermissionIds = [1, 2] };
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 2, TenantId = 2, RoleName = "Admin" }
            }.BuildMock());
            _mockUserRoleService.Setup(s => s.GetRoleNamesForUser(1, 2)).ReturnsAsync(["Admin"]);
            _mockUserRoleService.Setup(s => s.GetHighestRank(It.IsAny<IEnumerable<string>>())).Returns(Rank.Admin);
            _mockUserRoleService.Setup(s => s.GetRank("Admin")).Returns(Rank.Admin);
            _mockRolePermissionRepo.Setup(r => r.GetRolePermission(2, 1)).ReturnsAsync(new RolePermission { RoleId = 2, PermissionId = 1, TenantId = 2 });
            _mockRolePermissionRepo.Setup(r => r.GetRolePermission(2, 2)).ReturnsAsync((RolePermission?)null);
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>
            {
                new() { UserId = 5, RoleId = 2, TenantId = 2 }
            }.BuildMock());

            await _service.Remove(model);

            _mockRolePermissionRepo.Verify(r => r.RemoveRange(It.Is<IEnumerable<RolePermission>>(p => p.Count() == 1)), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            _mockUserService.Verify(s => s.RemoveUserPermissionsCache(5), Times.Once);
        }

        [Fact]
        public async Task GetAssignedPermissions_ReturnsList()
        {
            var data = new List<RolePermission>
            {
                new()
                {
                    RoleId = 1,
                    TenantId = 2,
                    PermissionId = 9,
                    Permission = new Permission { PermissionId = 9, PermissionAction = "A" }
                }
            }.BuildMock();
            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(data);

            var result = await _service.GetAssignedPermissions(1, 2);

            Assert.Single(result);
            Assert.Equal(9, result[0].PermissionId);
        }

        [Fact]
        public async Task GetUnAssignedPermissions_ReturnsList()
        {
            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(new List<RolePermission>
            {
                new() { RoleId = 1, TenantId = 2, PermissionId = 9 }
            }.BuildMock());
            _mockPermissionRepo.Setup(r => r.Query()).Returns(new List<Permission>
            {
                new() { PermissionId = 9, PermissionAction = "A" },
                new() { PermissionId = 10, PermissionAction = "B" }
            }.BuildMock());

            var result = await _service.GetUnAssignedPermissions(1, 2);

            Assert.Single(result);
            Assert.Equal(10, result[0].PermissionId);
        }

        [Fact]
        public async Task RemoveByRoleId_WhenSuperAdminRole_Throws()
        {
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 1, TenantId = 2, RoleName = "Super Admin" }
            }.BuildMock());
            _mockUserRoleService.Setup(s => s.GetRoleNamesForUser(1, 2)).ReturnsAsync(["Super Admin"]);
            _mockUserRoleService.Setup(s => s.GetHighestRank(It.IsAny<IEnumerable<string>>())).Returns(Rank.SuperAdmin);
            _mockUserRoleService.Setup(s => s.GetRank("Super Admin")).Returns(Rank.SuperAdmin);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RemoveByRoleId(1, 2));
        }

        [Fact]
        public async Task RemoveByRoleId_RemovesAllAndCompletes()
        {
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 2, TenantId = 2, RoleName = "Admin" }
            }.BuildMock());
            _mockUserRoleService.Setup(s => s.GetRoleNamesForUser(1, 2)).ReturnsAsync(["Admin"]);
            _mockUserRoleService.Setup(s => s.GetHighestRank(It.IsAny<IEnumerable<string>>())).Returns(Rank.Admin);
            _mockUserRoleService.Setup(s => s.GetRank("Admin")).Returns(Rank.Admin);
            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(new List<RolePermission>
            {
                new() { RoleId = 2, TenantId = 2, PermissionId = 1 }
            }.BuildMock());

            await _service.RemoveByRoleId(2, 2);

            _mockRolePermissionRepo.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<RolePermission>>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task EnsureCanEditRolePermissions_WhenRoleMissing_Throws()
        {
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>().BuildMock());

            var method = typeof(RolePermissionService).GetMethod("EnsureCanEditRolePermissions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task)method!.Invoke(_service, new object[] { 1, 2 })!;

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
        }
    }
}
