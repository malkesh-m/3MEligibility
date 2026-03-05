using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using Moq.Protected;
using MEligibilityPlatform.Application.Repository;

namespace EligibilityPlatform.Tests.Services
{
    public class TenantOnboardingServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly TenantOnboardingService _service;

        public TenantOnboardingServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockUserService = new Mock<IUserService>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockConfiguration = new Mock<IConfiguration>();
            _service = new TenantOnboardingService(
                _mockUow.Object, 
                _mockUserService.Object, 
                _mockHttpClientFactory.Object, 
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetById_ValidResponse_ReturnsData()
        {
            var tenantId = 1;
            var expectedResponse = new ApiResponse<TenantModel> { Data = new TenantModel { Id = tenantId }, Succeeded = true };
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/")
            };
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _mockConfiguration.Setup(c => c["MIdentityAPI:Version"]).Returns("1");

            var result = await _service.GetById(tenantId);

            Assert.NotNull(result.Data);
            Assert.Equal(tenantId, result.Data.Id);
        }

        [Fact]
        public async Task OnboardNewTenantAsync_Success_ReturnsSuccessResult()
        {
            var request = new TenantOnboardingRequest { TenantId = 1, AdminUserId = 1 };
            
            // Mock GetById (via HttpClient)
            var tenantResponse = new ApiResponse<TenantModel> { Data = new TenantModel { Id = 1 }, Succeeded = true };
            
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { 
                    StatusCode = HttpStatusCode.OK, 
                    Content = new StringContent(JsonSerializer.Serialize(tenantResponse)) 
                });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com/") };
            _mockHttpClientFactory.Setup(_ => _.CreateClient("MIdentityAPI")).Returns(httpClient);
            _mockConfiguration.Setup(c => c["MIdentityAPI:Version"]).Returns("1");

            // Mock UserService
            _mockUserService.Setup(s => s.GetById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserGetModel> { Data = new UserGetModel { TenantId = 1 }, Succeeded = true });

            // Mock UoW
            _mockUow.Setup(u => u.SecurityRoleRepository.Query()).Returns(new List<SecurityRole>().BuildMock());
            _mockUow.Setup(u => u.PermissionRepository.Query()).Returns(new List<Permission>().BuildMock());
            _mockUow.Setup(u => u.DataTypeRepository.Query()).Returns(new List<DataType>
            {
                new() { DataTypeId = 1, DataTypeName = "Numeric" },
                new() { DataTypeId = 2, DataTypeName = "Text" }
            }.BuildMock());
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(new List<Category>().BuildMock());
            var parameterRepo = new Mock<IParameterRepository>();
            _mockUow.Setup(u => u.ParameterRepository).Returns(parameterRepo.Object);
            
            var result = await _service.OnboardNewTenantAsync(request);

            Assert.True(result.Success);
            parameterRepo.Verify(r => r.AddRange(It.IsAny<IEnumerable<Parameter>>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task OnboardNewTenantAsync_TenantNotFound_ReturnsError()
        {
            var request = new TenantOnboardingRequest { TenantId = 1, AdminUserId = 1 };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}")
                });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com/") };
            _mockHttpClientFactory.Setup(_ => _.CreateClient("MIdentityAPI")).Returns(httpClient);
            _mockConfiguration.Setup(c => c["MIdentityAPI:Version"]).Returns("1");

            var result = await _service.OnboardNewTenantAsync(request);

            Assert.False(result.Success);
            Assert.Contains("Tenant not found.", result.Errors);
        }

        [Fact]
        public async Task OnboardNewTenantAsync_AdminUserNotInTenant_ReturnsError()
        {
            var request = new TenantOnboardingRequest { TenantId = 1, AdminUserId = 10 };

            var tenantResponse = new ApiResponse<TenantModel> { Data = new TenantModel { Id = 1 }, Succeeded = true };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tenantResponse))
                });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com/") };
            _mockHttpClientFactory.Setup(_ => _.CreateClient("MIdentityAPI")).Returns(httpClient);
            _mockConfiguration.Setup(c => c["MIdentityAPI:Version"]).Returns("1");

            _mockUserService.Setup(s => s.GetById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserGetModel> { Data = new UserGetModel { TenantId = 2 }, Succeeded = true });

            var result = await _service.OnboardNewTenantAsync(request);

            Assert.False(result.Success);
            Assert.Contains("does not belong", result.Errors.First());
        }

        [Fact]
        public async Task OnboardNewTenantAsync_AlreadyOnboarded_ReturnsEligibilityError()
        {
            var request = new TenantOnboardingRequest { TenantId = 1, AdminUserId = 1 };

            var tenantResponse = new ApiResponse<TenantModel> { Data = new TenantModel { Id = 1 }, Succeeded = true };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tenantResponse))
                });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com/") };
            _mockHttpClientFactory.Setup(_ => _.CreateClient("MIdentityAPI")).Returns(httpClient);
            _mockConfiguration.Setup(c => c["MIdentityAPI:Version"]).Returns("1");

            _mockUserService.Setup(s => s.GetById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserGetModel> { Data = new UserGetModel { TenantId = 1 }, Succeeded = true });

            _mockUow.Setup(u => u.SecurityRoleRepository.Query())
                .Returns(new List<SecurityRole> { new() { TenantId = 1 } }.BuildMock());

            var result = await _service.OnboardNewTenantAsync(request);

            Assert.False(result.Success);
            Assert.Contains("Core security configuration already exists", result.Errors.First());
        }

        [Fact]
        public async Task ValidateTenantSetupAsync_WhenRolesAndCategoriesExist_ReturnsTrue()
        {
            _mockUow.Setup(u => u.SecurityRoleRepository.Query())
                .Returns(new List<SecurityRole> { new() { TenantId = 1 } }.BuildMock());
            _mockUow.Setup(u => u.CategoryRepository.Query())
                .Returns(new List<Category> { new() { TenantId = 1 } }.BuildMock());

            var result = await _service.ValidateTenantSetupAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task ValidateTenantSetupAsync_WhenMissingCategories_ReturnsFalse()
        {
            _mockUow.Setup(u => u.SecurityRoleRepository.Query())
                .Returns(new List<SecurityRole> { new() { TenantId = 1 } }.BuildMock());
            _mockUow.Setup(u => u.CategoryRepository.Query())
                .Returns(new List<Category>().BuildMock());

            var result = await _service.ValidateTenantSetupAsync(1);

            Assert.False(result);
        }
    }
}
