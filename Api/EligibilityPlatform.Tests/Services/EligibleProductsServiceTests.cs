using Moq;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using EligibilityPlatform.Tests.Helpers;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using Moq.Protected;
using System.Net;
using System.Threading;
using System.Text.Json;

namespace EligibilityPlatform.Tests.Services
{
    public class EligibleProductsServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ILogger<EligibleProductsService>> _mockLogger;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<IIntegrationApiEvaluationRepository> _mockIntegrationApiEvalRepo;
        private readonly EligibleProductsService _service;

        public EligibleProductsServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<EligibleProductsService>>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockIntegrationApiEvalRepo = new Mock<IIntegrationApiEvaluationRepository>();
            _service = new EligibleProductsService(_mockUow.Object, _mockLogger.Object, _mockHttpClientFactory.Object);
        }

        private void SetupDefaultMocks(int tenantId)
        {
            // Create mock objects for all repositories
            var mockProductRepo = new Mock<IProductRepository>();
            var mockProductCapRepo = new Mock<IProductCapRepository>();
            var mockProductCapAmountRepo = new Mock<IProductCapAmountRepository>();
            var mockSettingRepo = new Mock<ISettingRepository>();
            var mockPcardRepo = new Mock<IPcardRepository>();
            var mockEcardRepo = new Mock<IEcardRepository>();
            var mockEruleRepo = new Mock<IEruleRepository>();
            var mockEruleMasterRepo = new Mock<IEruleMasterRepository>();
            var mockFactorRepo = new Mock<IFactorRepository>();
            var mockParameterRepo = new Mock<IParameterRepository>();
            var mockParameterBindingRepo = new Mock<IParameterBindingRepository>();
            var mockConditionRepo = new Mock<IConditionRepository>();
            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockNodeModelRepo = new Mock<INodeModelRepository>();
            var mockNodeApiRepo = new Mock<INodeApiRepository>();
            var mockApiParametersRepo = new Mock<IApiParametersRepository>();
            var mockApiParameterMapsRepo = new Mock<IApiParameterMapsRepository>();
            var mockEvaluationHistoryRepo = new Mock<IEvaluationHistoryRepository>();
            _mockIntegrationApiEvalRepo = new Mock<IIntegrationApiEvaluationRepository>();
            var mockRejectionReasonRepo = new Mock<IRejectionReasonRepository>();

            // Setup UoW to return these mock objects
            _mockUow.Setup(u => u.ProductRepository).Returns(mockProductRepo.Object);
            _mockUow.Setup(u => u.ProductCapRepository).Returns(mockProductCapRepo.Object);
            _mockUow.Setup(u => u.ProductCapAmountRepository).Returns(mockProductCapAmountRepo.Object);
            _mockUow.Setup(u => u.SettingRepository).Returns(mockSettingRepo.Object);
            _mockUow.Setup(u => u.PcardRepository).Returns(mockPcardRepo.Object);
            _mockUow.Setup(u => u.EcardRepository).Returns(mockEcardRepo.Object);
            _mockUow.Setup(u => u.EruleRepository).Returns(mockEruleRepo.Object);
            _mockUow.Setup(u => u.EruleMasterRepository).Returns(mockEruleMasterRepo.Object);
            _mockUow.Setup(u => u.FactorRepository).Returns(mockFactorRepo.Object);
            _mockUow.Setup(u => u.ParameterRepository).Returns(mockParameterRepo.Object);
            _mockUow.Setup(u => u.ParameterBindingRepository).Returns(mockParameterBindingRepo.Object);
            _mockUow.Setup(u => u.ConditionRepository).Returns(mockConditionRepo.Object);
            _mockUow.Setup(u => u.CategoryRepository).Returns(mockCategoryRepo.Object);
            _mockUow.Setup(u => u.NodeModelRepository).Returns(mockNodeModelRepo.Object);
            _mockUow.Setup(u => u.NodeApiRepository).Returns(mockNodeApiRepo.Object);
            _mockUow.Setup(u => u.ApiParametersRepository).Returns(mockApiParametersRepo.Object);
            _mockUow.Setup(u => u.ApiParameterMapsRepository).Returns(mockApiParameterMapsRepo.Object);
            _mockUow.Setup(u => u.EvaluationHistoryRepository).Returns(mockEvaluationHistoryRepo.Object);
            _mockUow.Setup(u => u.IntegrationApiEvaluationRepository).Returns(_mockIntegrationApiEvalRepo.Object);
            _mockUow.Setup(u => u.RejectionReasonRepository).Returns(mockRejectionReasonRepo.Object);

            // Default returns for Query() And methods
            mockProductRepo.Setup(u => u.Query()).Returns(new List<Product>().BuildMock());
            mockProductCapRepo.Setup(u => u.Query()).Returns(new List<ProductCap>().BuildMock());
            mockProductCapAmountRepo.Setup(u => u.Query()).Returns(new List<ProductCapAmount>().BuildMock());
            mockSettingRepo.Setup(u => u.Query()).Returns(new List<Setting>().BuildMock());
            mockPcardRepo.Setup(u => u.Query()).Returns(new List<Pcard>().BuildMock());
            mockPcardRepo.Setup(u => u.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard>()); 
            mockEcardRepo.Setup(u => u.Query()).Returns(new List<Ecard>().BuildMock());
            mockEruleRepo.Setup(u => u.Query()).Returns(new List<Erule>().BuildMock());
            mockEruleMasterRepo.Setup(u => u.Query()).Returns(new List<EruleMaster>().BuildMock());
            mockFactorRepo.Setup(u => u.Query()).Returns(new List<Factor>().BuildMock());
            mockParameterRepo.Setup(u => u.Query()).Returns(new List<Parameter>().BuildMock());
            mockParameterBindingRepo.Setup(u => u.Query()).Returns(new List<ParameterBinding>().BuildMock());
            mockConditionRepo.Setup(u => u.GetAll()).Returns(new List<Condition>());
            mockCategoryRepo.Setup(u => u.Query()).Returns(new List<Category>().BuildMock());
            mockNodeModelRepo.Setup(u => u.Query()).Returns(new List<Node>().BuildMock());
            mockNodeModelRepo.Setup(u => u.GetAll()).Returns(new List<Node>());
            mockNodeApiRepo.Setup(u => u.Query()).Returns(new List<NodeApi>().BuildMock());
            mockNodeApiRepo.Setup(u => u.GetAll()).Returns(new List<NodeApi>());
            mockApiParametersRepo.Setup(u => u.Query()).Returns(new List<ApiParameter>().BuildMock());
            mockApiParametersRepo.Setup(u => u.GetAll()).Returns(new List<ApiParameter>());
            mockApiParameterMapsRepo.Setup(u => u.Query()).Returns(new List<ApiParameterMap>().BuildMock());
            mockEvaluationHistoryRepo.Setup(u => u.Add(It.IsAny<EvaluationHistory>(), It.IsAny<bool>()));
            mockEvaluationHistoryRepo.Setup(u => u.Update(It.IsAny<EvaluationHistory>()));
            _mockIntegrationApiEvalRepo.Setup(u => u.Add(It.IsAny<IntegrationApiEvaluation>(), It.IsAny<bool>()));
            mockRejectionReasonRepo.Setup(u => u.Query()).Returns(new List<RejectionReasons>().BuildMock());
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        }

        [Fact]
        public void GetAllEligibleProducts_WhenNoProducts_ShouldReturnEmptyList()
        {
            // Arrange
            int tenantId = 1;
            var keyValues = new Dictionary<int, object>();
            SetupDefaultMocks(tenantId);

            // Act
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Products);
        }

        [Fact]
        public void GetAllEligibleProducts_WithNonPCardProduct_ShouldReturnProductAsNonEligible()
        {
            // Arrange
            int tenantId = 1;
            var keyValues = new Dictionary<int, object>();
            SetupDefaultMocks(tenantId);
            
            var product = new Product { ProductId = 1, TenantId = tenantId, ProductName = "No Card Plan", Code = "NP1" };
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());

            // Act
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Products);
            Assert.False(result.Products[0].IsEligible);
            Assert.Contains("Product does not have any Product CARD.", result.Products[0].Message);
        }

        [Fact]
        public async Task CallExternalApiAsync_ShouldReturnResponse_WhenApiCallIsSuccessful()
        {
            // Arrange
            var url = "https://api.example.com/test";
            var expectedResponse = "{\"status\":\"success\"}";
            
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
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _service.CallExternalApiAsync(url, "GET", null, 1);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task CallExternalApiAsync_WhenNonSuccessStatus_ShouldReturnErrorJson()
        {
            // Arrange
            var url = "https://api.example.com/fail";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _service.CallExternalApiAsync(url, "GET", null, 1);

            // Assert
            Assert.Contains("\"Success\":false", result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"StatusCode\":500", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetAllEligibleProducts_WithSuccessfulEligibility_ShouldReturnEligibleProduct()
        {
            int tenantId = 1;
            // Parameters: 1=Age, 2=Salary, 3=score
            var keyValues = new Dictionary<int, object> 
            { 
                { 1, "30" }, 
                { 2, "5000" }, 
                { 3, "80" } 
            }; 

            // 1. Setup Data
            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Gold Plan", Code = "GP1", MaxEligibleAmount = 10000 };
            var masterRule = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var rule = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule }; 
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, EcardName = "AgeECard", Expression = "1" }; 
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, PcardName = "GoldPCard", Expression = "200", Product = product };
            
            var factor = new Factor { FactorId = 1, FactorName = "Age", ParameterId = 1, TenantId = tenantId };
            var conditions = new List<Condition>
            {
                new Condition { ConditionId = 1, ConditionValue = ">" },
                new Condition { ConditionId = 2, ConditionValue = "=" }
            };

            var parameters = new List<Parameter>
            {
                new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId },
                new Parameter { ParameterId = 2, ParameterName = "Salary", TenantId = tenantId },
                new Parameter { ParameterId = 3, ParameterName = "score", TenantId = tenantId }
            };

            // ProductCapAmount for initial amount
            var capAmount = new ProductCapAmount { ProductId = 10, TenantId = tenantId, Age = "All", Salary = "All", Amount = 10000 };
            // ProductCap for scoring multiplier
            var productCap = new ProductCap { ProductId = 10, TenantId = tenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(conditions);
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(parameters.BuildMock());
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmount }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { productCap }.BuildMock());

            // Act
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Assert
            Assert.NotNull(result);
            var eligibleProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(eligibleProd);
            Assert.True(eligibleProd.IsEligible, $"Product should be eligible. Message: {eligibleProd.Message}");
            Assert.Equal(10000, eligibleProd.EligibleAmount);
        }

        [Fact]
        public void GetAllEligibleProducts_WithFailedRule_ShouldReturnNonEligibleWithErrorMessage()
        {
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "20" } }; // Age 20 (fails 1>25)

            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Gold Plan", Code = "GP1" };
            var masterRule = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var rule = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule };
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product };
            var factor = new Factor { FactorId = 1, FactorName = "Age", ParameterId = 1, TenantId = tenantId };
            
            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            // Act
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Assert
            var failedProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(failedProd);
            Assert.False(failedProd.IsEligible);
            // It could be "No Rule Match" or the specific rule error
            Assert.True(!string.IsNullOrEmpty(failedProd.Message), "Error message should not be empty");
        }

        [Fact]
        public void GetAllEligibleProducts_WithProductCap_ShouldLimitEligibleAmount()
        {
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> 
            { 
                { 1, "30" }, 
                { 2, "5000" }, 
                { 3, "100" } 
            };

            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Gold Plan", Code = "GP1", MaxEligibleAmount = 10000 };
            var masterRule = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var rule = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule };
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product };
            
            // Setup a Cap Amount of 2000 for Age > 25
            var capAmount = new ProductCapAmount { ProductId = 10, TenantId = tenantId, Age = "All", Salary = "All", Amount = 2000 };
            var productCap = new ProductCap { ProductId = 10, TenantId = tenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { new Factor { ParameterId = 1 } }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> 
            { 
                new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId },
                new Parameter { ParameterId = 2, ParameterName = "Salary", TenantId = tenantId },
                new Parameter { ParameterId = 3, ParameterName = "score", TenantId = tenantId }
            }.BuildMock());
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmount }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { productCap }.BuildMock());

            // Act
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Assert
            var eligibleProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(eligibleProd);
            Assert.True(eligibleProd.IsEligible);
            Assert.Equal(2000, eligibleProd.EligibleAmount); 
        }

        [Fact]
        public async Task CallExternalApiWithMappingAsync_ShouldReturnString_WhenApiCallIsSuccessful()
        {
            // Arrange
            int tenantId = 1;
            int nodeId = 1;
            int apiId = 100;
            SetupDefaultMocks(tenantId);
            
            var node = new Node { NodeId = nodeId, TenantId = tenantId, NodeUrl = "https://api.example.com" };
            var nodeApi = new NodeApi 
            { 
                Apiid = apiId, 
                NodeId = nodeId, 
                Apiname = "test-api", 
                IsActive = true, 
                HttpMethodType = "POST",
                EndpointPath = "/test",
                RequestBody = "{}",
                RequestParameters = "{}",
                ResponseRootPath = "",
                TargetTable = "",
                Node = node
            };
            
            _mockUow.Setup(u => u.NodeApiRepository.GetAll()).Returns(new List<NodeApi> { nodeApi });
            _mockUow.Setup(u => u.NodeModelRepository.GetAll()).Returns(new List<Node> { node });

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
                    Content = new StringContent("True")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var request = new DynamicApiRequest { Url = "https://api.example.com/test", HttpMethod = "POST" };
            var result = await _service.CallExternalApiWithMappingAsync(request);

            // Assert
            Assert.Contains("True", result, StringComparison.OrdinalIgnoreCase);
        }
        [Fact]
        public async Task ProcessBREIntegration_WithMissingMandatoryParams_ShouldReturnValidationResponse()
        {
            // Arrange
            int tenantId = 1;
            var keyValues = new Dictionary<string, object> { { "NationalId", "12345" } }; // Missing LoanNo
            SetupDefaultMocks(tenantId);

            var mandatoryParams = new List<Parameter>
            {
                new Parameter { ParameterId = 1, ParameterName = "NationalId", IsMandatory = true, TenantId = tenantId },
                new Parameter { ParameterId = 2, ParameterName = "LoanNo", IsMandatory = true, TenantId = tenantId }
            };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(mandatoryParams.BuildMock());

            // Act
            var result = await _service.ProcessBREIntegration(keyValues, tenantId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("mandatory", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("LoanNo", result.MandatoryParameters);
        }

        [Fact]
        public async Task ProcessBREIntegration_ComplexLogic_ShouldReturnCorrectEligibility()
        {
            // 1. Arrange
            int tenantId = 1;
            var input = new Dictionary<string, object>
            {
                { "NationalId", "NAT123" },
                { "LoanNo", "L001" },
                { "score", "85" },
                { "probabilityofdefault", "5" }
            };

            SetupDefaultMocks(tenantId);

            // Mock Parameters
            var parameters = new List<Parameter>
            {
                new Parameter { ParameterId = 1, ParameterName = "NationalId", IsMandatory = true, TenantId = tenantId },
                new Parameter { ParameterId = 2, ParameterName = "LoanNo", IsMandatory = true, TenantId = tenantId },
                new Parameter { ParameterId = 3, ParameterName = "Age", TenantId = tenantId }, // From API
                new Parameter { ParameterId = 4, ParameterName = "Salary", TenantId = tenantId }, // From API
                new Parameter { ParameterId = 5, ParameterName = "score", TenantId = tenantId },
                new Parameter { ParameterId = 6, ParameterName = "probabilityofdefault", TenantId = tenantId }
            };
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(parameters.BuildMock());

            // Rules: Rule 10 (Age > 25), Rule 11 (Salary > 30000)
            var masterRule1 = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var masterRule2 = new EruleMaster { Id = 101, EruleName = "SalaryRule", IsActive = true, TenantId = tenantId };
            var rules = new List<Erule>
            {
                new Erule { EruleId = 10, EruleMasterId = 100, Expression = "3>25", TenantId = tenantId, EruleMaster = masterRule1 }, // 3 is Age
                new Erule { EruleId = 11, EruleMasterId = 101, Expression = "4>30000", TenantId = tenantId, EruleMaster = masterRule2 } // 4 is Salary
            };
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(rules.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule1, masterRule2 }.BuildMock());

            // ECards: ECard 200 (Rule 10 AND Rule 11)
            var ecards = new List<Ecard>
            {
                new Ecard { EcardId = 200, EcardName = "ComplexECard", Expression = "10 AND 11", TenantId = tenantId }
            };
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(ecards.BuildMock());

            // PCards: PCard 300 (ECard 200)
            var goldProduct = new Product { ProductId = 77, ProductName = "Gold Plan", Code = "GOLD", TenantId = tenantId, MaxEligibleAmount = 100000 };
            var pcards = new List<Pcard>
            {
                new Pcard { PcardId = 300, PcardName = "GoldPCard", Expression = "200", ProductId = 77, Product = goldProduct, TenantId = tenantId }
            };
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(pcards.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcards[0] });
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { goldProduct }.BuildMock());

            // External API Setup (Returns Age: 30, Salary: 50000)
            var node = new Node { NodeId = 1, NodeUrl = "http://api.internal", TenantId = tenantId };
            var api = new NodeApi { Apiid = 50, NodeId = 1, Apiname = "customer-data", HttpMethodType = "GET", IsActive = true, TenantId = tenantId, EndpointPath = "/data", RequestBody = "{}", RequestParameters = "{}" };
            _mockUow.Setup(u => u.NodeModelRepository.GetAll()).Returns(new List<Node> { node });
            _mockUow.Setup(u => u.NodeApiRepository.GetAll()).Returns(new List<NodeApi> { api });

            // API Parameter Mappings (Map API "age" to internal Param 3, "salary" to internal Param 4)
            var apiParams = new List<ApiParameter>
            {
                new ApiParameter { ApiParamterId = 501, ApiId = 50, ParameterName = "age", ParameterDirection = "Output", TenantId = tenantId },
                new ApiParameter { ApiParamterId = 502, ApiId = 50, ParameterName = "salary", ParameterDirection = "Output", TenantId = tenantId }
            };
            var apiMaps = new List<ApiParameterMap>
            {
                new ApiParameterMap { ApiParameterId = 501, ParameterId = 3, ApiId = 50, TenantId = tenantId },
                new ApiParameterMap { ApiParameterId = 502, ParameterId = 4, ApiId = 50, TenantId = tenantId }
            };
            _mockUow.Setup(u => u.ApiParametersRepository.GetAll()).Returns(apiParams);
            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(apiMaps.BuildMock());
            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(apiParams.BuildMock());
            _mockUow.Setup(u => u.ApiParameterMapsRepository.GetAll()).Returns(apiMaps);

            // Other necessary mocks for Eligibility pipeline
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { 
                new Factor { FactorId = 1, FactorName = "AgeFactor", ParameterId = 3, TenantId = tenantId },
                new Factor { FactorId = 2, FactorName = "SalaryFactor", ParameterId = 4, TenantId = tenantId }
            }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { 
                new ProductCap { ProductId = 77, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100, TenantId = tenantId }
            }.BuildMock());
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { 
                new ProductCapAmount { ProductId = 77, Age = "All", Salary = "All", Amount = 100000, TenantId = tenantId }
            }.BuildMock());

            // HttpClient Mock
            var jsonResponse = "{\"age\": 30, \"salary\": 50000}";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));

            // 2. Act
            var result = await _service.ProcessBREIntegration(input, tenantId, "REQ-001");

            // 3. Assert
            Assert.NotNull(result);
            Assert.Equal(85, result.CustomerScore);
            Assert.Single(result.EligibleProducts);
            Assert.Equal("GOLD", result.EligibleProducts[0].ProductCode);
        }
        [Fact]
        public async Task ProcessBREIntegration_EndToEnd_ShouldLimitByProductCap()
        {
            // 1. Arrange
            int tenantId = 1;
            var input = new Dictionary<string, object>
            {
                { "NationalId", "NAT456" },
                { "LoanNo", "L002" },
                { "Age", "30" }, // Direct input
                { "score", "80" }
            };

            SetupDefaultMocks(tenantId);

            // Mock Parameters
            var parameters = new List<Parameter>
            {
                new Parameter { ParameterId = 1, ParameterName = "NationalId", IsMandatory = true, TenantId = tenantId },
                new Parameter { ParameterId = 2, ParameterName = "LoanNo", IsMandatory = true, TenantId = tenantId },
                new Parameter { ParameterId = 3, ParameterName = "Age", TenantId = tenantId },
                new Parameter { ParameterId = 4, ParameterName = "Salary", TenantId = tenantId }, // From API
                new Parameter { ParameterId = 5, ParameterName = "score", TenantId = tenantId }
            };
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(parameters.BuildMock());

            // Simple Rule: 3 > 25 (Age > 25)
            var masterRule = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var rule = new Erule { EruleId = 10, EruleMasterId = 100, Expression = "3>25", TenantId = tenantId, EruleMaster = masterRule };
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());

            // ECard & PCard
            var ecard = new Ecard { EcardId = 200, Expression = "10", TenantId = tenantId };
            var product = new Product { ProductId = 88, ProductName = "Capped Plan", Code = "CAP", TenantId = tenantId, MaxEligibleAmount = 50000 };
            var pcard = new Pcard { PcardId = 300, Expression = "200", ProductId = 88, Product = product, TenantId = tenantId };
            
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());

            // External API Setup (Returns Salary: 45000)
            var node = new Node { NodeId = 1, NodeUrl = "http://api.internal", TenantId = tenantId };
            var api = new NodeApi { Apiid = 51, NodeId = 1, Apiname = "financial-data", HttpMethodType = "GET", IsActive = true, TenantId = tenantId, EndpointPath = "/fin", RequestBody = "{}", RequestParameters = "{}" };
            _mockUow.Setup(u => u.NodeModelRepository.GetAll()).Returns(new List<Node> { node });
            _mockUow.Setup(u => u.NodeApiRepository.GetAll()).Returns(new List<NodeApi> { api });

            var apiParams = new List<ApiParameter>
            {
                new ApiParameter { ApiParamterId = 601, ApiId = 51, ParameterName = "salary", ParameterDirection = "Output", TenantId = tenantId }
            };
            var apiMaps = new List<ApiParameterMap>
            {
                new ApiParameterMap { ApiParameterId = 601, ParameterId = 4, ApiId = 51, TenantId = tenantId }
            };
            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(apiParams.BuildMock());
            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(apiMaps.BuildMock());

            // ProductCapAmount: Limit to 20000 if Salary > 40000
            var capAmount = new ProductCapAmount 
            { 
                ProductId = 88, 
                TenantId = tenantId, 
                Age = "All", 
                Salary = ">40000", 
                Amount = 20000 
            };
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmount }.BuildMock());

            // ProductCap (Scoring): 50% multiplier for score 70-90
            var productCap = new ProductCap 
            { 
                ProductId = 88, 
                TenantId = tenantId, 
                MinimumScore = 70, 
                MaximumScore = 90, 
                ProductCapPercentage = 50 
            };
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { productCap }.BuildMock());

            // Mock other helpers
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { new Factor { ParameterId = 3 } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            // HttpClient Mock
            var jsonResponse = "{\"salary\": 45000}";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));

            // 2. Act
            var result = await _service.ProcessBREIntegration(input, tenantId, "REQ-002");

            // 3. Assert
            Assert.NotNull(result);
            var eligibleProd = result.EligibleProducts.FirstOrDefault(p => p.ProductCode == "CAP");
            Assert.NotNull(eligibleProd);
            // Expected: 20000 (salary cap) * 0.50 (score multiplier) = 10000
            Assert.Equal(10000, eligibleProd.EligibleAmount);
        }

        [Fact]
        public async Task ProcessBREIntegration_NegativeRange_ShouldHandleCorrectly()
        {
            var tenantId = 1;
            SetupDefaultMocks(tenantId);

            var products = new List<Product>
            {
                new() { ProductId = 1, ProductName = "Test Product", TenantId = tenantId, MaxEligibleAmount = 10000, Code = "P1" }
            }.BuildMock();
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(products);

            // Mock PCards, ECards, and Rules to ensure initial eligibility passes
            var erules = new List<Erule>
            {
                new() { EruleId = 10, Expression = "1>0", TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(erules);

            var factors = new List<Factor>
            {
                new() { FactorId = 1, ParameterId = 1, Value1 = "ALL", TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(factors);

            var ecards = new List<Ecard>
            {
                new() { EcardId = 1, Expression = "10", TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(ecards);

            var pcardsList = new List<Pcard>
            {
                new() { PcardId = 1, Expression = "1", ProductId = 1, TenantId = tenantId, PcardName = "Test", Product = new Product { ProductId = 1, TenantId = tenantId } }
            };
            var pcards = pcardsList.BuildMock();
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(pcards);
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(pcardsList);

            var capAmounts = new List<ProductCapAmount>
            {
                new() { ProductId = 1, Age = "-100--50", Salary = "All", Amount = 5000, TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(capAmounts);

            var productCaps = new List<ProductCap>
            {
                new() { ProductId = 1, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100, TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(productCaps);

            var keyValues = new Dictionary<string, object>
            {
                { "Age", "-75" },
                { "LoanNo", "123" },
                { "NationalId", "NAT-001" }
            };

            var parameters = new List<Parameter>
            {
                new() { ParameterId = 1, ParameterName = "Age", TenantId = tenantId },
                new() { ParameterId = 2, ParameterName = "LoanNo", IsMandatory = true, TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "NationalId", IsMandatory = true, TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(parameters);

            var result = await _service.ProcessBREIntegration(keyValues, tenantId, "req-1");

            var eligibleProd = result.EligibleProducts.FirstOrDefault(p => p.ProductCode == "P1");
            // If the bug exists, this will be null because the range match failed
            Assert.NotNull(eligibleProd);
            Assert.Equal(5000, eligibleProd.EligibleAmount);
        }

        [Fact]
        public async Task ProcessBREIntegration_PartialApiFailure_ShouldReflectError()
        {
            var tenantId = 1;
            SetupDefaultMocks(tenantId);

            // Mock an active API that will fail
            var nodeApis = new List<NodeApi>
            {
                new() { Apiid = 1, Apiname = "api1", NodeId = 1, IsActive = true, TenantId = tenantId, HttpMethodType = "POST", ExecutionOrder = 1 }
            };
            _mockUow.Setup(u => u.NodeApiRepository.GetAll()).Returns(nodeApis);

            var nodes = new List<Node>
            {
                new() { NodeId = 1, NodeUrl = "http://fail", TenantId = tenantId }
            };
            _mockUow.Setup(u => u.NodeModelRepository.GetAll()).Returns(nodes);

            // HttpClient mock focused on failure
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent("Critical system error") });
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));

            var input = new Dictionary<string, object> { 
                { "LoanNo", "456" },
                { "NationalId", "NAT-002" }
            };
            var parameters = new List<Parameter>
            {
                new() { ParameterId = 2, ParameterName = "LoanNo", IsMandatory = true, TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "NationalId", IsMandatory = true, TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(parameters);

            // Execute
            var result = await _service.ProcessBREIntegration(input, tenantId, "req-2");

            // Assert: While the overall call might succeed, we should verify the API failure was logged or impacted the state
            _mockUow.Verify(u => u.IntegrationApiEvaluationRepository.Add(It.Is<IntegrationApiEvaluation>(e => e.ApiResponse.Contains("Critical system error")), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task ProcessBREIntegration_TenantIsolation_ShouldNotCallOtherTenantApis()
        {
            var tenantId = 1;
            SetupDefaultMocks(tenantId);

            // Mock APIs for TWO different tenants
            var nodeApis = new List<NodeApi>
            {
                new() { Apiid = 1, Apiname = "Tenant1Api", NodeId = 1, IsActive = true, TenantId = 1, HttpMethodType = "POST", ExecutionOrder = 1 },
                new() { Apiid = 2, Apiname = "Tenant2Api", NodeId = 2, IsActive = true, TenantId = 2, HttpMethodType = "POST", ExecutionOrder = 1 }
            };
            _mockUow.Setup(u => u.NodeApiRepository.GetAll()).Returns(nodeApis);

            var nodes = new List<Node>
            {
                new() { NodeId = 1, NodeUrl = "http://t1", TenantId = 1 },
                new() { NodeId = 2, NodeUrl = "http://t2", TenantId = 2 }
            };
            _mockUow.Setup(u => u.NodeModelRepository.GetAll()).Returns(nodes);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{}") });
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));

            var input = new Dictionary<string, object> { 
                { "LoanNo", "789" },
                { "NationalId", "NAT-003" }
            };
            var parameters = new List<Parameter>
            {
                new() { ParameterId = 2, ParameterName = "LoanNo", IsMandatory = true, TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "NationalId", IsMandatory = true, TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(parameters);

            // Execute for Tenant 1
            await _service.ProcessBREIntegration(input, 1, "req-3");

            // Assert: Only Tenant1Api should have been called (verified via log entry)
            _mockIntegrationApiEvalRepo.Verify(u => u.Add(It.Is<IntegrationApiEvaluation>(e => e.NodeApiId == 1), It.IsAny<bool>()), Times.Once);
            _mockIntegrationApiEvalRepo.Verify(u => u.Add(It.Is<IntegrationApiEvaluation>(e => e.NodeApiId == 2), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task ProcessBREIntegration_MalformedJson_ShouldHandleGracefully()
        {
            var tenantId = 1;
            SetupDefaultMocks(tenantId);

            // Mock an active API that returns malformed/unexpected JSON
            var nodeApis = new List<NodeApi>
            {
                new() { Apiid = 1, Apiname = "BadJsonApi", NodeId = 1, IsActive = true, TenantId = tenantId, HttpMethodType = "GET", ExecutionOrder = 1 }
            };
            _mockUow.Setup(u => u.NodeApiRepository.GetAll()).Returns(nodeApis);

            var nodes = new List<Node>
            {
                new() { NodeId = 1, NodeUrl = "http://badjson", TenantId = tenantId }
            };
            _mockUow.Setup(u => u.NodeModelRepository.GetAll()).Returns(nodes);

            // HttpClient mock returning invalid JSON
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("INVALID JSON {") });
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));

            var input = new Dictionary<string, object> { 
                { "LoanNo", "101" },
                { "NationalId", "NAT-004" }
            };
            var parameters = new List<Parameter>
            {
                new() { ParameterId = 2, ParameterName = "LoanNo", IsMandatory = true, TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "NationalId", IsMandatory = true, TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(parameters);

            // Execute - Should NOT throw but catch the deserialization error
            var result = await _service.ProcessBREIntegration(input, tenantId, "req-4");

            // Assert
            Assert.NotNull(result); // Should still return a response, even if API failed
        }

        [Fact]
        public void GetAllEligibleProducts_RuleRequiresMissingKey_ShouldReturnNoRuleResultsMessage()
        {
            // Arrange
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "30" } }; // Missing param 2

            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Plan", Code = "P1" };
            var masterRule = new EruleMaster { Id = 100, EruleName = "Rule", IsActive = true, TenantId = tenantId };
            var rule = new Erule
            {
                EruleId = 1,
                EruleMasterId = 100,
                TenantId = tenantId,
                Expression = "2>25", // requires ParamId 2
                Version = 1,
                EruleMaster = masterRule,
                ValidFrom = DateTime.Now.AddDays(-1)
            };
            var factor = new Factor { FactorId = 1, FactorName = "Age", ParameterId = 2, TenantId = tenantId };
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 2, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            // Act
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Assert
            var failedProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(failedProd);
            Assert.False(failedProd.IsEligible);
            Assert.Contains("No rule results provided", failedProd.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetAllEligibleProducts_ShouldUseHighestRuleVersion()
        {
            // Arrange
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "20" } };

            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Gold Plan", Code = "GP1", MaxEligibleAmount = 10000 };
            var masterRule = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var ruleV1 = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>10", Version = 1, EruleMaster = masterRule, ValidFrom = DateTime.Now.AddDays(-1) };
            var ruleV2 = new Erule { EruleId = 2, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 2, EruleMaster = masterRule, ValidFrom = DateTime.Now.AddDays(-1) };

            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "2" }; // points to ruleV2
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product };
            var factor = new Factor { FactorId = 1, FactorName = "Age", ParameterId = 1, TenantId = tenantId };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { ruleV1, ruleV2 }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            // Act
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Assert
            var failedProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(failedProd);
            Assert.False(failedProd.IsEligible); // Version 2 should be used and fail
        }

        [Fact]
        public void GetAllEligibleProducts_WithRuleOutsideDateRange_ShouldReturnNoRuleResultsMessage()
        {
            // Arrange
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "30" } };

            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Plan", Code = "P1" };
            var masterRule = new EruleMaster { Id = 100, EruleName = "Rule", IsActive = true, TenantId = tenantId };
            var rule = new Erule
            {
                EruleId = 1,
                EruleMasterId = 100,
                TenantId = tenantId,
                Expression = "1>25",
                Version = 1,
                EruleMaster = masterRule,
                ValidFrom = DateTime.Now.AddDays(5)
            };
            var factor = new Factor { FactorId = 1, FactorName = "Age", ParameterId = 1, TenantId = tenantId };
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            // Act
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Assert
            var failedProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(failedProd);
            Assert.Contains("No rule results provided", failedProd.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetEcardByEruleId_WithInvalidParentheses_ShouldSkipEcard()
        {
            var tenantId = 1;
            var ecards = new List<Ecard>
            {
                new() { EcardId = 200, TenantId = tenantId, Expression = "(1 AND (2 OR 3" } // invalid
            }.BuildMock();

            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(ecards);

            var result = _service.GetEcardByEruleId(tenantId, [1, 2, 3]);

            Assert.Empty(result);
        }

        [Fact]
        public void GetEcardByEruleId_WithValidParentheses_ShouldEvaluateCorrectly()
        {
            var tenantId = 1;
            var ecards = new List<Ecard>
            {
                new() { EcardId = 201, TenantId = tenantId, Expression = "1 Or (2 And 3)" },
                new() { EcardId = 202, TenantId = tenantId, Expression = "(1 Or 2) And 3" }
            }.BuildMock();

            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(ecards);

            var result = _service.GetEcardByEruleId(tenantId, [2, 3]);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, e => e.EcardId == 201);
            Assert.Contains(result, e => e.EcardId == 202);
        }

        [Fact]
        public void ValidatePcards_WithInvalidExpression_ShouldReturnFalse()
        {
            var ecardResults = new List<EcardResult> { new() { EcardID = 200, Result = true } };
            var pcards = new List<Pcard> { new() { PcardId = 300, Expression = "200 And" } };

            var results = _service.ValidatePcards(pcards, ecardResults);

            Assert.Single(results);
            Assert.False(results[0].Result);
        }

        [Fact]
        public void ValidatePcards_WithNotAndLowercase_ShouldEvaluateCorrectly()
        {
            var ecardResults = new List<EcardResult>
            {
                new() { EcardID = 200, Result = true },
                new() { EcardID = 201, Result = false }
            };
            var pcards = new List<Pcard>
            {
                new() { PcardId = 302, Expression = "not 200 Or 201" } // !true OR false => false
            };

            var results = _service.ValidatePcards(pcards, ecardResults);

            Assert.Single(results);
            Assert.False(results[0].Result);
        }

        [Fact]
        public void CheckEligibleAmount_WhenNoScoreCriteria_ShouldReturnScoreError()
        {
            var tenantId = 1;
            var product = new Product { ProductId = 1, ProductName = "P1", Code = "P1" };
            var validateProducts = new List<ProductEligibilityResult>
            {
                new() { ProductId = 1, ProductName = "P1", IsProcessedByException = false }
            };

            var caps = new List<ProductCap>(); // no score criteria
            var capAmounts = new List<ProductCapAmount>
            {
                new() { ProductId = 1, TenantId = tenantId, Age = "All", Salary = "All", Amount = 1000 }
            }.BuildMock();

            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(capAmounts);
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>().BuildMock());

            var keyValues = new Dictionary<int, object> { { 1, "30" }, { 2, "5000" }, { 3, "50" } };

            var result = _service.CheckEligibleAmount(tenantId, validateProducts, [product], caps, keyValues);

            Assert.Single(result.Products);
            Assert.False(result.Products[0].IsEligible);
            Assert.Contains("eligible Score criteria", result.Products[0].ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CheckEligibleAmount_WhenNoCapAmountMatch_ShouldReturnAmountError()
        {
            var tenantId = 1;
            var product = new Product { ProductId = 1, ProductName = "P1", Code = "P1" };
            var validateProducts = new List<ProductEligibilityResult>
            {
                new() { ProductId = 1, ProductName = "P1", IsProcessedByException = false }
            };

            var caps = new List<ProductCap>
            {
                new() { ProductId = 1, TenantId = tenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 }
            };
            var capAmounts = new List<ProductCapAmount>
            {
                new() { ProductId = 1, TenantId = tenantId, Age = ">40", Salary = "All", Amount = 1000 }
            }.BuildMock();

            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(capAmounts);
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>().BuildMock());

            var keyValues = new Dictionary<int, object> { { 1, "30" }, { 2, "5000" }, { 3, "50" } };

            var result = _service.CheckEligibleAmount(tenantId, validateProducts, [product], caps, keyValues);

            Assert.Single(result.Products);
            Assert.False(result.Products[0].IsEligible);
            Assert.Contains("eligible amount criteria", result.Products[0].ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CallExternalApiAsync_WithInvalidHeadersJson_ShouldReturnErrorJson()
        {
            var url = "https://api.example.com/test";
            var result = await _service.CallExternalApiAsync(url, "GET", null, 1, null, "{invalid-json}");

            Assert.Contains("\"Success\":false", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ValidateEcards_WithBracketsAndOrAnd_ShouldEvaluateCorrectly()
        {
            // Arrange
            var ruleResults = new List<RuleResult>
            {
                new() { RuleID = 10, IsValid = true },
                new() { RuleID = 11, IsValid = true },
                new() { RuleID = 12, IsValid = false }
            };

            var ecards = new List<Ecard>
            {
                new() { EcardId = 200, Expression = "10 Or (11 And 12)" },
                new() { EcardId = 201, Expression = "(10 Or 11) And 12" }
            };

            // Act
            var results = _service.ValidateEcards(ecards, ruleResults);

            // Assert
            var e1 = results.FirstOrDefault(r => r.EcardID == 200);
            var e2 = results.FirstOrDefault(r => r.EcardID == 201);

            Assert.NotNull(e1);
            Assert.NotNull(e2);
            Assert.True(e1.Result);  // true OR (true AND false) => true
            Assert.False(e2.Result); // (true OR true) And false => false
        }

        [Fact]
        public void ValidatePcards_WithOrAndBrackets_ShouldEvaluateCorrectly()
        {
            // Arrange
            var ecardResults = new List<EcardResult>
            {
                new() { EcardID = 200, Result = true },
                new() { EcardID = 201, Result = false },
                new() { EcardID = 202, Result = true }
            };

            var pcards = new List<Pcard>
            {
                new() { PcardId = 300, Expression = "200 Or 201" },
                new() { PcardId = 301, Expression = "200 And (201 Or 202)" }
            };

            // Act
            var results = _service.ValidatePcards(pcards, ecardResults);

            // Assert
            var p1 = results.FirstOrDefault(r => r.PcardID == 300);
            var p2 = results.FirstOrDefault(r => r.PcardID == 301);

            Assert.NotNull(p1);
            Assert.NotNull(p2);
            Assert.True(p1.Result);  // true OR false => true
            Assert.True(p2.Result);  // true AND (false OR true) => true
        }

        [Fact]
        public void ValidateEcards_WithoutBrackets_ShouldRespectAndOrPrecedence()
        {
            // Arrange
            // Expression: 10 Or 11 And 12 => 10 Or (11 And 12)
            var ruleResults = new List<RuleResult>
            {
                new() { RuleID = 10, IsValid = false },
                new() { RuleID = 11, IsValid = true },
                new() { RuleID = 12, IsValid = true }
            };

            var ecards = new List<Ecard>
            {
                new() { EcardId = 210, Expression = "10 Or 11 And 12" }
            };

            // Act
            var results = _service.ValidateEcards(ecards, ruleResults);

            // Assert
            var e1 = results.FirstOrDefault(r => r.EcardID == 210);
            Assert.NotNull(e1);
            Assert.True(e1.Result); // false OR (true AND true) => true
        }

        [Fact]
        public void ValidatePcards_WithoutBrackets_ShouldRespectAndOrPrecedence()
        {
            // Arrange
            // Expression: 200 Or 201 And 202 => 200 Or (201 And 202)
            var ecardResults = new List<EcardResult>
            {
                new() { EcardID = 200, Result = false },
                new() { EcardID = 201, Result = true },
                new() { EcardID = 202, Result = true }
            };

            var pcards = new List<Pcard>
            {
                new() { PcardId = 310, Expression = "200 Or 201 And 202" }
            };

            // Act
            var results = _service.ValidatePcards(pcards, ecardResults);

            // Assert
            var p1 = results.FirstOrDefault(r => r.PcardID == 310);
            Assert.NotNull(p1);
            Assert.True(p1.Result); // false OR (true AND true) => true
        }

        [Fact]
        public void GetAllEligibleProducts_WithInactiveRuleMaster_ShouldReturnNoRuleResultsMessage()
        {
            // Arrange
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "30" } };

            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Gold Plan", Code = "GP1" };
            var inactiveMaster = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = false, TenantId = tenantId };
            var rule = new Erule
            {
                EruleId = 1,
                EruleMasterId = 100,
                TenantId = tenantId,
                Expression = "1>25",
                Version = 1,
                EruleMaster = inactiveMaster,
                ValidFrom = DateTime.Now.AddDays(-1)
            };
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { new Factor { ParameterId = 1 } }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            // Act
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Assert
            var failedProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(failedProd);
            Assert.False(failedProd.IsEligible);
            Assert.Contains("No rule results provided", failedProd.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ProcessBREIntegration_WithInactiveApis_ShouldNotCallOrAddIntegrationLog()
        {
            // Arrange
            var tenantId = 1;
            SetupDefaultMocks(tenantId);

            var nodeApis = new List<NodeApi>
            {
                new() { Apiid = 1, Apiname = "InactiveApi", NodeId = 1, IsActive = false, TenantId = tenantId, HttpMethodType = "GET", ExecutionOrder = 1 }
            };
            _mockUow.Setup(u => u.NodeApiRepository.GetAll()).Returns(nodeApis);

            var nodes = new List<Node>
            {
                new() { NodeId = 1, NodeUrl = "http://inactive", TenantId = tenantId }
            };
            _mockUow.Setup(u => u.NodeModelRepository.GetAll()).Returns(nodes);

            var input = new Dictionary<string, object>
            {
                { "LoanNo", "999" },
                { "NationalId", "NAT-999" }
            };
            var parameters = new List<Parameter>
            {
                new() { ParameterId = 2, ParameterName = "LoanNo", IsMandatory = true, TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "NationalId", IsMandatory = true, TenantId = tenantId }
            }.BuildMock();
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(parameters);

            // Act
            var result = await _service.ProcessBREIntegration(input, tenantId, "req-inactive");

            // Assert
            Assert.NotNull(result);
            _mockIntegrationApiEvalRepo.Verify(u => u.Add(It.IsAny<IntegrationApiEvaluation>(), It.IsAny<bool>()), Times.Never);
        }

        // =====================================================================
        // MatchCondition — all 7 operator branches + edge cases
        // =====================================================================

        [Theory]
        [InlineData(">25", "30", true)]    // greater-than: 30 > 25 → true
        [InlineData(">25", "20", false)]   // greater-than: 20 > 25 → false
        [InlineData(">=25", "25", true)]   // greater-or-equal: 25 >= 25 → true
        [InlineData(">=25", "24", false)]  // greater-or-equal: 24 >= 25 → false
        [InlineData("<25", "20", true)]    // less-than: 20 < 25 → true
        [InlineData("<25", "25", false)]   // less-than: boundary 25 < 25 → false
        [InlineData("<=25", "25", true)]   // less-or-equal: 25 <= 25 → true
        [InlineData("<=25", "26", false)]  // less-or-equal: 26 <= 25 → false
        [InlineData("=100", "100", true)]  // equality: match
        [InlineData("=100", "99", false)]  // equality: no match
        public void MatchCondition_NumericOperators_ShouldReturnCorrectResult(string expression, string input, bool expected)
        {
            var result = EligibleProductsService.MatchCondition(expression, input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("1000-2000", "1500", true)]   // inside range
        [InlineData("1000-2000", "1000", true)]   // on lower boundary
        [InlineData("1000-2000", "2000", true)]   // on upper boundary
        [InlineData("1000-2000", "2001", false)]  // above range
        [InlineData("1000-2000", "999", false)]   // below range
        [InlineData("-100--50", "-75", true)]     // negative range, inside
        [InlineData("-100--50", "-50", true)]     // negative range, on upper boundary
        [InlineData("-100--50", "-49", false)]    // negative range, above
        public void MatchCondition_RangeExpressions_ShouldReturnCorrectResult(string expression, string input, bool expected)
        {
            var result = EligibleProductsService.MatchCondition(expression, input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MatchCondition_StringFallback_ShouldMatchCaseInsensitive()
        {
            // No operator prefix → falls back to string equality (OrdinalIgnoreCase)
            Assert.True(EligibleProductsService.MatchCondition("MALE", "male"));
            Assert.True(EligibleProductsService.MatchCondition("All", "ALL"));
            Assert.False(EligibleProductsService.MatchCondition("MALE", "female"));
        }

        [Fact]
        public void MatchCondition_NullOrBlankInput_ShouldReturnFalse()
        {
            Assert.False(EligibleProductsService.MatchCondition(">25", ""));
            Assert.False(EligibleProductsService.MatchCondition(">25", "   "));
            Assert.False(EligibleProductsService.MatchCondition("", "30"));
        }

        [Fact]
        public void MatchCondition_NonNumericInputWithNumericOperator_ShouldReturnFalse()
        {
            // "ABC" is not parseable as decimal, so numeric conditions should fail gracefully
            Assert.False(EligibleProductsService.MatchCondition(">25", "ABC"));
            Assert.False(EligibleProductsService.MatchCondition("1000-2000", "ABC"));
        }

        // =====================================================================
        // GetErrorMessagesForProduct — all 5 early-return paths
        // =====================================================================

        [Fact]
        public void GetAllEligibleProducts_WithNoPCards_ShouldReturnNoPCardsMessage()
        {
            // Path: GetErrorMessagesForProduct → "No PCARDs found for Product ID"
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "30" } };
            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Plan", Code = "P1" };
            var masterRule = new EruleMaster { Id = 100, EruleName = "Rule", IsActive = true, TenantId = tenantId };
            var rule = new Erule
            {
                EruleId = 1, EruleMasterId = 100, TenantId = tenantId,
                Expression = "1>25", Version = 1, EruleMaster = masterRule,
                ValidFrom = DateTime.Now.AddDays(-1)
            };
            var factor = new Factor { FactorId = 1, ParameterId = 1, TenantId = tenantId };
            // No PCard for product 10 (PCard has ProductId = 99, not 10)
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 99, Expression = "1", Product = new Product { ProductId = 99, TenantId = tenantId } };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard>().BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            var failedProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(failedProd);
            Assert.False(failedProd.IsEligible);
            // The product has no PCard → will get the NonPCard message
            Assert.Contains("Product does not have any Product CARD", failedProd.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetAllEligibleProducts_WithPCardHavingEmptyExpression_ShouldReturnNoECardIdsMessage()
        {
            // Path: GetErrorMessagesForProduct → "No valid ECard IDs found in PCARD expressions"
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "20" } }; // fails rule
            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Plan", Code = "P1" };
            var masterRule = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var rule = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor = new Factor { FactorId = 1, ParameterId = 1, TenantId = tenantId };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "", Product = product }; // empty expression
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1" };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            var failedProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(failedProd);
            Assert.False(failedProd.IsEligible);
            // PCard has empty expression → "No valid ECard IDs found"
            Assert.Contains("No valid ECard IDs", failedProd.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetAllEligibleProducts_WithPCardPointingToNonExistentEcard_ShouldReturnNoECardsFoundMessage()
        {
            // Path: GetErrorMessagesForProduct → "No ECARDs found for Product ID"
            // PCard references ECard 999 but no such ECard exists
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "20" } }; // fails rule
            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Plan", Code = "P1" };
            var masterRule = new EruleMaster { Id = 100, EruleName = "Rule", IsActive = true, TenantId = tenantId };
            var rule = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor = new Factor { FactorId = 1, ParameterId = 1, TenantId = tenantId };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "999", Product = product }; // ECard 999 does not exist

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard>().BuildMock()); // No ECards
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            var failedProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(failedProd);
            Assert.False(failedProd.IsEligible);
            Assert.Contains("No ECARDs found", failedProd.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        // =====================================================================
        // CheckEligibleAmount — branch: validProduct=true, but count=0 (no score match)
        // =====================================================================

        [Fact]
        public void CheckEligibleAmount_WhenCapAmountMatchesButScoreOutOfRange_ShouldReturnScoreError()
        {
            // Cap amount OK (Age=All, Salary=All) but score=50 doesn't fall in ProductCap range 70-90
            var tenantId = 1;
            var product = new Product { ProductId = 1, ProductName = "P1", Code = "P1" };
            var validateProducts = new List<ProductEligibilityResult>
            {
                new() { ProductId = 1, ProductName = "P1", IsProcessedByException = false }
            };

            var caps = new List<ProductCap>
            {
                new() { ProductId = 1, TenantId = tenantId, MinimumScore = 70, MaximumScore = 90, ProductCapPercentage = 80 }
            };
            var capAmounts = new List<ProductCapAmount>
            {
                new() { ProductId = 1, TenantId = tenantId, Age = "All", Salary = "All", Amount = 5000 }
            }.BuildMock();

            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(capAmounts);
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>().BuildMock());

            // score=50 → outside 70-90 range
            var keyValues = new Dictionary<int, object> { { 1, "30" }, { 2, "5000" }, { 3, "50" } };

            var result = _service.CheckEligibleAmount(tenantId, validateProducts, [product], caps, keyValues);

            Assert.Single(result.Products);
            Assert.False(result.Products[0].IsEligible);
            Assert.Contains("eligible Score criteria", result.Products[0].ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CheckEligibleAmount_WhenAgeConditionFailsWithOperator_ShouldReturnAmountError()
        {
            // Age expression ">40" but input Age is "30" → should fail cap amount check
            var tenantId = 1;
            var product = new Product { ProductId = 1, ProductName = "P1", Code = "P1" };
            var validateProducts = new List<ProductEligibilityResult>
            {
                new() { ProductId = 1, ProductName = "P1", IsProcessedByException = false }
            };

            var caps = new List<ProductCap>
            {
                new() { ProductId = 1, TenantId = tenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 }
            };
            var capAmounts = new List<ProductCapAmount>
            {
                new() { ProductId = 1, TenantId = tenantId, Age = ">40", Salary = "All", Amount = 8000 }
            }.BuildMock();

            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(capAmounts);
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>
            {
                new() { ParameterId = 1, ParameterName = "Age", TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "score", TenantId = tenantId }
            }.BuildMock());

            // Age=30 < 40 → condition fails → no cap amount match
            var keyValues = new Dictionary<int, object> { { 1, "30" }, { 3, "80" } };

            var result = _service.CheckEligibleAmount(tenantId, validateProducts, [product], caps, keyValues);

            Assert.Single(result.Products);
            Assert.False(result.Products[0].IsEligible);
            Assert.Contains("eligible amount criteria", result.Products[0].ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CheckEligibleAmount_WhenSalaryRangeMatches_ShouldReturnCorrectAmount()
        {
            // Salary expression "30000-50000", input=40000 → match → eligible amount = (80/100)*6000 = 4800
            var tenantId = 1;
            var product = new Product { ProductId = 1, ProductName = "P1", Code = "P1" };
            var validateProducts = new List<ProductEligibilityResult>
            {
                new() { ProductId = 1, ProductName = "P1", IsProcessedByException = false }
            };

            var caps = new List<ProductCap>
            {
                new() { ProductId = 1, TenantId = tenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 80 }
            };
            var capAmounts = new List<ProductCapAmount>
            {
                new() { ProductId = 1, TenantId = tenantId, Age = "All", Salary = "30000-50000", Amount = 6000 }
            }.BuildMock();

            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(capAmounts);
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>
            {
                new() { ParameterId = 1, ParameterName = "Age", TenantId = tenantId },
                new() { ParameterId = 2, ParameterName = "Salary", TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "score", TenantId = tenantId }
            }.BuildMock());

            // Salary=40000 → inside 30000-50000 range; score=50 → inside 0-100
            var keyValues = new Dictionary<int, object> { { 1, "25" }, { 2, "40000" }, { 3, "50" } };

            var result = _service.CheckEligibleAmount(tenantId, validateProducts, [product], caps, keyValues);

            Assert.Single(result.Products);
            Assert.True(result.Products[0].IsEligible);
            Assert.Equal(4800m, result.Products[0].EligibleAmount); // 80% of 6000
        }

        // =====================================================================
        // GetNonPCardProducts — multiple tenant isolation
        // =====================================================================

        [Fact]
        public void GetAllEligibleProducts_WithProductsFromDifferentTenants_ShouldOnlyReturnCurrentTenantProducts()
        {
            // Tenant 1 has product 10; Tenant 2 has product 20. When querying tenant 1, only product 10 appears.
            int tenantId = 1;
            var keyValues = new Dictionary<int, object>();

            var product1 = new Product { ProductId = 10, TenantId = 1, ProductName = "Tenant1Plan", Code = "T1P" };
            var product2 = new Product { ProductId = 20, TenantId = 2, ProductName = "Tenant2Plan", Code = "T2P" };

            SetupDefaultMocks(tenantId);
            // ProductRepository filters by tenantId in the service, so mock returns only tenant1 product
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product1 }.BuildMock());

            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            Assert.Single(result.Products);
            Assert.Equal(10, result.Products[0].ProductId);
            Assert.DoesNotContain(result.Products, p => p.ProductId == 20);
        }

        // =====================================================================
        // Duplicate product IDs in GetAllEligibleProducts — productDict dedup
        // =====================================================================

        [Fact]
        public void GetAllEligibleProducts_WhenSameProductAppearsMultipleTimes_ShouldDeduplicateInFinalResult()
        {
            // Arrange: same product (ID=10) appears in both nonPCardEligibilityResults and pcardFailProducts
            // This tests the productDict.ContainsKey guard that prevents duplicates
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "20" } };

            // Product 10 has a PCard configured, but rule will fail → goes to pcardFailProducts
            // ALSO not linked through a valid PCard → could theoretically be in nonPCard too (won't happen in practice, but tests dedup logic)
            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Plan", Code = "P1" };
            var masterRule = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var rule = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor = new Factor { FactorId = 1, ParameterId = 1, TenantId = tenantId };
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            // Product 10 must appear EXACTLY once, not duplicated
            Assert.Single(result.Products.Where(p => p.ProductId == 10));
        }

        // =====================================================================
        // Score exactly on ProductCap boundary (MinimumScore and MaximumScore inclusive)
        // =====================================================================

        [Theory]
        [InlineData(70, true)]   // on MinimumScore boundary → eligible
        [InlineData(90, true)]   // on MaximumScore boundary → eligible
        [InlineData(69, false)]  // just below min → not eligible (score error)
        [InlineData(91, false)]  // just above max → not eligible (score error)
        public void CheckEligibleAmount_ScoreBoundaryConditions_ShouldRespectInclusiveBounds(int score, bool expectedEligible)
        {
            var tenantId = 1;
            var product = new Product { ProductId = 1, ProductName = "P1", Code = "P1" };
            var validateProducts = new List<ProductEligibilityResult>
            {
                new() { ProductId = 1, ProductName = "P1", IsProcessedByException = false }
            };

            var caps = new List<ProductCap>
            {
                new() { ProductId = 1, TenantId = tenantId, MinimumScore = 70, MaximumScore = 90, ProductCapPercentage = 100 }
            };
            var capAmounts = new List<ProductCapAmount>
            {
                new() { ProductId = 1, TenantId = tenantId, Age = "All", Salary = "All", Amount = 10000 }
            }.BuildMock();

            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(capAmounts);
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>
            {
                new() { ParameterId = 1, ParameterName = "Age", TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "score", TenantId = tenantId }
            }.BuildMock());

            var keyValues = new Dictionary<int, object> { { 1, "30" }, { 3, score.ToString() } };
            var result = _service.CheckEligibleAmount(tenantId, validateProducts, [product], caps, keyValues);

            Assert.Equal(expectedEligible, result.Products[0].IsEligible);
        }

        // =====================================================================
        // ParseIdsFromExpression — used internally in GetErrorMessagesForProduct
        // =====================================================================

        [Fact]
        public void GetAllEligibleProducts_WithComplexNestedPCardExpression_ShouldParseAllECardIds()
        {
            // PCard expression "( 200 And ( 201 Or 202 ) )" should correctly extract ECard IDs 200, 201, 202
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "20" } }; // fails rule → goes to GetErrorMessagesForProduct
            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Plan", Code = "P1" };
            var masterRule = new EruleMaster { Id = 100, EruleName = "Rule", IsActive = true, TenantId = tenantId };
            var rule = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor = new Factor { FactorId = 1, ParameterId = 1, TenantId = tenantId };

            // Three ECARDs, all referenced from a complex nested PCard expression
            var ecard200 = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1" };
            var ecard201 = new Ecard { EcardId = 201, TenantId = tenantId, Expression = "1" };
            var ecard202 = new Ecard { EcardId = 202, TenantId = tenantId, Expression = "1" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "( 200 And ( 201 Or 202 ) )", Product = product };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard200, ecard201, ecard202 }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter> { new Parameter { ParameterId = 1, ParameterName = "Age", TenantId = tenantId } }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            // The test verifies the product is found and an error message is generated (meaning all 3 ECard IDs were parsed)
            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            var failedProd = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(failedProd);
            Assert.False(failedProd.IsEligible);
            // Message should NOT be "No ECARDs found" — it found all 3 ECARDs, meaning ids were parsed correctly
            Assert.DoesNotContain("No ECARDs found", failedProd.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        // =====================================================================
        // ECard "Or" expression — one rule fails, one passes → ECard should still pass
        // =====================================================================

        [Fact]
        public void GetAllEligibleProducts_WithOrEcardExpression_OneRulePassesShouldBeEligible()
        {
            // ECard expression: "1 Or 2" → Rule 1 fails, Rule 2 passes → ECard should evaluate to TRUE
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "20" }, { 2, "30" } };
            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Plan", Code = "P1", MaxEligibleAmount = 10000 };
            var masterRule1 = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var masterRule2 = new EruleMaster { Id = 101, EruleName = "AgeRule2", IsActive = true, TenantId = tenantId };
            // Rule 1: 1>25 → Age=20 fails. Rule 2: 2>25 → Value=30 passes.
            var rule1 = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule1, ValidFrom = DateTime.Now.AddDays(-1) };
            var rule2 = new Erule { EruleId = 2, EruleMasterId = 101, TenantId = tenantId, Expression = "2>25", Version = 1, EruleMaster = masterRule2, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor1 = new Factor { FactorId = 1, ParameterId = 1, TenantId = tenantId };
            var factor2 = new Factor { FactorId = 2, ParameterId = 2, TenantId = tenantId };
            // ECard uses "Or": either rule passing is enough
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1 Or 2" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product };
            var capAmount = new ProductCapAmount { ProductId = 10, TenantId = tenantId, Age = "All", Salary = "All", Amount = 10000 };
            var productCap = new ProductCap { ProductId = 10, TenantId = tenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule1, rule2 }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule1, masterRule2 }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor1, factor2 }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>
            {
                new() { ParameterId = 1, ParameterName = "Age", TenantId = tenantId },
                new() { ParameterId = 2, ParameterName = "Salary", TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "score", TenantId = tenantId }
            }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmount }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { productCap }.BuildMock());

            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            var prod = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(prod);
            // Rule1 fails but Rule2 passes → ECard "1 Or 2" → True → PCard "200" → True → Eligible
            Assert.True(prod.IsEligible, $"Expected eligible but got: {prod.Message}");
        }

        // =====================================================================
        // ECard "And" expression — both rules must pass → if one fails, not eligible
        // =====================================================================

        [Fact]
        public void GetAllEligibleProducts_WithAndEcardExpression_OneRuleFailsShouldBeNotEligible()
        {
            // ECard expression: "1 And 2" → Rule 1 fails (age=20<25), Rule 2 passes → ECard = FALSE
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "20" }, { 2, "30" } };
            var product = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Plan", Code = "P1", MaxEligibleAmount = 10000 };
            var masterRule1 = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var masterRule2 = new EruleMaster { Id = 101, EruleName = "AgeRule2", IsActive = true, TenantId = tenantId };
            var rule1 = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule1, ValidFrom = DateTime.Now.AddDays(-1) };
            var rule2 = new Erule { EruleId = 2, EruleMasterId = 101, TenantId = tenantId, Expression = "2>25", Version = 1, EruleMaster = masterRule2, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor1 = new Factor { FactorId = 1, ParameterId = 1, TenantId = tenantId };
            var factor2 = new Factor { FactorId = 2, ParameterId = 2, TenantId = tenantId };
            // ECard uses "And": BOTH rules must pass
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1 And 2" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule1, rule2 }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule1, masterRule2 }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor1, factor2 }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>
            {
                new() { ParameterId = 1, ParameterName = "Age", TenantId = tenantId },
                new() { ParameterId = 2, ParameterName = "Salary", TenantId = tenantId }
            }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });

            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            var prod = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(prod);
            // Rule1 fails → ECard "1 And 2" → False → NOT eligible
            Assert.False(prod.IsEligible, "ECard with AND expression should fail if ANY rule fails");
        }

        // =====================================================================
        // Multiple products — one eligible, one not — both returned correctly
        // =====================================================================

        [Fact]
        public void GetAllEligibleProducts_WithTwoProducts_OneEligibleOneNot_ShouldReturnBothCorrectly()
        {
            int tenantId = 1;
            var keyValues = new Dictionary<int, object> { { 1, "30" }, { 2, "5000" }, { 3, "80" } };

            // Product 10 → linked through PCard → eligible
            // Product 20 → no PCard → not eligible with "Product does not have any Product CARD."
            var product10 = new Product { ProductId = 10, TenantId = tenantId, ProductName = "Gold", Code = "G1", MaxEligibleAmount = 10000 };
            var product20 = new Product { ProductId = 20, TenantId = tenantId, ProductName = "Silver", Code = "S1" };
            var masterRule = new EruleMaster { Id = 100, EruleName = "AgeRule", IsActive = true, TenantId = tenantId };
            var rule = new Erule { EruleId = 1, EruleMasterId = 100, TenantId = tenantId, Expression = "1>25", Version = 1, EruleMaster = masterRule, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor = new Factor { FactorId = 1, ParameterId = 1, TenantId = tenantId };
            var ecard = new Ecard { EcardId = 200, TenantId = tenantId, Expression = "1" };
            var pcard = new Pcard { PcardId = 300, TenantId = tenantId, ProductId = 10, Expression = "200", Product = product10 };
            var capAmount = new ProductCapAmount { ProductId = 10, TenantId = tenantId, Age = "All", Salary = "All", Amount = 10000 };
            var productCap = new ProductCap { ProductId = 10, TenantId = tenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            SetupDefaultMocks(tenantId);
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product10, product20 }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { masterRule }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(new List<Parameter>
            {
                new() { ParameterId = 1, ParameterName = "Age", TenantId = tenantId },
                new() { ParameterId = 2, ParameterName = "Salary", TenantId = tenantId },
                new() { ParameterId = 3, ParameterName = "score", TenantId = tenantId }
            }.BuildMock());
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = ">" } });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmount }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { productCap }.BuildMock());

            var result = _service.GetAllEligibleProducts(tenantId, keyValues);

            Assert.Equal(2, result.Products.Count);

            var gold = result.Products.FirstOrDefault(p => p.ProductId == 10);
            Assert.NotNull(gold);
            Assert.True(gold.IsEligible, $"Gold should be eligible. Message: {gold.Message}");

            var silver = result.Products.FirstOrDefault(p => p.ProductId == 20);
            Assert.NotNull(silver);
            Assert.False(silver.IsEligible);
            Assert.Contains("Product does not have any Product CARD", silver.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }

    // =============================================================================
    // END-TO-END ProcessBREIntegration Tests — Full Pipeline
    // =============================================================================
    public class EndToEndBREIntegrationTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ILogger<EligibleProductsService>> _mockLogger;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly EligibleProductsService _service;

        // Standard tenant setup used across all tests
        private const int TenantId = 1;

        // Parameter IDs
        private const int PIdNationalId = 1;
        private const int PIdLoanNo     = 2;
        private const int PIdAge        = 3;
        private const int PIdSalary     = 4;
        private const int PIdScore      = 5;

        public EndToEndBREIntegrationTests()
        {
            _mockUow             = new Mock<IUnitOfWork>();
            _mockLogger          = new Mock<ILogger<EligibleProductsService>>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _service = new EligibleProductsService(_mockUow.Object, _mockLogger.Object, _mockHttpClientFactory.Object);
            BootstrapInfrastructure();
        }

        /// <summary>
        /// Sets up all UoW repositories with empty defaults. Individual tests override as needed.
        /// </summary>
        private void BootstrapInfrastructure()
        {
            var evalRepo   = new Mock<IEvaluationHistoryRepository>();
            var integRepo  = new Mock<IIntegrationApiEvaluationRepository>();
            var rejRepo    = new Mock<IRejectionReasonRepository>();

            evalRepo.Setup(r => r.Add(It.IsAny<EvaluationHistory>(), It.IsAny<bool>()));
            evalRepo.Setup(r => r.Update(It.IsAny<EvaluationHistory>()));
            integRepo.Setup(r => r.Add(It.IsAny<IntegrationApiEvaluation>(), It.IsAny<bool>()));
            _mockUow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            _mockUow.Setup(u => u.EvaluationHistoryRepository).Returns(evalRepo.Object);
            _mockUow.Setup(u => u.IntegrationApiEvaluationRepository).Returns(integRepo.Object);
            _mockUow.Setup(u => u.RejectionReasonRepository).Returns(rejRepo.Object);

            SetRepo<IProductRepository,         Product>(         u => u.ProductRepository);
            SetRepo<IProductCapRepository,      ProductCap>(      u => u.ProductCapRepository);
            SetRepo<IProductCapAmountRepository, ProductCapAmount>(u => u.ProductCapAmountRepository);
            SetRepo<IEcardRepository,            Ecard>(          u => u.EcardRepository);
            SetRepo<IPcardRepository,            Pcard>(          u => u.PcardRepository, pcardExtra: true);
            SetRepo<IEruleRepository,            Erule>(          u => u.EruleRepository);
            SetRepo<IEruleMasterRepository,      EruleMaster>(    u => u.EruleMasterRepository);
            SetRepo<IFactorRepository,           Factor>(         u => u.FactorRepository);
            SetRepo<IParameterRepository,        Parameter>(      u => u.ParameterRepository);
            SetRepo<IParameterBindingRepository, ParameterBinding>(u => u.ParameterBindingRepository);
            SetRepo<ISettingRepository,          Setting>(        u => u.SettingRepository);
            SetRepo<ICategoryRepository,         Category>(       u => u.CategoryRepository);

            var condRepo = new Mock<IConditionRepository>();
            condRepo.Setup(r => r.GetAll()).Returns(new List<Condition> { new() { ConditionValue = ">" }, new() { ConditionValue = ">=" }, new() { ConditionValue = "<" }, new() { ConditionValue = "<=" }, new() { ConditionValue = "=" } });
            _mockUow.Setup(u => u.ConditionRepository).Returns(condRepo.Object);

            var nodeRepo   = new Mock<INodeModelRepository>();
            var nodeApiRepo= new Mock<INodeApiRepository>();
            var apiParRepo = new Mock<IApiParametersRepository>();
            var apiMapRepo = new Mock<IApiParameterMapsRepository>();

            nodeRepo.Setup(r => r.GetAll()).Returns(new List<Node>());
            nodeApiRepo.Setup(r => r.GetAll()).Returns(new List<NodeApi>());
            apiParRepo.Setup(r => r.GetAll()).Returns(new List<ApiParameter>());
            apiParRepo.Setup(r => r.Query()).Returns(new List<ApiParameter>().BuildMock());
            apiMapRepo.Setup(r => r.Query()).Returns(new List<ApiParameterMap>().BuildMock());

            _mockUow.Setup(u => u.NodeModelRepository).Returns(nodeRepo.Object);
            _mockUow.Setup(u => u.NodeApiRepository).Returns(nodeApiRepo.Object);
            _mockUow.Setup(u => u.ApiParametersRepository).Returns(apiParRepo.Object);
            _mockUow.Setup(u => u.ApiParameterMapsRepository).Returns(apiMapRepo.Object);

            // HttpClient – returns empty JSON by default
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{}") });
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler.Object));
        }

        private void SetRepo<TRepo, TEntity>(
            System.Linq.Expressions.Expression<Func<IUnitOfWork, TRepo>> selector,
            bool pcardExtra = false)
            where TRepo : class, IRepository<TEntity>
            where TEntity : class
        {
            var mock = new Mock<TRepo>();
            mock.Setup(r => r.Query()).Returns(new List<TEntity>().BuildMock());
            if (pcardExtra && mock is Mock<IPcardRepository> pm)
                pm.Setup(r => r.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard>());
            _mockUow.Setup(selector).Returns(mock.Object);
        }

        // -----------------------------------------------------------------------
        // Helper: build a standard set of parameters
        // -----------------------------------------------------------------------
        private List<Parameter> StandardParams() => new()
        {
            new() { ParameterId = PIdNationalId, ParameterName = "NationalId", IsMandatory = true, TenantId = TenantId },
            new() { ParameterId = PIdLoanNo,     ParameterName = "LoanNo",     IsMandatory = true, TenantId = TenantId },
            new() { ParameterId = PIdAge,        ParameterName = "Age",                            TenantId = TenantId },
            new() { ParameterId = PIdSalary,     ParameterName = "Salary",                         TenantId = TenantId },
            new() { ParameterId = PIdScore,      ParameterName = "score",                          TenantId = TenantId },
        };

        // -----------------------------------------------------------------------
        // 1. Missing mandatory parameter → early return with message
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_MissingMandatoryParam_ShouldReturnValidationError()
        {
            _mockUow.Setup(u => u.ParameterRepository.Query())
                    .Returns(StandardParams().BuildMock());

            // NationalId is missing
            var input = new Dictionary<string, object> { { "LoanNo", "L001" } };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-m1");

            Assert.NotNull(result);
            Assert.Contains("mandatory", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("NationalId", result.MandatoryParameters ?? []);
        }

        // -----------------------------------------------------------------------
        // 2. LoanNo present but NationalId value empty → specific guard
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_EmptyNationalId_ShouldReturnRequiredFieldsMessage()
        {
            _mockUow.Setup(u => u.ParameterRepository.Query())
                    .Returns(StandardParams().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "" },   // empty
                { "LoanNo", "L001" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-m2");

            Assert.NotNull(result);
            Assert.Contains("required", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        // -----------------------------------------------------------------------
        // 3. Single Rule (Age > 25) PASSES → ECARD "1" → PCARD "E1" → Eligible
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_SingleRulePasses_ProductShouldBeEligible()
        {
            var product  = new Product  { ProductId = 10, ProductName = "Gold", Code = "GOLD", TenantId = TenantId, MaxEligibleAmount = 50000 };
            var master   = new EruleMaster { Id = 1,  EruleName = "AgeRule",  IsActive = true,  TenantId = TenantId };
            var rule     = new Erule     { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>25", Version = 1, EruleMaster = master, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor   = new Factor   { FactorId = 1, ParameterId = PIdAge, TenantId = TenantId };
            var ecard    = new Ecard    { EcardId = 1,  Expression = "1",   TenantId = TenantId };
            var pcard    = new Pcard   { PcardId = 1,  Expression = "1",   ProductId = 10, Product = product, TenantId = TenantId };
            var capAmt   = new ProductCapAmount { ProductId = 10, TenantId = TenantId, Age = "All", Salary = "All", Amount = 50000 };
            var cap      = new ProductCap { ProductId = 10, TenantId = TenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { master }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmt }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { cap }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "N001" }, { "LoanNo", "L001" },
                { "Age", "30" },          { "score", "80" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-1");

            Assert.NotNull(result);
            var eligible = result.EligibleProducts?.FirstOrDefault(p => p.ProductCode == "GOLD");
            Assert.NotNull(eligible);
            Assert.True(eligible.IsEligible);
        }

        // -----------------------------------------------------------------------
        // 4. Single Rule (Age > 25) FAILS → Product NOT eligible
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_SingleRuleFails_ProductShouldBeNotEligible()
        {
            var product = new Product  { ProductId = 10, ProductName = "Gold", Code = "GOLD", TenantId = TenantId };
            var master  = new EruleMaster { Id = 1, EruleName = "AgeRule", IsActive = true, TenantId = TenantId };
            var rule    = new Erule    { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>25", Version = 1, EruleMaster = master, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor  = new Factor  { FactorId = 1, ParameterId = PIdAge, TenantId = TenantId };
            var ecard   = new Ecard   { EcardId = 1, Expression = "1", TenantId = TenantId };
            var pcard   = new Pcard  { PcardId = 1, Expression = "1", ProductId = 10, Product = product, TenantId = TenantId };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { master }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            // Age = 20 → rule 3>25 FAILS
            var input = new Dictionary<string, object>
            {
                { "NationalId", "N001" }, { "LoanNo", "L001" },
                { "Age", "20" },          { "score", "80" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-2");

            Assert.NotNull(result);
            var nonEligible = result.NonEligibleProducts?.FirstOrDefault(p => p.ProductCode == "GOLD");
            Assert.NotNull(nonEligible);
        }

        // -----------------------------------------------------------------------
        // 5. ECard "1 AND 2": both rules must pass. Rule1 passes, Rule2 fails → NOT eligible
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_ECard_AND_OneRuleFails_ShouldBeNotEligible()
        {
            var product = new Product { ProductId = 20, ProductName = "Plat", Code = "PLAT", TenantId = TenantId };
            var m1 = new EruleMaster { Id = 1, EruleName = "R1", IsActive = true, TenantId = TenantId };
            var m2 = new EruleMaster { Id = 2, EruleName = "R2", IsActive = true, TenantId = TenantId };
            // Rule1: Age>25 (passes with Age=30), Rule2: Salary>50000 (fails with Salary=40000)
            var rule1 = new Erule { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>25",    Version = 1, EruleMaster = m1, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var rule2 = new Erule { EruleId = 2, EruleMasterId = 2, Expression = $"{PIdSalary}>50000", Version = 1, EruleMaster = m2, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var f1 = new Factor { FactorId = 1, ParameterId = PIdAge,    TenantId = TenantId };
            var f2 = new Factor { FactorId = 2, ParameterId = PIdSalary, TenantId = TenantId };
            // ECard requires BOTH rules: "1 AND 2"
            var ecard = new Ecard { EcardId = 10, Expression = "1 AND 2", TenantId = TenantId };
            var pcard = new Pcard { PcardId = 10, Expression = "10", ProductId = 20, Product = product, TenantId = TenantId };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule1, rule2 }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { m1, m2 }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { f1, f2 }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "N002" }, { "LoanNo", "L002" },
                { "Age", "30" },          { "Salary", "40000" }, { "score", "70" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-3");

            Assert.NotNull(result);
            // ECard AND fails → PCard false → not eligible
            var eligible = result.EligibleProducts?.FirstOrDefault(p => p.ProductCode == "PLAT");
            Assert.Null(eligible); // should NOT be in eligible list
        }

        // -----------------------------------------------------------------------
        // 6. ECard "1 OR 2": Rule1 fails, Rule2 passes → ELIGIBLE
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_ECard_OR_OneRulePasses_ShouldBeEligible()
        {
            var product = new Product { ProductId = 30, ProductName = "Silver", Code = "SIL", TenantId = TenantId, MaxEligibleAmount = 20000 };
            var m1 = new EruleMaster { Id = 1, EruleName = "R1", IsActive = true, TenantId = TenantId };
            var m2 = new EruleMaster { Id = 2, EruleName = "R2", IsActive = true, TenantId = TenantId };
            // Rule1: Age>40 FAILS (Age=30), Rule2: Salary>30000 PASSES (Salary=40000)
            var rule1 = new Erule { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>40",       Version = 1, EruleMaster = m1, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var rule2 = new Erule { EruleId = 2, EruleMasterId = 2, Expression = $"{PIdSalary}>30000", Version = 1, EruleMaster = m2, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var f1 = new Factor { FactorId = 1, ParameterId = PIdAge,    TenantId = TenantId };
            var f2 = new Factor { FactorId = 2, ParameterId = PIdSalary, TenantId = TenantId };
            var ecard = new Ecard { EcardId = 20, Expression = "1 OR 2", TenantId = TenantId };
            var pcard = new Pcard { PcardId = 20, Expression = "20", ProductId = 30, Product = product, TenantId = TenantId };
            var capAmt = new ProductCapAmount { ProductId = 30, TenantId = TenantId, Age = "All", Salary = "All", Amount = 20000 };
            var cap    = new ProductCap { ProductId = 30, TenantId = TenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule1, rule2 }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { m1, m2 }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { f1, f2 }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmt }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { cap }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "N003" }, { "LoanNo", "L003" },
                { "Age", "30" }, { "Salary", "40000" }, { "score", "60" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-4");

            Assert.NotNull(result);
            var eligible = result.EligibleProducts?.FirstOrDefault(p => p.ProductCode == "SIL");
            Assert.NotNull(eligible);
            Assert.True(eligible.IsEligible);
        }

        // -----------------------------------------------------------------------
        // 7. PCard "E1 AND E2": two ECARDs must both pass → only PCard whose ECards both pass is eligible
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_PCard_AND_TwoECards_BothPassEligible()
        {
            var product = new Product { ProductId = 40, ProductName = "Premium", Code = "PREM", TenantId = TenantId, MaxEligibleAmount = 100000 };
            var m1 = new EruleMaster { Id = 1, EruleName = "R1", IsActive = true, TenantId = TenantId };
            var m2 = new EruleMaster { Id = 2, EruleName = "R2", IsActive = true, TenantId = TenantId };
            var rule1 = new Erule { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>25",       Version = 1, EruleMaster = m1, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var rule2 = new Erule { EruleId = 2, EruleMasterId = 2, Expression = $"{PIdSalary}>20000", Version = 1, EruleMaster = m2, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var f1 = new Factor { FactorId = 1, ParameterId = PIdAge,    TenantId = TenantId };
            var f2 = new Factor { FactorId = 2, ParameterId = PIdSalary, TenantId = TenantId };
            var ecard1 = new Ecard { EcardId = 100, Expression = "1", TenantId = TenantId }; // Rule1
            var ecard2 = new Ecard { EcardId = 101, Expression = "2", TenantId = TenantId }; // Rule2
            // PCard requires BOTH ECARDs
            var pcard = new Pcard { PcardId = 50, Expression = "100 AND 101", ProductId = 40, Product = product, TenantId = TenantId };
            var capAmt = new ProductCapAmount { ProductId = 40, TenantId = TenantId, Age = "All", Salary = "All", Amount = 100000 };
            var cap    = new ProductCap { ProductId = 40, TenantId = TenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 80 };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule1, rule2 }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { m1, m2 }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { f1, f2 }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard1, ecard2 }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmt }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { cap }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            // Both rules pass: Age=30>25, Salary=30000>20000
            var input = new Dictionary<string, object>
            {
                { "NationalId", "N004" }, { "LoanNo", "L004" },
                { "Age", "30" }, { "Salary", "30000" }, { "score", "75" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-5");

            Assert.NotNull(result);
            var eligible = result.EligibleProducts?.FirstOrDefault(p => p.ProductCode == "PREM");
            Assert.NotNull(eligible);
            Assert.True(eligible.IsEligible);
            Assert.Equal(80000m, eligible.EligibleAmount); // 80% of 100000
        }

        // -----------------------------------------------------------------------
        // 8. PCard "E1 OR E2": ECard1 fails, ECard2 passes → Eligible
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_PCard_OR_OneECardFails_OtherPasses_ShouldBeEligible()
        {
            var product = new Product { ProductId = 50, ProductName = "Flexi", Code = "FLX", TenantId = TenantId, MaxEligibleAmount = 30000 };
            var m1 = new EruleMaster { Id = 1, EruleName = "R1", IsActive = true, TenantId = TenantId };
            var m2 = new EruleMaster { Id = 2, EruleName = "R2", IsActive = true, TenantId = TenantId };
            // Rule1: Age>40 FAILS (Age=25), Rule2: Salary>10000 PASSES (Salary=20000)
            var rule1 = new Erule { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>40",       Version = 1, EruleMaster = m1, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var rule2 = new Erule { EruleId = 2, EruleMasterId = 2, Expression = $"{PIdSalary}>10000", Version = 1, EruleMaster = m2, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var f1 = new Factor { FactorId = 1, ParameterId = PIdAge,    TenantId = TenantId };
            var f2 = new Factor { FactorId = 2, ParameterId = PIdSalary, TenantId = TenantId };
            var ecard1 = new Ecard { EcardId = 200, Expression = "1", TenantId = TenantId };
            var ecard2 = new Ecard { EcardId = 201, Expression = "2", TenantId = TenantId };
            // PCard OR: either ECard passes is enough
            var pcard = new Pcard { PcardId = 60, Expression = "200 OR 201", ProductId = 50, Product = product, TenantId = TenantId };
            var capAmt = new ProductCapAmount { ProductId = 50, TenantId = TenantId, Age = "All", Salary = "All", Amount = 30000 };
            var cap    = new ProductCap { ProductId = 50, TenantId = TenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule1, rule2 }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { m1, m2 }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { f1, f2 }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard1, ecard2 }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmt }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { cap }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "N005" }, { "LoanNo", "L005" },
                { "Age", "25" }, { "Salary", "20000" }, { "score", "50" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-6");

            Assert.NotNull(result);
            var eligible = result.EligibleProducts?.FirstOrDefault(p => p.ProductCode == "FLX");
            Assert.NotNull(eligible);
            Assert.True(eligible.IsEligible);
        }

        // -----------------------------------------------------------------------
        // 9. Score out of all ProductCap ranges → amount=0, error message
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_ScoreOutOfRange_ShouldReturnZeroAmountWithMessage()
        {
            var product = new Product { ProductId = 60, ProductName = "Bronze", Code = "BRZ", TenantId = TenantId, MaxEligibleAmount = 15000 };
            var master  = new EruleMaster { Id = 1, EruleName = "R1", IsActive = true, TenantId = TenantId };
            var rule    = new Erule { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>18", Version = 1, EruleMaster = master, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor  = new Factor { FactorId = 1, ParameterId = PIdAge, TenantId = TenantId };
            var ecard   = new Ecard { EcardId = 1, Expression = "1", TenantId = TenantId };
            var pcard   = new Pcard { PcardId = 1, Expression = "1", ProductId = 60, Product = product, TenantId = TenantId };
            var capAmt  = new ProductCapAmount { ProductId = 60, TenantId = TenantId, Age = "All", Salary = "All", Amount = 15000 };
            // ProductCap only covers score 70-90; customer score=50 → no match
            var cap     = new ProductCap { ProductId = 60, TenantId = TenantId, MinimumScore = 70, MaximumScore = 90, ProductCapPercentage = 100 };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { master }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmt }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { cap }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "N006" }, { "LoanNo", "L006" },
                { "Age", "25" }, { "score", "50" }  // score 50 outside 70-90
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-7");

            Assert.NotNull(result);
            // Should appear in non-eligible or eligible with 0 amount
            var prod = result.EligibleProducts?.FirstOrDefault(p => p.ProductCode == "BRZ");
            if (prod != null)
            {
                Assert.Equal(0, prod.EligibleAmount);
                Assert.False(prod.IsEligible);
            }
            else
            {
                var nonElig = result.NonEligibleProducts?.FirstOrDefault(p => p.ProductCode == "BRZ");
                Assert.NotNull(nonElig);
            }
        }

        // -----------------------------------------------------------------------
        // 10. Age cap fails, Salary cap matches → correct amount from salary cap row
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_SalaryCapRowMatches_ShouldReturnCorrectEligibleAmount()
        {
            var product = new Product { ProductId = 70, ProductName = "Salary Plan", Code = "SAL", TenantId = TenantId, MaxEligibleAmount = 60000 };
            var master  = new EruleMaster { Id = 1, EruleName = "R1", IsActive = true, TenantId = TenantId };
            var rule    = new Erule { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>18", Version = 1, EruleMaster = master, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor  = new Factor { FactorId = 1, ParameterId = PIdAge, TenantId = TenantId };
            var ecard   = new Ecard { EcardId = 1, Expression = "1", TenantId = TenantId };
            var pcard   = new Pcard { PcardId = 1, Expression = "1", ProductId = 70, Product = product, TenantId = TenantId };
            // Two cap-amount rows: first fails (Age>40), second succeeds (Salary 30000-60000)
            var capAmt1 = new ProductCapAmount { ProductId = 70, TenantId = TenantId, Age = ">40", Salary = "All", Amount = 30000 };
            var capAmt2 = new ProductCapAmount { ProductId = 70, TenantId = TenantId, Age = "All", Salary = "30000-60000", Amount = 12000 };
            var cap     = new ProductCap { ProductId = 70, TenantId = TenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 50 };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { master }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmt1, capAmt2 }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { cap }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            // Age=25 fails capAmt1 (>40), Salary=45000 in 30000-60000 → capAmt2 matches
            var input = new Dictionary<string, object>
            {
                { "NationalId", "N007" }, { "LoanNo", "L007" },
                { "Age", "25" }, { "Salary", "45000" }, { "score", "60" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-8");

            Assert.NotNull(result);
            var eligible = result.EligibleProducts?.FirstOrDefault(p => p.ProductCode == "SAL");
            Assert.NotNull(eligible);
            Assert.Equal(6000m, eligible.EligibleAmount); // 50% of 12000
        }

        // -----------------------------------------------------------------------
        // 11. Two products: one eligible, one has no PCard → both returned correctly
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_TwoProducts_OneEligibleOneNoPCard_BothInResponse()
        {
            var prodA = new Product { ProductId = 80, ProductName = "A", Code = "PA", TenantId = TenantId, MaxEligibleAmount = 10000 };
            var prodB = new Product { ProductId = 81, ProductName = "B", Code = "PB", TenantId = TenantId };
            var master = new EruleMaster { Id = 1, EruleName = "R1", IsActive = true, TenantId = TenantId };
            var rule   = new Erule { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>18", Version = 1, EruleMaster = master, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor = new Factor { FactorId = 1, ParameterId = PIdAge, TenantId = TenantId };
            var ecard  = new Ecard { EcardId = 1, Expression = "1", TenantId = TenantId };
            // Only prodA has a PCard; prodB has no PCard
            var pcard  = new Pcard { PcardId = 1, Expression = "1", ProductId = 80, Product = prodA, TenantId = TenantId };
            var capAmt = new ProductCapAmount { ProductId = 80, TenantId = TenantId, Age = "All", Salary = "All", Amount = 10000 };
            var cap    = new ProductCap { ProductId = 80, TenantId = TenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { prodA, prodB }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { master }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmt }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { cap }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "N008" }, { "LoanNo", "L008" },
                { "Age", "25" }, { "score", "60" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-9");

            Assert.NotNull(result);
            Assert.Contains(result.EligibleProducts ?? [], p => p.ProductCode == "PA");
            Assert.Contains(result.NonEligibleProducts ?? [], p => p.ProductCode == "PB");
        }

        // -----------------------------------------------------------------------
        // 12. Complex nested ECard + PCard: "(E1 AND E2) OR E3" on PCard level
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_Complex_NestedPCardExpression_OneGroupPasses_ShouldBeEligible()
        {
            var product = new Product { ProductId = 90, ProductName = "Complex", Code = "CMX", TenantId = TenantId, MaxEligibleAmount = 50000 };
            var m1 = new EruleMaster { Id = 1, EruleName = "R1", IsActive = true, TenantId = TenantId };
            var m2 = new EruleMaster { Id = 2, EruleName = "R2", IsActive = true, TenantId = TenantId };
            var m3 = new EruleMaster { Id = 3, EruleName = "R3", IsActive = true, TenantId = TenantId };
            // R1: Age>40 FAILS, R2: Salary>50000 FAILS, R3: Age>20 PASSES
            var rule1 = new Erule { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdAge}>40",       Version = 1, EruleMaster = m1, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var rule2 = new Erule { EruleId = 2, EruleMasterId = 2, Expression = $"{PIdSalary}>50000", Version = 1, EruleMaster = m2, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var rule3 = new Erule { EruleId = 3, EruleMasterId = 3, Expression = $"{PIdAge}>20",       Version = 1, EruleMaster = m3, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var f1 = new Factor { FactorId = 1, ParameterId = PIdAge,    TenantId = TenantId };
            var f2 = new Factor { FactorId = 2, ParameterId = PIdSalary, TenantId = TenantId };
            // E1→R1, E2→R2, E3→R3
            var e1 = new Ecard { EcardId = 300, Expression = "1", TenantId = TenantId };
            var e2 = new Ecard { EcardId = 301, Expression = "2", TenantId = TenantId };
            var e3 = new Ecard { EcardId = 302, Expression = "3", TenantId = TenantId };
            // PCard: "(300 AND 301) OR 302" → (false AND false) OR true = true
            var pcard = new Pcard { PcardId = 90, Expression = "(300 AND 301) OR 302", ProductId = 90, Product = product, TenantId = TenantId };
            var capAmt = new ProductCapAmount { ProductId = 90, TenantId = TenantId, Age = "All", Salary = "All", Amount = 50000 };
            var cap    = new ProductCap { ProductId = 90, TenantId = TenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule1, rule2, rule3 }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { m1, m2, m3 }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { f1, f2 }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { e1, e2, e3 }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmt }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { cap }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "N009" }, { "LoanNo", "L009" },
                { "Age", "25" }, { "Salary", "30000" }, { "score", "55" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-10");

            Assert.NotNull(result);
            var eligible = result.EligibleProducts?.FirstOrDefault(p => p.ProductCode == "CMX");
            Assert.NotNull(eligible);
            Assert.True(eligible.IsEligible);
        }

        // -----------------------------------------------------------------------
        // 13. External API enriches Salary; rule uses API-provided Salary → eligible
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_ExternalApi_ProvidesSalary_RuleUsesIt_ShouldBeEligible()
        {
            var product = new Product { ProductId = 100, ProductName = "ApiPlan", Code = "API", TenantId = TenantId, MaxEligibleAmount = 40000 };
            var master  = new EruleMaster { Id = 1, EruleName = "SalRule", IsActive = true, TenantId = TenantId };
            // Rule: Salary>30000 — Salary will come from external API
            var rule    = new Erule { EruleId = 1, EruleMasterId = 1, Expression = $"{PIdSalary}>30000", Version = 1, EruleMaster = master, TenantId = TenantId, ValidFrom = DateTime.Now.AddDays(-1) };
            var factor  = new Factor { FactorId = 1, ParameterId = PIdSalary, TenantId = TenantId };
            var ecard   = new Ecard { EcardId = 1, Expression = "1", TenantId = TenantId };
            var pcard   = new Pcard { PcardId = 1, Expression = "1", ProductId = 100, Product = product, TenantId = TenantId };
            var capAmt  = new ProductCapAmount { ProductId = 100, TenantId = TenantId, Age = "All", Salary = "All", Amount = 40000 };
            var cap     = new ProductCap { ProductId = 100, TenantId = TenantId, MinimumScore = 0, MaximumScore = 100, ProductCapPercentage = 100 };

            // External API returns salary=45000
            var node   = new Node   { NodeId = 1, NodeUrl = "http://ext-api", TenantId = TenantId };
            var api    = new NodeApi { Apiid = 1, NodeId = 1, Apiname = "FinApi", IsActive = true, TenantId = TenantId, HttpMethodType = "GET", EndpointPath = "/salary", RequestBody = "{}", RequestParameters = "{}" };
            var apiParam = new ApiParameter { ApiParamterId = 1, ApiId = 1, ParameterName = "salary", ParameterDirection = "Output", TenantId = TenantId };
            var apiMap   = new ApiParameterMap { ApiParameterId = 1, ParameterId = PIdSalary, ApiId = 1, TenantId = TenantId };

            _mockUow.Setup(u => u.NodeModelRepository.GetAll()).Returns(new List<Node> { node });
            _mockUow.Setup(u => u.NodeApiRepository.GetAll()).Returns(new List<NodeApi> { api });
            _mockUow.Setup(u => u.ApiParametersRepository.Query()).Returns(new List<ApiParameter> { apiParam }.BuildMock());
            _mockUow.Setup(u => u.ApiParametersRepository.GetAll()).Returns(new List<ApiParameter> { apiParam });
            _mockUow.Setup(u => u.ApiParameterMapsRepository.Query()).Returns(new List<ApiParameterMap> { apiMap }.BuildMock());

            // Override HttpClient to return salary=45000
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{\"salary\":45000}") });
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler.Object));

            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ProductRepository.Query()).Returns(new List<Product> { product }.BuildMock());
            _mockUow.Setup(u => u.EruleRepository.Query()).Returns(new List<Erule> { rule }.BuildMock());
            _mockUow.Setup(u => u.EruleMasterRepository.Query()).Returns(new List<EruleMaster> { master }.BuildMock());
            _mockUow.Setup(u => u.FactorRepository.Query()).Returns(new List<Factor> { factor }.BuildMock());
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard> { ecard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard> { pcard }.BuildMock());
            _mockUow.Setup(u => u.PcardRepository.GetByIds(It.IsAny<List<int>>())).Returns(new List<Pcard> { pcard });
            _mockUow.Setup(u => u.ProductCapAmountRepository.Query()).Returns(new List<ProductCapAmount> { capAmt }.BuildMock());
            _mockUow.Setup(u => u.ProductCapRepository.Query()).Returns(new List<ProductCap> { cap }.BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            // No Salary in input — comes from API
            var input = new Dictionary<string, object>
            {
                { "NationalId", "N010" }, { "LoanNo", "L010" }, { "score", "70" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-11");

            Assert.NotNull(result);
            var eligible = result.EligibleProducts?.FirstOrDefault(p => p.ProductCode == "API");
            Assert.NotNull(eligible);
            Assert.True(eligible.IsEligible);
        }

        // -----------------------------------------------------------------------
        // 14. customer score returned in response matches input
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_ScoreInResponse_ShouldMatchInputScore()
        {
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "N011" }, { "LoanNo", "L011" }, { "score", "88" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "req-12");

            Assert.NotNull(result);
            Assert.Equal(88, result.CustomerScore);
        }

        // -----------------------------------------------------------------------
        // 15. RequestId is echoed back in response
        // -----------------------------------------------------------------------
        [Fact]
        public async Task BRE_RequestId_ShouldBeEchoedInResponse()
        {
            _mockUow.Setup(u => u.ParameterRepository.Query()).Returns(StandardParams().BuildMock());
            _mockUow.Setup(u => u.ParameterBindingRepository.Query()).Returns(new List<ParameterBinding>().BuildMock());

            var input = new Dictionary<string, object>
            {
                { "NationalId", "N012" }, { "LoanNo", "L012" }
            };

            var result = await _service.ProcessBREIntegration(input, TenantId, "MYREQ-999");

            Assert.NotNull(result);
            Assert.Equal("MYREQ-999", result.RequestId);
        }
    }
}

