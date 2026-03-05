using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace EligibilityPlatform.Tests.Services
{
    public class EligibleProductsServiceApiTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ILogger<EligibleProductsService>> _mockLogger;

        public EligibleProductsServiceApiTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<EligibleProductsService>>();
        }

        [Fact]
        public async Task CallExternalApiAsync_GetWithPayload_ShouldAppendQueryAndReturnMessage()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            HttpRequestMessage? capturedRequest = null;

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(string.Empty)
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new EligibleProductsService(_mockUow.Object, _mockLogger.Object, new TestHttpClientFactory(httpClient));

            var result = await service.CallExternalApiAsync(
                "http://test/api",
                "GET",
                new Dictionary<string, object> { { "a", "1" } },
                0);

            Assert.NotNull(capturedRequest);
            Assert.Contains("a=1", capturedRequest!.RequestUri!.ToString());
            Assert.Contains("No response returned", result);
        }

        [Fact]
        public async Task CallExternalApiAsync_NonSuccess_ShouldReturnErrorJson()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new EligibleProductsService(_mockUow.Object, _mockLogger.Object, new TestHttpClientFactory(httpClient));

            var result = await service.CallExternalApiAsync("http://test/api", "POST", new { x = 1 }, 0);

            Assert.Contains("\"Success\":false", result);
            Assert.Contains("\"StatusCode\":400", result);
        }

        [Fact]
        public async Task CallExternalApiAsync_WhenSendThrows_ShouldReturnErrorJson()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network down"));

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new EligibleProductsService(_mockUow.Object, _mockLogger.Object, new TestHttpClientFactory(httpClient));

            var result = await service.CallExternalApiAsync("http://test/api", "POST", new { x = 1 }, 0);

            Assert.Contains("\"Success\":false", result);
            Assert.Contains("Network down", result);
        }

        [Fact]
        public async Task CallExternalApiWithMappingAsync_ShouldMapPayloadAndCallApi()
        {
            var nodeApiRepo = new Mock<INodeApiRepository>();
            var nodeModelRepo = new Mock<INodeModelRepository>();
            var apiParamsRepo = new Mock<IApiParametersRepository>();
            var apiParamMapsRepo = new Mock<IApiParameterMapsRepository>();
            var paramRepo = new Mock<IParameterRepository>();

            _mockUow.Setup(u => u.NodeApiRepository).Returns(nodeApiRepo.Object);
            _mockUow.Setup(u => u.NodeModelRepository).Returns(nodeModelRepo.Object);
            _mockUow.Setup(u => u.ApiParametersRepository).Returns(apiParamsRepo.Object);
            _mockUow.Setup(u => u.ApiParameterMapsRepository).Returns(apiParamMapsRepo.Object);
            _mockUow.Setup(u => u.ParameterRepository).Returns(paramRepo.Object);

            nodeApiRepo.Setup(r => r.GetAll()).Returns(new List<NodeApi>
            {
                new() { Apiid = 10, Apiname = "foo", NodeId = 1, HttpMethodType = "POST", IsActive = true, ExecutionOrder = 1 }
            });

            nodeModelRepo.Setup(r => r.GetAll()).Returns(new List<Node>
            {
                new() { NodeId = 1, NodeUrl = "http://test" }
            });

            apiParamsRepo.Setup(r => r.GetAll()).Returns(new List<ApiParameter>
            {
                new() { ApiParamterId = 100, ApiId = 10, ParameterName = "ParamA", ParameterDirection = "Input", DefaultValue = "DEF" }
            });

            apiParamMapsRepo.Setup(r => r.GetAll()).Returns(new List<ApiParameterMap>
            {
                new() { ApiParameterId = 100, ParameterId = 5 }
            });

            paramRepo.Setup(r => r.GetAll()).Returns(new List<Parameter>
            {
                new() { ParameterId = 5, ParameterName = "InternalParam" }
            });

            string? capturedPayload = null;
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>(async (req, _) =>
                {
                    if (req.Content != null)
                        capturedPayload = await req.Content.ReadAsStringAsync();
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"ok\":true}", Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new EligibleProductsService(_mockUow.Object, _mockLogger.Object, new TestHttpClientFactory(httpClient));

            var request = new DynamicApiRequest
            {
                Url = "http://test/api",
                HttpMethod = "POST",
                Payload = new Dictionary<string, object>
                {
                    { "InternalParam", "X" },
                    { "Extra", "Y" }
                }
            };

            var result = await service.CallExternalApiWithMappingAsync(request);

            Assert.Contains("\"ok\":true", result);
            Assert.NotNull(capturedPayload);
            Assert.Contains("ParamA", capturedPayload!);
            Assert.Contains("Extra", capturedPayload!);
        }

        [Fact]
        public void TryGetPayloadValue_ObjectPayload_ShouldDeserialize()
        {
            var service = new EligibleProductsService(_mockUow.Object, _mockLogger.Object, new TestHttpClientFactory(new HttpClient()));
            var method = typeof(EligibleProductsService).GetMethod("TryGetPayloadValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            var args = new object?[] { new { InternalParam = "X" }, "InternalParam", null };
            var success = (bool)method!.Invoke(service, args)!;

            Assert.True(success);
            Assert.Equal("X", args[2]!.ToString());
        }

        [Fact]
        public async Task CallExternalApiAsync_WithHeadersAndEvaluation_ShouldLog()
        {
            var evalRepo = new Mock<IIntegrationApiEvaluationRepository>();
            _mockUow.Setup(u => u.IntegrationApiEvaluationRepository).Returns(evalRepo.Object);
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"ok\":true}", Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new EligibleProductsService(_mockUow.Object, _mockLogger.Object, new TestHttpClientFactory(httpClient));

            var headers = new Dictionary<string, string>
            {
                { "X-Test", "1" },
                { "", "skip" }
            };

            var evaluation = new EvaluationHistory { EvaluationHistoryId = 99 };

            var result = await service.CallExternalApiAsync(
                "http://test/api",
                "POST",
                new Dictionary<string, object> { { " A ", "1" } },
                10,
                evaluation,
                JsonSerializer.Serialize(headers));

            Assert.Contains("\"ok\":true", result);
            evalRepo.Verify(r => r.Add(It.IsAny<IntegrationApiEvaluation>(), It.IsAny<bool>()), Times.Once);
            _mockUow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public void TryGetPayloadValue_NormalizedKey_ShouldMatch()
        {
            var service = new EligibleProductsService(_mockUow.Object, _mockLogger.Object, new TestHttpClientFactory(new HttpClient()));
            var method = typeof(EligibleProductsService).GetMethod("TryGetPayloadValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            var payload = new Dictionary<string, object> { { "My-Key", "V" } };
            var args = new object?[] { payload, "My Key", null };
            var success = (bool)method!.Invoke(service, args)!;

            Assert.True(success);
            Assert.Equal("V", args[2]);
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
