using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;
using EligibilityPlatform.Tests.Helpers;
using System.Text.Json;

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

            _mockConfiguration.Setup(c => c["MIdentityAPI:Version"]).Returns("1");

            _service = new TenantOnboardingService(
                _mockUow.Object,
                _mockUserService.Object,
                _mockHttpClientFactory.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task OnboardNewTenantAsync_ShouldReturnError_WhenTenantNotFound()
        {
            var request = new TenantOnboardingRequest { TenantId = 1, AdminUserId = 1 };
            
            // Mock HttpClient to return no data
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new ApiResponse<TenantModel> { Data = null }))
                });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.example.com") };
            _mockHttpClientFactory.Setup(f => f.CreateClient("MIdentityAPI")).Returns(httpClient);

            var result = await _service.OnboardNewTenantAsync(request);

            Assert.Contains("Tenant not found.", result.Errors);
        }

        [Fact]
        public async Task OnboardNewTenantAsync_ShouldReturnError_WhenAdminUserInvalid()
        {
            var request = new TenantOnboardingRequest { TenantId = 1, AdminUserId = 1 };

            // Mock HttpClient to return valid tenant
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new ApiResponse<TenantModel> { Data = new TenantModel() }))
                });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.example.com") };
            _mockHttpClientFactory.Setup(f => f.CreateClient("MIdentityAPI")).Returns(httpClient);

            // Mock UserService to return invalid user
            _mockUserService.Setup(s => s.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserGetModel> { Data = null });

            var result = await _service.OnboardNewTenantAsync(request);

            Assert.Contains("The specified admin user does not belong to this tenant.", result.Errors);
        }

        [Fact]
        public async Task OnboardNewTenantAsync_ShouldReturnError_WhenTenantAlreadyOnboarded()
        {
            var request = new TenantOnboardingRequest { TenantId = 1, AdminUserId = 1 };

            // Mock HTTP Client
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new ApiResponse<TenantModel> { Data = new TenantModel() }))
                });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.example.com") };
            _mockHttpClientFactory.Setup(f => f.CreateClient("MIdentityAPI")).Returns(httpClient);

            // Mock UserService
            _mockUserService.Setup(s => s.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponse<UserGetModel> 
                { 
                    Data = new UserGetModel { TenantId = 1, FirstName = "admin", LastName = "admin", Email = "admin@test.com" } 
                });

            // Mock already onboarded
            _mockUow.Setup(u => u.SecurityRoleRepository.Query()).Returns(new List<SecurityRole> { new SecurityRole { TenantId = 1 } }.BuildMock());

            var result = await _service.OnboardNewTenantAsync(request);

            Assert.False(result.Success);
            Assert.Contains("Core security configuration already exists for this tenant.", result.Errors);
        }

        [Fact]
        public async Task ValidateTenantSetupAsync_ShouldReturnTrue_WhenSetupIsComplete()
        {
            _mockUow.Setup(u => u.SecurityRoleRepository.Query()).Returns(new List<SecurityRole> { new SecurityRole { TenantId = 1 } }.BuildMock());
            _mockUow.Setup(u => u.CategoryRepository.Query()).Returns(new List<Category> { new Category { TenantId = 1 } }.BuildMock());

            var result = await _service.ValidateTenantSetupAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task ValidateTenantSetupAsync_ShouldReturnFalse_WhenSetupIsIncomplete()
        {
            _mockUow.Setup(u => u.SecurityRoleRepository.Query()).Returns(new List<SecurityRole>().AsQueryable());

            var result = await _service.ValidateTenantSetupAsync(1);

            Assert.False(result);
        }
    }
}
