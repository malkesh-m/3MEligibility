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
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class UserRoleServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
        private readonly Mock<ISecurityRoleRepository> _mockSecurityRoleRepo;
        private readonly UserRoleService _service;

        public UserRoleServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockUserService = new Mock<IUserService>();
            _mockUserRoleRepo = new Mock<IUserRoleRepository>();
            _mockSecurityRoleRepo = new Mock<ISecurityRoleRepository>();

            _mockUow.Setup(u => u.UserRoleRepository).Returns(_mockUserRoleRepo.Object);
            _mockUow.Setup(u => u.SecurityRoleRepository).Returns(_mockSecurityRoleRepo.Object);
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().BuildMock());
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>().BuildMock());

            _service = new UserRoleService(_mockUow.Object, _mockMapper.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task Add_WhenAlreadyExists_ReturnsMessage()
        {
            var existing = new List<UserRole>
            {
                new() { UserId = 1, RoleId = 2, TenantId = 3 }
            }.BuildMock();
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(existing);

            var result = await _service.Add(new UserRoleCreateUpdateModel { UserId = 1, RoleId = 2, TenantId = 3 });

            Assert.Equal("User already added in this role", result);
            _mockUserRoleRepo.Verify(r => r.Add(It.IsAny<UserRole>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task Add_WhenNew_AddsAndClearsCache()
        {
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().BuildMock());
            var model = new UserRoleCreateUpdateModel { UserId = 5, RoleId = 7, TenantId = 9 };
            var entity = new UserRole { UserId = 5, RoleId = 7, TenantId = 9 };
            _mockMapper.Setup(m => m.Map<UserRole>(model)).Returns(entity);

            var result = await _service.Add(model);

            Assert.Equal("Success", result);
            _mockUserRoleRepo.Verify(r => r.Add(entity, false), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            _mockUserService.Verify(s => s.RemoveUserPermissionsCache(5), Times.Once);
        }

        [Fact]
        public void GetAll_ReturnsMappedList()
        {
            var data = new List<UserRole> { new() { UserId = 1, RoleId = 2 } };
            _mockUserRoleRepo.Setup(r => r.GetAll()).Returns(data);
            _mockMapper.Setup(m => m.Map<List<UserRoleModel>>(data)).Returns([new UserRoleModel { UserId = 1, RoleId = 2 }]);

            var result = _service.GetAll();

            Assert.Single(result);
            Assert.Equal(1, result[0].UserId);
        }

        [Fact]
        public void GetUserByRoleId_ReturnsRepositoryResult()
        {
            var users = new ApiResponse<List<UserGetModel>> { Data = [new UserGetModel { Id = 1 }] };
            _mockUserRoleRepo.Setup(r => r.GetUserByRoleId(4, users)).Returns([new UserInfo { UserId = 1, UserName = "U", MobileNo = "1", Email = "u@test.com" }]);

            var result = _service.GetUserByRoleId(4, users);

            Assert.Single(result);
            Assert.Equal(1, result[0].UserId);
        }

        [Fact]
        public async Task GetByUserRoleId_ReturnsValue()
        {
            _mockUserRoleRepo.Setup(r => r.GetByUserRoleId(2)).ReturnsAsync(true);

            var result = await _service.GetByUserRoleId(2);

            Assert.True(result);
        }

        [Fact]
        public async Task GetByUserRolesId_ReturnsValue()
        {
            _mockUserRoleRepo.Setup(r => r.GetByUserRolesId(3)).ReturnsAsync(true);

            var result = await _service.GetByUserRolesId(3);

            Assert.True(result);
        }

        [Fact]
        public async Task GetUserCountByRoleId_CountsAcrossTenantAndGlobal()
        {
            var data = new List<UserRole>
            {
                new() { RoleId = 1, TenantId = 10 },
                new() { RoleId = 1, TenantId = 0 },
                new() { RoleId = 1, TenantId = 11 }
            }.BuildMock();
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(data);

            var result = await _service.GetUserCountByRoleId(1, 10);

            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetRoleCountByUserId_ReturnsDistinctRoles()
        {
            var userRoles = new List<UserRole>
            {
                new() { UserId = 1, RoleId = 10 },
                new() { UserId = 1, RoleId = 10 },
                new() { UserId = 1, RoleId = 20 }
            }.BuildMock();
            var securityRoles = new List<SecurityRole>
            {
                new() { RoleId = 10, TenantId = 5 },
                new() { RoleId = 20, TenantId = 5 }
            }.BuildMock();
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(userRoles);
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(securityRoles);

            var result = await _service.GetRoleCountByUserId(1, 5);

            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetRoleNamesForUser_ReturnsDistinctNames()
        {
            var userRoles = new List<UserRole>
            {
                new() { UserId = 1, RoleId = 10 },
                new() { UserId = 1, RoleId = 20 }
            }.BuildMock();
            var securityRoles = new List<SecurityRole>
            {
                new() { RoleId = 10, TenantId = 7, RoleName = "Admin" },
                new() { RoleId = 20, TenantId = 7, RoleName = "Admin" }
            }.BuildMock();
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(userRoles);
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(securityRoles);

            var result = await _service.GetRoleNamesForUser(1, 7);

            Assert.Single(result);
            Assert.Equal("Admin", result[0]);
        }

        [Fact]
        public async Task GetRoleNameById_ReturnsName()
        {
            var securityRoles = new List<SecurityRole>
            {
                new() { RoleId = 3, TenantId = 2, RoleName = "Admin" }
            }.BuildMock();
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(securityRoles);

            var result = await _service.GetRoleNameById(3, 2);

            Assert.Equal("Admin", result);
        }

        [Fact]
        public async Task GetRoleNamesByIds_ReturnsDictionary()
        {
            var securityRoles = new List<SecurityRole>
            {
                new() { RoleId = 1, TenantId = 2, RoleName = "Admin" },
                new() { RoleId = 2, TenantId = 0, RoleName = "User" }
            }.BuildMock();
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(securityRoles);

            var result = await _service.GetRoleNamesByIds([1, 2], 2);

            Assert.Equal(2, result.Count);
            Assert.Equal("Admin", result[1]);
            Assert.Equal("User", result[2]);
        }

        [Fact]
        public async Task Remove_RemovesAndCompletes()
        {
            var entity = new UserRole { UserId = 1, RoleId = 2 };
            _mockUserRoleRepo.Setup(r => r.GetById(5)).Returns(entity);

            await _service.Remove(5);

            _mockUserRoleRepo.Verify(r => r.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void GetRank_MapsValues()
        {
            Assert.Equal(Rank.SuperAdmin, _service.GetRank("Super Admin"));
            Assert.Equal(Rank.Admin, _service.GetRank("Admin"));
            Assert.Equal(Rank.User, _service.GetRank("User"));
            Assert.Equal(Rank.None, _service.GetRank("Other"));
        }

        [Fact]
        public void GetHighestRank_ReturnsHighest()
        {
            var result = _service.GetHighestRank(["User", "Admin"]);

            Assert.Equal(Rank.Admin, result);
        }

        [Fact]
        public async Task EnsureCanManageUserRole_WhenRoleNotFound_ReturnsError()
        {
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>().BuildMock());

            var result = await _service.EnsureCanManageUserRole(1, 2, 3, "assign");

            Assert.False(result.IsValid);
            Assert.Equal("Role not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task EnsureCanManageUserRole_WhenTargetSuperAdminAndCurrentNot_ReturnsError()
        {
            var roles = new List<SecurityRole>
            {
                new() { RoleId = 1, TenantId = 5, RoleName = "Super Admin" }
            }.BuildMock();
            var userRoles = new List<UserRole>
            {
                new() { UserId = 9, RoleId = 2 }
            }.BuildMock();
            var securityRoles = new List<SecurityRole>
            {
                new() { RoleId = 2, TenantId = 5, RoleName = "User" },
                new() { RoleId = 1, TenantId = 5, RoleName = "Super Admin" }
            }.BuildMock();
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(securityRoles);
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(userRoles);

            var result = await _service.EnsureCanManageUserRole(1, 5, 9, "assign");

            Assert.False(result.IsValid);
            Assert.Contains("Only Super Admin", result.ErrorMessage);
        }

        [Fact]
        public async Task EnsureCanManageUserRole_WhenRemovingLastSuperAdmin_ReturnsError()
        {
            var userRoles = new List<UserRole>
            {
                new() { UserId = 1, RoleId = 1, TenantId = 5 }
            }.BuildMock();
            var securityRoles = new List<SecurityRole>
            {
                new() { RoleId = 1, TenantId = 5, RoleName = "Super Admin" }
            }.BuildMock();
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(userRoles);
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(securityRoles);

            var result = await _service.EnsureCanManageUserRole(1, 5, 1, "remove users from");

            Assert.False(result.IsValid);
            Assert.Equal("You cannot remove the last Super Admin user.", result.ErrorMessage);
        }

        [Fact]
        public async Task EnsureCanManageUserRole_WhenTargetAdminAndCurrentUser_ReturnsError()
        {
            var userRoles = new List<UserRole>
            {
                new() { UserId = 5, RoleId = 3, TenantId = 2 }
            }.BuildMock();
            var securityRoles = new List<SecurityRole>
            {
                new() { RoleId = 2, TenantId = 2, RoleName = "Admin" },
                new() { RoleId = 3, TenantId = 2, RoleName = "User" }
            }.BuildMock();
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(userRoles);
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(securityRoles);

            var result = await _service.EnsureCanManageUserRole(2, 2, 5, "assign");

            Assert.False(result.IsValid);
            Assert.Contains("Only Admin or Super Admin", result.ErrorMessage);
        }

        [Fact]
        public async Task EnsureCanManageUserRole_WhenAllowed_ReturnsSuccess()
        {
            var userRoles = new List<UserRole>
            {
                new() { UserId = 5, RoleId = 2, TenantId = 2 }
            }.BuildMock();
            var securityRoles = new List<SecurityRole>
            {
                new() { RoleId = 2, TenantId = 2, RoleName = "Admin" }
            }.BuildMock();
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(userRoles);
            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(securityRoles);

            var result = await _service.EnsureCanManageUserRole(2, 2, 5, "assign");

            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public async Task RemoveUserRole_WhenFound_RemovesAndClearsCache()
        {
            var entity = new UserRole { UserId = 7, RoleId = 9 };
            _mockUserRoleRepo.Setup(r => r.DeleteUserRoleAsync(7, 9)).ReturnsAsync(entity);

            await _service.RemoveUserRole(7, 9);

            _mockUserRoleRepo.Verify(r => r.Remove(entity), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
            _mockUserService.Verify(s => s.RemoveUserPermissionsCache(7), Times.Once);
        }

        [Fact]
        public async Task RemoveUserRole_WhenNotFound_Throws()
        {
            _mockUserRoleRepo.Setup(r => r.DeleteUserRoleAsync(7, 9)).ReturnsAsync((UserRole?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.RemoveUserRole(7, 9));
        }
    }
}
