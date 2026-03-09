using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Constants;
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
    public class RolePermissionServiceSimplifiedTests
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

        public RolePermissionServiceSimplifiedTests()
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
        public async Task Remove_ScreenWithSharedView_PreservesView()
        {
            // Setup: Role has Rule.Screen AND ECard.Screen. Both need Rule.View.
            var roleId = 1;
            var tenantId = 1;

            var pRuleScreen = new Permission { PermissionId = 1, PermissionAction = Permissions.Rule.Screen };
            var pRuleView = new Permission { PermissionId = 2, PermissionAction = Permissions.Rule.View };
            var pEcardScreen = new Permission { PermissionId = 3, PermissionAction = Permissions.ECard.Screen };

            var allPermissions = new List<Permission> { pRuleScreen, pRuleView, pEcardScreen };
            _mockPermissionRepo.Setup(p => p.Query()).Returns(allPermissions.AsQueryable().BuildMock());

            var assigned = new List<RolePermission>
            {
                new() { RoleId = roleId, TenantId = tenantId, PermissionId = 1, Permission = pRuleScreen },
                new() { RoleId = roleId, TenantId = tenantId, PermissionId = 2, Permission = pRuleView },
                new() { RoleId = roleId, TenantId = tenantId, PermissionId = 3, Permission = pEcardScreen }
            };
            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(assigned.AsQueryable().BuildMock());

            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole> {
                new() { RoleId = roleId, TenantId = tenantId, RoleName = "Test" }
            }.AsQueryable().BuildMock());
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().AsQueryable().BuildMock());

            // Act: Remove Rule.Screen
            var model = new RolePermissionModel { RoleId = roleId, TenantId = tenantId, PermissionIds = [1] };
            await _service.Remove(model);

            // Assert: Rule.Screen is removed, but Rule.View is NOT (it's still needed by ECard.Screen)
            _mockRolePermissionRepo.Verify(r => r.RemoveRange(It.Is<IEnumerable<RolePermission>>(irp => 
                irp.Count() == 1 &&
                irp.Any(x => x.PermissionId == 1) &&
                !irp.Any(x => x.PermissionId == 2)
            )), Times.Once);
        }

        [Fact]
        public async Task Remove_LastScreenNeedingView_CleansUpView()
        {
            // Setup: Role has Rule.Screen and Rule.View ONLY.
            var roleId = 1;
            var tenantId = 1;

            var pRuleScreen = new Permission { PermissionId = 1, PermissionAction = Permissions.Rule.Screen };
            var pRuleView = new Permission { PermissionId = 2, PermissionAction = Permissions.Rule.View };

            var allPermissions = new List<Permission> { pRuleScreen, pRuleView };
            _mockPermissionRepo.Setup(p => p.Query()).Returns(allPermissions.AsQueryable().BuildMock());

            var assigned = new List<RolePermission>
            {
                new() { RoleId = roleId, TenantId = tenantId, PermissionId = 1, Permission = pRuleScreen },
                new() { RoleId = roleId, TenantId = tenantId, PermissionId = 2, Permission = pRuleView }
            };
            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(assigned.AsQueryable().BuildMock());

            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole> {
                new() { RoleId = roleId, TenantId = tenantId, RoleName = "Test" }
            }.AsQueryable().BuildMock());
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().AsQueryable().BuildMock());

            // Act: Remove Rule.Screen
            var model = new RolePermissionModel { RoleId = roleId, TenantId = tenantId, PermissionIds = [1] };
            await _service.Remove(model);

            // Assert: BOTH are removed because Rule.View is no longer needed by "what's left" (nothing left).
            _mockRolePermissionRepo.Verify(r => r.RemoveRange(It.Is<IEnumerable<RolePermission>>(irp => 
                irp.Count() == 2 &&
                irp.Any(x => x.PermissionId == 1) &&
                irp.Any(x => x.PermissionId == 2)
            )), Times.Once);
        }

        [Fact]
        public async Task Remove_WhenEditStillAssigned_PreservesView()
        {
            // Setup: Role has Rule.Edit and Rule.Screen. Both need Rule.View.
            var roleId = 1;
            var tenantId = 1;

            var pRuleScreen = new Permission { PermissionId = 1, PermissionAction = Permissions.Rule.Screen };
            var pRuleView = new Permission { PermissionId = 2, PermissionAction = Permissions.Rule.View };
            var pRuleEdit = new Permission { PermissionId = 3, PermissionAction = Permissions.Rule.Edit };

            var allPermissions = new List<Permission> { pRuleScreen, pRuleView, pRuleEdit };
            _mockPermissionRepo.Setup(p => p.Query()).Returns(allPermissions.AsQueryable().BuildMock());

            var assigned = new List<RolePermission>
            {
                new() { RoleId = roleId, TenantId = tenantId, PermissionId = 1, Permission = pRuleScreen },
                new() { RoleId = roleId, TenantId = tenantId, PermissionId = 2, Permission = pRuleView },
                new() { RoleId = roleId, TenantId = tenantId, PermissionId = 3, Permission = pRuleEdit }
            };
            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(assigned.AsQueryable().BuildMock());

            _mockSecurityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole> {
                new() { RoleId = roleId, TenantId = tenantId, RoleName = "Test" }
            }.AsQueryable().BuildMock());
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().AsQueryable().BuildMock());

            // Act: Remove Rule.Screen
            var model = new RolePermissionModel { RoleId = roleId, TenantId = tenantId, PermissionIds = [1] };
            await _service.Remove(model);

            // Assert: Rule.Screen is removed, but Rule.View is KEPT because Rule.Edit is still assigned.
            _mockRolePermissionRepo.Verify(r => r.RemoveRange(It.Is<IEnumerable<RolePermission>>(irp => 
                irp.Count() == 1 &&
                irp.Any(x => x.PermissionId == 1) &&
                !irp.Any(x => x.PermissionId == 2)
            )), Times.Once);
        }

        [Fact]
        public async Task Remove_WhenScreenRemoved_RemovesTechnicalViewsButKeepsHeaders()
        {
            // Arrange
            var roleId = 1;
            var tenantId = 1;

            // Scenario: Role has ManagedLists.Screen, MasterData.Access, and ListItem.View.
            // When ManagedList.Screen is explicitly removed:
            // 1. ManagedList.Screen should be removed (explicit).
            // 2. ListItem.View should be removed (it's orphaned because no .Screen keeps it alive).
            // 3. MasterData.Access should NOT be removed (headers are kept).
            
            var assigned = new List<RolePermission>
            {
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 1 }, // ManagedList.Screen
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 2 }, // ListItem.View
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 3 }  // MasterData.Access
            };

            var permissions = new List<Permission>
            {
                new Permission { PermissionId = 1, PermissionAction = Permissions.ManagedList.Screen },
                new Permission { PermissionId = 2, PermissionAction = Permissions.ListItem.View },
                new Permission { PermissionId = 3, PermissionAction = Permissions.MasterData.Access }
            };

            var securityRoles = new List<SecurityRole> { new SecurityRole { RoleId = roleId, TenantId = tenantId, RoleName = "Admin" } };
            _mockSecurityRoleRepo.Setup(s => s.Query()).Returns(securityRoles.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.SecurityRoleRepository).Returns(_mockSecurityRoleRepo.Object);

            _mockUserContext.Setup(c => c.GetUserId()).Returns(1); // Assuming int userId
            _mockUserRoleService.Setup(s => s.GetRoleNamesForUser(1, tenantId)).ReturnsAsync(new List<string> { "SuperAdmin" });
            _mockUserRoleService.Setup(s => s.GetHighestRank(It.IsAny<List<string>>())).Returns(Rank.SuperAdmin);
            _mockUserRoleService.Setup(s => s.GetRank(It.IsAny<string>())).Returns(Rank.Admin);

            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(assigned.AsQueryable().BuildMock());
            _mockPermissionRepo.Setup(p => p.Query()).Returns(permissions.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.PermissionRepository).Returns(_mockPermissionRepo.Object);
            _mockUow.Setup(u => u.RolePermissionRepository).Returns(_mockRolePermissionRepo.Object);
            _mockUow.Setup(u => u.UserRoleRepository).Returns(_mockUserRoleRepo.Object);
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().AsQueryable().BuildMock());

            var model = new RolePermissionModel
            {
                RoleId = roleId,
                TenantId = tenantId,
                PermissionIds = new List<int> { 1 } // Remove ManagedList.Screen
            };

            // Act
            await _service.Remove(model);

            // Assert
            // ManagedList.Screen (1) AND ListItem.View (2) should be removed.
            // MasterData.Access (3) should NOT be removed!
            _mockRolePermissionRepo.Verify(r => r.RemoveRange(It.Is<IEnumerable<RolePermission>>(irp => 
                irp.Count() == 2 && 
                irp.Any(x => x.PermissionId == 1) && 
                irp.Any(x => x.PermissionId == 2) &&
                !irp.Any(x => x.PermissionId == 3)
            )), Times.Once);
        }

        [Fact]
        public async Task Remove_WhenAccessRemoved_RemovesChildScreensAndOrphanedViews()
        {
            // Arrange
            var roleId = 1;
            var tenantId = 1;

            // Scenario: Role has LimitAndCaps.Access, ProductCap.Screen, ProductCapAmount.Screen, 
            // ProductCap.View, ProductCapAmount.View, Product.View, AND Product.Screen
            // Removing LimitAndCaps.Access should:
            // 1. Remove LimitAndCaps.Access (explicit)
            // 2. Remove ProductCap.Screen & ProductCapAmount.Screen (implicitly expanded from Access)
            // 3. Remove ProductCap.View & ProductCapAmount.View (orphaned technical views)
            // 4. KEEP Product.Screen (not under LimitAndCaps)
            // 5. KEEP Product.View (kept alive by Product.Screen)
            
            var assigned = new List<RolePermission>
            {
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 1 }, // LimitAndCaps.Access
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 2 }, // ProductCap.Screen
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 3 }, // ProductCapAmount.Screen
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 4 }, // ProductCap.View
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 5 }, // ProductCapAmount.View
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 6 }, // Product.View
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 7 }  // Product.Screen (Keeps Product.View alive)
            };

            var permissions = new List<Permission>
            {
                new Permission { PermissionId = 1, PermissionAction = Permissions.LimitAndCaps.Access },
                new Permission { PermissionId = 2, PermissionAction = Permissions.ProductCap.Screen },
                new Permission { PermissionId = 3, PermissionAction = Permissions.ProductCapAmount.Screen },
                new Permission { PermissionId = 4, PermissionAction = Permissions.ProductCap.View },
                new Permission { PermissionId = 5, PermissionAction = Permissions.ProductCapAmount.View },
                new Permission { PermissionId = 6, PermissionAction = Permissions.Product.View },
                new Permission { PermissionId = 7, PermissionAction = Permissions.Product.Screen }
            };

            var securityRoles = new List<SecurityRole> { new SecurityRole { RoleId = roleId, TenantId = tenantId, RoleName = "Admin" } };
            _mockSecurityRoleRepo.Setup(s => s.Query()).Returns(securityRoles.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.SecurityRoleRepository).Returns(_mockSecurityRoleRepo.Object);

            _mockUserContext.Setup(c => c.GetUserId()).Returns(1);
            _mockUserRoleService.Setup(s => s.GetRoleNamesForUser(1, tenantId)).ReturnsAsync(new List<string> { "SuperAdmin" });
            _mockUserRoleService.Setup(s => s.GetHighestRank(It.IsAny<List<string>>())).Returns(Rank.SuperAdmin);
            _mockUserRoleService.Setup(s => s.GetRank(It.IsAny<string>())).Returns(Rank.Admin);

            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(assigned.AsQueryable().BuildMock());
            _mockPermissionRepo.Setup(p => p.Query()).Returns(permissions.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.PermissionRepository).Returns(_mockPermissionRepo.Object);
            _mockUow.Setup(u => u.RolePermissionRepository).Returns(_mockRolePermissionRepo.Object);
            _mockUow.Setup(u => u.UserRoleRepository).Returns(_mockUserRoleRepo.Object);
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().AsQueryable().BuildMock());

            var model = new RolePermissionModel
            {
                RoleId = roleId,
                TenantId = tenantId,
                PermissionIds = new List<int> { 1 } // Remove LimitAndCaps.Access
            };

            // Act
            await _service.Remove(model);

            // Assert
            // Items 1, 2, 3, 4, 5 should be removed.
            // Items 6, 7 should be kept (Product.Screen and Product.View).
            _mockRolePermissionRepo.Verify(r => r.RemoveRange(It.Is<IEnumerable<RolePermission>>(irp => 
                irp.Count() == 5 && 
                irp.Any(x => x.PermissionId == 1) && 
                irp.Any(x => x.PermissionId == 2) &&
                irp.Any(x => x.PermissionId == 3) &&
                irp.Any(x => x.PermissionId == 4) &&
                irp.Any(x => x.PermissionId == 5) &&
                !irp.Any(x => x.PermissionId == 6) &&
                !irp.Any(x => x.PermissionId == 7)
            )), Times.Once);
        }

        [Fact]
        public async Task Remove_WhenScreenRemoved_RemovesFunctionalActionsButFollowsOrphanViewRules()
        {
            // Arrange
            var roleId = 1;
            var tenantId = 1;

            // Scenario: Role has BusinessLogic.Access, Rule.Screen, Rule.Create, Rule.View, ECard.Screen, ECard.View
            // When Rule.Screen is removed:
            // 1. Rule.Screen is explicitly removed.
            // 2. Rule.Create is removed (auto-removed because its parent screen is gone).
            // 3. Rule.View SURVIVES (because ECard.Screen still needs it!)
            // 4. BusinessLogic.Access SURVIVES (because ECard.Screen is still in the module)
            
            var assigned = new List<RolePermission>
            {
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 1 }, // BusinessLogic.Access
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 2 }, // Rule.Screen
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 3 }, // Rule.Create
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 4 }, // Rule.View
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 5 }, // ECard.Screen
                new RolePermission { RoleId = roleId, TenantId = tenantId, PermissionId = 6 }, // ECard.View
            };

            var permissions = new List<Permission>
            {
                new Permission { PermissionId = 1, PermissionAction = Permissions.BusinessLogic.Access },
                new Permission { PermissionId = 2, PermissionAction = Permissions.Rule.Screen },
                new Permission { PermissionId = 3, PermissionAction = Permissions.Rule.Create },
                new Permission { PermissionId = 4, PermissionAction = Permissions.Rule.View },
                new Permission { PermissionId = 5, PermissionAction = Permissions.ECard.Screen },
                new Permission { PermissionId = 6, PermissionAction = Permissions.ECard.View }
            };

            var securityRoles = new List<SecurityRole> { new SecurityRole { RoleId = roleId, TenantId = tenantId, RoleName = "Admin" } };
            _mockSecurityRoleRepo.Setup(s => s.Query()).Returns(securityRoles.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.SecurityRoleRepository).Returns(_mockSecurityRoleRepo.Object);

            _mockUserContext.Setup(c => c.GetUserId()).Returns(1);
            _mockUserRoleService.Setup(s => s.GetRoleNamesForUser(1, tenantId)).ReturnsAsync(new List<string> { "SuperAdmin" });
            _mockUserRoleService.Setup(s => s.GetHighestRank(It.IsAny<List<string>>())).Returns(Rank.SuperAdmin);
            _mockUserRoleService.Setup(s => s.GetRank(It.IsAny<string>())).Returns(Rank.Admin);

            _mockRolePermissionRepo.Setup(r => r.Query()).Returns(assigned.AsQueryable().BuildMock());
            _mockPermissionRepo.Setup(p => p.Query()).Returns(permissions.AsQueryable().BuildMock());
            _mockUow.Setup(u => u.PermissionRepository).Returns(_mockPermissionRepo.Object);
            _mockUow.Setup(u => u.RolePermissionRepository).Returns(_mockRolePermissionRepo.Object);
            _mockUow.Setup(u => u.UserRoleRepository).Returns(_mockUserRoleRepo.Object);
            _mockUserRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>().AsQueryable().BuildMock());

            var model = new RolePermissionModel
            {
                RoleId = roleId,
                TenantId = tenantId,
                PermissionIds = new List<int> { 2 } // Remove Rule.Screen
            };

            // Act
            await _service.Remove(model);

            // Assert
            // Items 2, 3 should be removed (Rule.Screen, Rule.Create).
            // Items 1, 4, 5, 6 should be kept!
            _mockRolePermissionRepo.Verify(r => r.RemoveRange(It.Is<IEnumerable<RolePermission>>(irp => 
                irp.Count() == 2 && 
                !irp.Any(x => x.PermissionId == 1) && 
                irp.Any(x => x.PermissionId == 2) &&
                irp.Any(x => x.PermissionId == 3) &&
                !irp.Any(x => x.PermissionId == 4) &&
                !irp.Any(x => x.PermissionId == 5) &&
                !irp.Any(x => x.PermissionId == 6)
            )), Times.Once);
        }
    }
}

