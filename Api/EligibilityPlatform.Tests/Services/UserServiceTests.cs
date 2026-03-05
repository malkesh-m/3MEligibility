using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MapsterMapper;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IEmailService> _mockEmail;
        private readonly Mock<ITokenService> _mockToken;
        private readonly Mock<ILdapService> _mockLdap;
        private readonly MemoryCache _cache;

        public UserServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockEmail = new Mock<IEmailService>();
            _mockToken = new Mock<ITokenService>();
            _mockLdap = new Mock<ILdapService>();
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public async Task GetAll_ReturnsData()
        {
            var expected = new ApiResponse<List<UserGetModel>>
            {
                Data = [new UserGetModel { Id = 1, TenantId = 2 }],
                Succeeded = true
            };
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            HttpRequestMessage? captured = null;
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(expected))
                });

            var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("http://test.local/") };
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MIdentityAPI:Version"] = "2"
            }).Build();

            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                config,
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(httpClient));

            var result = await service.GetAll(2);

            Assert.Single(result.Data);
            Assert.Equal("/api/v2/Users/GetAllByTenantId/2", captured?.RequestUri?.AbsolutePath);
        }

        [Fact]
        public async Task GetById_ReturnsData()
        {
            var expected = new ApiResponse<UserGetModel>
            {
                Data = new UserGetModel { Id = 10, TenantId = 3 },
                Succeeded = true
            };
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(expected))
                });

            var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("http://test.local/") };
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MIdentityAPI:Version"] = "1"
            }).Build();

            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                config,
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(httpClient));

            var result = await service.GetById(10);

            Assert.Equal(10, result.Data.Id);
        }

        [Fact]
        public async Task GetRolesByRoleIds_ReturnsPermissions()
        {
            var rolePermissionRepo = new Mock<IRolePermissionRepository>();
            rolePermissionRepo.Setup(r => r.GetRolePermissions(1)).ReturnsAsync([1, 2]);
            rolePermissionRepo.Setup(r => r.GetRolePermissions(2)).ReturnsAsync([2, 3]);
            var permissionRepo = new Mock<IPermissionRepository>();
            permissionRepo.Setup(r => r.Query()).Returns(new List<Permission>
            {
                new() { PermissionId = 1, PermissionAction = "A" },
                new() { PermissionId = 2, PermissionAction = "B" },
                new() { PermissionId = 3, PermissionAction = "C" }
            }.BuildMock());

            _mockUow.Setup(u => u.RolePermissionRepository).Returns(rolePermissionRepo.Object);
            _mockUow.Setup(u => u.PermissionRepository).Returns(permissionRepo.Object);

            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                new ConfigurationBuilder().Build(),
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(new HttpClient()));

            var result = await service.GetRolesByRoleIds([new RoleModel { RoleId = 1, RoleName = "A" }, new RoleModel { RoleId = 2, RoleName = "B" }]);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetRolesByRoleIds_WhenNoPermissions_ReturnsEmpty()
        {
            var rolePermissionRepo = new Mock<IRolePermissionRepository>();
            rolePermissionRepo.Setup(r => r.GetRolePermissions(1)).ReturnsAsync([]);
            var permissionRepo = new Mock<IPermissionRepository>();

            _mockUow.Setup(u => u.RolePermissionRepository).Returns(rolePermissionRepo.Object);
            _mockUow.Setup(u => u.PermissionRepository).Returns(permissionRepo.Object);

            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                new ConfigurationBuilder().Build(),
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(new HttpClient()));

            var result = await service.GetRolesByRoleIds([new RoleModel { RoleId = 1, RoleName = "A" }]);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPermissionsByRoleId_ReturnsPermissions()
        {
            var rolePermissionRepo = new Mock<IRolePermissionRepository>();
            rolePermissionRepo.Setup(r => r.GetRolePermissions(1)).ReturnsAsync([2]);
            var permissionRepo = new Mock<IPermissionRepository>();
            permissionRepo.Setup(r => r.Query()).Returns(new List<Permission>
            {
                new() { PermissionId = 2, PermissionAction = "B" }
            }.BuildMock());

            _mockUow.Setup(u => u.RolePermissionRepository).Returns(rolePermissionRepo.Object);
            _mockUow.Setup(u => u.PermissionRepository).Returns(permissionRepo.Object);

            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                new ConfigurationBuilder().Build(),
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(new HttpClient()));

            var result = await service.GetPermissionsByRoleId(1);

            Assert.Single(result);
            Assert.Equal(2, result[0].PermissionId);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_ReturnsAndCaches()
        {
            var userRoleRepo = new Mock<IUserRoleRepository>();
            var rolePermissionRepo = new Mock<IRolePermissionRepository>();
            var permissionRepo = new Mock<IPermissionRepository>();
            userRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>
            {
                new() { UserId = 7, RoleId = 1, TenantId = 2 }
            }.BuildMock());
            rolePermissionRepo.Setup(r => r.Query()).Returns(new List<RolePermission>
            {
                new() { RoleId = 1, TenantId = 2, PermissionId = 10 }
            }.BuildMock());
            permissionRepo.Setup(r => r.Query()).Returns(new List<Permission>
            {
                new() { PermissionId = 10, PermissionAction = "X" }
            }.BuildMock());

            _mockUow.Setup(u => u.UserRoleRepository).Returns(userRoleRepo.Object);
            _mockUow.Setup(u => u.RolePermissionRepository).Returns(rolePermissionRepo.Object);
            _mockUow.Setup(u => u.PermissionRepository).Returns(permissionRepo.Object);

            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                new ConfigurationBuilder().Build(),
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(new HttpClient()));

            var result1 = await service.GetUserPermissionsAsync(7, 2);
            var result2 = await service.GetUserPermissionsAsync(7, 2);

            Assert.Single(result1);
            Assert.Single(result2);
            Assert.Equal("X", result1[0]);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_WhenCached_ReturnsWithoutQuery()
        {
            _cache.Set("PERMISSIONS_USER_5", new List<string> { "A" });
            var rolePermissionRepo = new Mock<IRolePermissionRepository>();
            rolePermissionRepo.Setup(r => r.Query()).Throws(new Exception("should not query"));
            _mockUow.Setup(u => u.RolePermissionRepository).Returns(rolePermissionRepo.Object);
            _mockUow.Setup(u => u.UserRoleRepository).Returns(new Mock<IUserRoleRepository>().Object);
            _mockUow.Setup(u => u.PermissionRepository).Returns(new Mock<IPermissionRepository>().Object);

            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                new ConfigurationBuilder().Build(),
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(new HttpClient()));

            var result = await service.GetUserPermissionsAsync(5, 1);

            Assert.Single(result);
            Assert.Equal("A", result[0]);
        }

        [Fact]
        public void RemoveUserPermissionsCache_RemovesEntry()
        {
            _cache.Set("PERMISSIONS_USER_9", new List<string> { "A" });
            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                new ConfigurationBuilder().Build(),
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(new HttpClient()));

            service.RemoveUserPermissionsCache(9);

            Assert.False(_cache.TryGetValue("PERMISSIONS_USER_9", out _));
        }

        [Fact]
        public async Task GetUserRolesAsync_ReturnsRoles()
        {
            var userRoleRepo = new Mock<IUserRoleRepository>();
            var securityRoleRepo = new Mock<ISecurityRoleRepository>();
            userRoleRepo.Setup(r => r.Query()).Returns(new List<UserRole>
            {
                new() { UserId = 1, RoleId = 2 }
            }.BuildMock());
            securityRoleRepo.Setup(r => r.Query()).Returns(new List<SecurityRole>
            {
                new() { RoleId = 2, TenantId = 3, RoleName = "Admin" }
            }.BuildMock());
            _mockUow.Setup(u => u.UserRoleRepository).Returns(userRoleRepo.Object);
            _mockUow.Setup(u => u.SecurityRoleRepository).Returns(securityRoleRepo.Object);

            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                new ConfigurationBuilder().Build(),
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(new HttpClient()));

            var method = typeof(UserService).GetMethod("GetUserRolesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<List<RoleModel>>)method!.Invoke(service, new object[] { 1, 3 })!;
            var result = await task;

            Assert.Single(result);
            Assert.Equal("Admin", result[0].RoleName);
        }

        [Fact]
        public async Task GetUserRolesAsync_WhenQueryFails_ReturnsEmpty()
        {
            var userRoleRepo = new Mock<IUserRoleRepository>();
            userRoleRepo.Setup(r => r.Query()).Throws(new Exception("fail"));
            _mockUow.Setup(u => u.UserRoleRepository).Returns(userRoleRepo.Object);
            _mockUow.Setup(u => u.SecurityRoleRepository).Returns(new Mock<ISecurityRoleRepository>().Object);

            var service = new UserService(
                _mockUow.Object,
                _mockMapper.Object,
                new ConfigurationBuilder().Build(),
                _mockEmail.Object,
                _mockToken.Object,
                _mockLdap.Object,
                _cache,
                new TestHttpClientFactory(new HttpClient()));

            var method = typeof(UserService).GetMethod("GetUserRolesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<List<RoleModel>>)method!.Invoke(service, new object[] { 1, 3 })!;
            var result = await task;

            Assert.Empty(result);
        }

        [Fact]
        public void GetUserRoles_ReturnsRoleFromClaims()
        {
            var claims = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
            }, "test");
            var principal = new System.Security.Claims.ClaimsPrincipal(claims);
            var method = typeof(UserService).GetMethod("GetUserRoles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (string?)method!.Invoke(null, new object[] { principal });

            Assert.Equal("Admin", result);
        }

        private sealed class TestHttpClientFactory(HttpClient client) : IHttpClientFactory
        {
            private readonly HttpClient _client = client;

            public HttpClient CreateClient(string name)
            {
                return _client;
            }
        }
    }
}
