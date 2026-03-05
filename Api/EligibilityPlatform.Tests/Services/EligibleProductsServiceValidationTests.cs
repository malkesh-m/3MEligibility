using System;
using System.Collections.Generic;
using System.Net.Http;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using EligibilityPlatform.Tests.Helpers;

namespace EligibilityPlatform.Tests.Services
{
    public class EligibleProductsServiceValidationTests
    {
        [Fact]
        public void Validate_ShouldHandleRangeAndListConditions()
        {
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<EligibleProductsService>>();

            var managedListRepo = new Mock<IManagedListRepository>();
            var listItemRepo = new Mock<IListItemRepository>();

            mockUow.Setup(u => u.ManagedListRepository).Returns(managedListRepo.Object);
            mockUow.Setup(u => u.ListItemRepository).Returns(listItemRepo.Object);

            managedListRepo.Setup(r => r.Query()).Returns(new List<ManagedList>
            {
                new() { ListId = 1, ListName = "MyList", TenantId = 1 }
            }.BuildMock());

            listItemRepo.Setup(r => r.Query()).Returns(new List<ListItem>
            {
                new() { ListId = 1, TenantId = 1, ItemName = "A" }
            }.BuildMock());

            var service = new EligibleProductsService(mockUow.Object, mockLogger.Object, new TestHttpClientFactory(new HttpClient()));

            var method = typeof(EligibleProductsService).GetMethod("Validate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            var eq = (ValidationDetail)method!.Invoke(service, new object[] { new Condition { ConditionValue = "=" }, new Factor { ParameterId = 1, Value1 = "X", FactorName = "F1" }, "X", 1, "Text", null })!;
            var range = (ValidationDetail)method.Invoke(service, new object[] { new Condition { ConditionValue = "Range" }, new Factor { ParameterId = 2, Value1 = "2020-01-01", Value2 = "2020-12-31", FactorName = "F2" }, "2020-06-01", 1, "Date", null })!;
            var inList = (ValidationDetail)method.Invoke(service, new object[] { new Condition { ConditionValue = "In List" }, new Factor { ParameterId = 3, Value1 = "MyList", FactorName = "F3" }, "A", 1, "Text", null })!;
            var notInList = (ValidationDetail)method.Invoke(service, new object[] { new Condition { ConditionValue = "Not In List" }, new Factor { ParameterId = 3, Value1 = "MyList", FactorName = "F3" }, "A", 1, "Text", null })!;

            Assert.True(eq.IsValid);
            Assert.True(range.IsValid);
            Assert.True(inList.IsValid);
            Assert.False(notInList.IsValid);
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
