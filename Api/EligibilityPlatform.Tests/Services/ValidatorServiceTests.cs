using Moq;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using EligibilityPlatform.Tests.Helpers;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Tests.Services
{
    public class ValidatorServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IHistoryPcService> _mockHistoryPcService;
        private readonly ValidatorService _service;

        public ValidatorServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockHistoryPcService = new Mock<IHistoryPcService>();
            _service = new ValidatorService(_mockUow.Object, _mockHistoryPcService.Object);
        }

        private void SetupBasics()
        {
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition>
            {
                new() { ConditionValue = "=" },
                new() { ConditionValue = "!=" },
                new() { ConditionValue = "<" },
                new() { ConditionValue = ">" },
                new() { ConditionValue = "<=" },
                new() { ConditionValue = ">=" },
                new() { ConditionValue = "Range" },
                new() { ConditionValue = "In List" },
                new() { ConditionValue = "Not In List" }
            });

            _mockUow.Setup(u => u.DataTypeRepository.GetById(1)).Returns(new DataType { DataTypeId = 1, DataTypeName = "Number" });
            _mockUow.Setup(u => u.DataTypeRepository.GetById(2)).Returns(new DataType { DataTypeId = 2, DataTypeName = "Date" });
            _mockUow.Setup(u => u.DataTypeRepository.GetById(3)).Returns(new DataType { DataTypeId = 3, DataTypeName = "String" });

            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(new List<Factor>());
        }

        // =============================================================================
        // BASIC RULE VALIDATION (ValidateRule / ValidateEntity)
        // =============================================================================

        [Theory]
        [InlineData("=", "25", 25, true)]
        [InlineData("=", "25", 30, false)]
        [InlineData("!=", "25", 30, true)]
        [InlineData("<", "25", 20, true)]
        [InlineData(">", "25", 30, true)]
        [InlineData("<=", "25", 25, true)]
        [InlineData(">=", "25", 25, true)]
        public void ValidateRule_NumericComparisons(string op, string factorVal, int inputVal, bool expected)
        {
            SetupBasics();
            var rule = new Erule { EruleId = 1, ExpShown = $"Age {op} {factorVal}" };
            var factors = new List<Factor> { new Factor { FactorName = "Age", ParameterId = 1, Value1 = factorVal } };
            
            _mockUow.Setup(u => u.EruleRepository.GetById(1)).Returns(rule);
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(factors);
            _mockUow.Setup(u => u.ParameterRepository.GetById(1)).Returns(new Parameter { ParameterId = 1, DataTypeId = 1 });

            var result = _service.ValidateRule(1, new Dictionary<int, object> { { 1, inputVal } });

            Assert.Equal(expected, result.IsValidationPassed);
        }

        [Fact]
        public void ValidateRule_Range_Numeric_Success()
        {
            SetupBasics();
            var rule = new Erule { EruleId = 2, ExpShown = "Salary Range 3000-5000" };
            // IMPORTANT: ValidatorService.Validate(Range) checks facts[1] which must match factor.Value1
            // In EvaluateExpression, splitting "Salary Range 3000-5000" by "range" gives facts[0]="Salary", facts[1]="3000-5000"
            // So factor.Value1 must be "3000-5000" for ValidateEntity to find it.
            var factors = new List<Factor> { new Factor { FactorName = "Salary", ParameterId = 2, Value1 = "3000-5000", Value2 = "5000" } };
            // Wait, looking at EligibleProductsService.cs:730 (Range case):
            // if (double.TryParse(factor.Value1, out var lowerBound) && double.TryParse(factor.Value2, out var upperBound) ...)
            // So facts[1] must match factor.Value1. 
            // If the expression is "Salary Range 3000", facts[1] is "3000".
            
            var factorsFixed = new List<Factor> { new Factor { FactorName = "Salary", ParameterId = 2, Value1 = "3000", Value2 = "5000" } };
            _mockUow.Setup(u => u.EruleRepository.GetById(2)).Returns(new Erule { EruleId = 2, ExpShown = "Salary Range 3000" });
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(factorsFixed);
            _mockUow.Setup(u => u.ParameterRepository.GetById(2)).Returns(new Parameter { ParameterId = 2, DataTypeId = 1 });

            // This should match: condition "Range" found in "Salary Range 3000", facts[1] is "3000", which equals factor.Value1
            var result = _service.ValidateRule(2, new Dictionary<int, object> { { 2, 4000 } });

            Assert.True(result.IsValidationPassed);
        }

        [Fact]
        public void ValidateRule_DateRange_Success()
        {
            SetupBasics();
            var factors = new List<Factor> { new Factor { FactorName = "DOB", ParameterId = 3, Value1 = "1990-01-01", Value2 = "2000-01-01" } };
            _mockUow.Setup(u => u.EruleRepository.GetById(3)).Returns(new Erule { EruleId = 3, ExpShown = "DOB Range 1990-01-01" });
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(factors);
            _mockUow.Setup(u => u.ParameterRepository.GetById(3)).Returns(new Parameter { ParameterId = 3, DataTypeId = 2 });

            var result = _service.ValidateRule(3, new Dictionary<int, object> { { 3, "1995-05-05" } });

            Assert.True(result.IsValidationPassed);
        }

        [Fact]
        public void ValidateRule_InList_Success()
        {
            SetupBasics();
            _mockUow.Setup(u => u.EruleRepository.GetById(4)).Returns(new Erule { EruleId = 4, ExpShown = "City In List SaudiCities" });
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(new List<Factor> { new Factor { FactorName = "City", ParameterId = 4, Value1 = "SaudiCities" } });
            _mockUow.Setup(u => u.ParameterRepository.GetById(4)).Returns(new Parameter { ParameterId = 4, DataTypeId = 3 });
            
            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(new List<ManagedList> { new ManagedList { ListId = 10, ListName = "SaudiCities" } }.BuildMock());
            _mockUow.Setup(u => u.ListItemRepository.GetAll()).Returns(new List<ListItem> { new ListItem { ListId = 10, ItemName = "Riyadh" }, new ListItem { ListId = 10, ItemName = "Jeddah" } });

            var result = _service.ValidateRule(4, new Dictionary<int, object> { { 4, "Riyadh" } });

            Assert.True(result.IsValidationPassed);
        }

        [Fact]
        public void ValidateRule_NotInList_Success()
        {
            SetupBasics();
            _mockUow.Setup(u => u.EruleRepository.GetById(5)).Returns(new Erule { EruleId = 5, ExpShown = "Status Not In List Blocked" });
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(new List<Factor> { new Factor { FactorName = "Status", ParameterId = 5, Value1 = "Blocked" } });
            _mockUow.Setup(u => u.ParameterRepository.GetById(5)).Returns(new Parameter { ParameterId = 5, DataTypeId = 3 });
            
            _mockUow.Setup(u => u.ManagedListRepository.Query()).Returns(new List<ManagedList> { new ManagedList { ListId = 20, ListName = "Blocked" } }.BuildMock());
            _mockUow.Setup(u => u.ListItemRepository.GetAll()).Returns(new List<ListItem> { new ListItem { ListId = 20, ItemName = "Fraud" } });

            var result = _service.ValidateRule(5, new Dictionary<int, object> { { 5, "Active" } });

            Assert.True(result.IsValidationPassed);
        }

        [Fact]
        public void ValidateRule_AllValue_ShouldAlwaysPass()
        {
            SetupBasics();
            // If factor.Value1 is "All", it should return true regardless of input
            _mockUow.Setup(u => u.EruleRepository.GetById(6)).Returns(new Erule { EruleId = 6, ExpShown = "Field = All" });
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(new List<Factor> { new Factor { FactorName = "Field", ParameterId = 6, Value1 = "All" } });
            _mockUow.Setup(u => u.ParameterRepository.GetById(6)).Returns(new Parameter { ParameterId = 6, DataTypeId = 3 });

            var result = _service.ValidateRule(6, new Dictionary<int, object> { { 6, "Anything" } });

            Assert.True(result.IsValidationPassed);
        }

        // =============================================================================
        // EXPRESSION EVALUATION (AND / OR / Parentheses)
        // =============================================================================

        [Fact]
        public void ProcessExpression_AND_FailOnFirst()
        {
            SetupBasics();
            // f1 = 10 (T) AND f2 = 10 (F) => False (because 20 != 10)
            // ValidateEntity splits by condition and then looks up factor by name (lowercase, no spaces).
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(new List<Factor> 
            { 
                new Factor { FactorName = "f1", ParameterId = 1, Value1 = "10" },
                new Factor { FactorName = "f2", ParameterId = 2, Value1 = "10" }
            });
            _mockUow.Setup(u => u.ParameterRepository.GetById(1)).Returns(new Parameter { ParameterId = 1, DataTypeId = 1 });
            _mockUow.Setup(u => u.ParameterRepository.GetById(2)).Returns(new Parameter { ParameterId = 2, DataTypeId = 1 });

            var result = _service.ValidateFormErule("f1 = 10 AND f2 = 10", new Dictionary<int, object> { { 1, 10 }, { 2, 20 } });

            Assert.False(result.IsValidationPassed, "AND should be false if one part fails");
        }

        [Fact]
        public void ProcessExpression_OR_PassOnSecond()
        {
            SetupBasics();
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(new List<Factor> 
            { 
                new Factor { FactorName = "F1", ParameterId = 1, Value1 = "10" },
                new Factor { FactorName = "F2", ParameterId = 2, Value1 = "20" }
            });
            _mockUow.Setup(u => u.ParameterRepository.GetById(1)).Returns(new Parameter { ParameterId = 1, DataTypeId = 1 });
            _mockUow.Setup(u => u.ParameterRepository.GetById(2)).Returns(new Parameter { ParameterId = 2, DataTypeId = 1 });

            var result = _service.ValidateFormErule("F1=5 OR F2=20", new Dictionary<int, object> { { 1, 10 }, { 2, 20 } });

            Assert.True(result.IsValidationPassed);
        }

        [Fact]
        public void ProcessExpression_NestedParentheses_Complex()
        {
            SetupBasics();
            // ( (T AND F) OR T ) AND T => True
            var factors = new List<Factor> { new Factor { FactorName = "F1", ParameterId = 1, Value1 = "X" } };
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(factors);
            _mockUow.Setup(u => u.ParameterRepository.GetById(1)).Returns(new Parameter { ParameterId = 1, DataTypeId = 3 });

            // We'll use literals to test pure expression logic
            var expression = "((true AND false) OR true) AND true";
            var result = _service.ValidateFormErule(expression, new Dictionary<int, object>());

            Assert.True(result.IsValidationPassed);
        }

        [Fact]
        public void ProcessExpression_Literals_Comparisons()
        {
            SetupBasics();
            // Mock the Rule if needed (though ValidateFormErule uses direct expression)
            // But if it ever tries to Parse numbers as Rules:
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(new List<Factor>());
            
            var result = _service.ValidateFormErule("100 > 50 AND true = true AND 5.5 <= 10", new Dictionary<int, object>());

            Assert.True(result.IsValidationPassed);
        }

        // =============================================================================
        // ECARD / PCARD FLOWS
        // =============================================================================

        [Fact]
        public async Task ValidAsync_PcardNotFound_ThrowsException()
        {
            _mockUow.Setup(u => u.PcardRepository.Query()).Returns(new List<Pcard>().BuildMock());
            
            await Assert.ThrowsAsync<Exception>(() => _service.ValidAsync(1, 999, new Dictionary<int, object>()));
        }

        [Fact]
        public async Task ValidateCard_EcardNotFound_ThrowsException()
        {
            _mockUow.Setup(u => u.EcardRepository.Query()).Returns(new List<Ecard>().BuildMock());
            
            await Assert.ThrowsAsync<Exception>(() => _service.ValidateCard(888, new Dictionary<int, object>()));
        }

        [Fact]
        public void ValidateFormECard_MultipleNumbers_Success()
        {
            SetupBasics();
            // ValidateFormECard extracts numbers from "101, 102" -> ("101"), ("102")
            // Then calls ProcessExpression( "(101)", ...) and ProcessExpression( "(102)", ...)
            var ecards = new List<Ecard> 
            { 
                new Ecard { EcardId = 101, Expression = "true" },
                new Ecard { EcardId = 102, Expression = "true" }
            };
            _mockUow.Setup(u => u.EcardRepository.GetAll()).Returns(ecards);
            // Since ValidateFormECard calls ProcessExpression which might look for Rule 101/102
            _mockUow.Setup(u => u.EruleRepository.GetById(101)).Returns(new Erule { EruleId = 101, ExpShown = "true" });
            _mockUow.Setup(u => u.EruleRepository.GetById(102)).Returns(new Erule { EruleId = 102, ExpShown = "true" });
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(new List<Factor>());

            var result = _service.ValidateFormECard("101 AND 102", new Dictionary<int, object>());

            Assert.True(result.IsValidationPassed);
        }

        // =============================================================================
        // EDGE CASES & ERROR HANDLING
        // =============================================================================

        [Fact]
        public void Validate_NullValue_ReturnsInvalidWithErrorMessage()
        {
            SetupBasics();
            var factors = new List<Factor> { new Factor { FactorName = "F1", ParameterId = 1, Value1 = "10" } };
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(factors);
            _mockUow.Setup(u => u.ParameterRepository.GetById(1)).Returns(new Parameter { ParameterId = 1, DataTypeId = 1 });

            // Passing null as the value in keyValues
            var result = _service.ValidateFormErule("F1=10", new Dictionary<int, object> { { 1, null! } });

            Assert.False(result.IsValidationPassed);
            Assert.Contains("Arguments cannot be null", result.ValidationDetails[0].ErrorMessage![0]);
        }

        [Fact]
        public void ProcessExpression_MalformedParentheses_HandledByException()
        {
            SetupBasics();
            var expression = "true AND (false"; 
            
            var result = _service.ValidateFormErule(expression, new Dictionary<int, object>());
            Assert.False(result.IsValidationPassed, "Malformed expression should return failure, not throw.");
        }

        [Fact]
        public void ValidateRule_Range_Hyphenated_Success()
        {
            SetupBasics();
            // User case: Expression "Age Range 18-25"
            // Factor has Value1 = "18-25", Value2 = null
            var factors = new List<Factor> { new Factor { FactorName = "Age", ParameterId = 1, Value1 = "18-25", Value2 = null } };
            _mockUow.Setup(u => u.EruleRepository.GetById(1)).Returns(new Erule { EruleId = 1, ExpShown = "Age Range 18-25" });
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(factors);
            _mockUow.Setup(u => u.ParameterRepository.GetById(1)).Returns(new Parameter { ParameterId = 1, DataTypeId = 1 });

            var result = _service.ValidateRule(1, new Dictionary<int, object> { { 1, 20 } });

            Assert.True(result.IsValidationPassed, "Should support hyphenated range '18-25' in Value1 when Value2 is null");
        }

        [Fact]
        public void ProcessExpression_Range_Literal_Success()
        {
            SetupBasics();
            // User case: "( 20 Range 18-25 )"
            var result = _service.ValidateFormErule("( 20 Range 18-25 )", new Dictionary<int, object>());

            Assert.True(result.IsValidationPassed, "Should support literal range comparison '( 20 Range 18-25 )'");
        }

        [Fact]
        public void Validate_WhitespaceHandling()
        {
            SetupBasics();
            // F1: Use ParameterId 10, Value1 "John"
            var factors = new List<Factor> { new Factor { FactorName = "Name", ParameterId = 10, Value1 = "John" } };
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(factors);
            _mockUow.Setup(u => u.ParameterRepository.GetById(10)).Returns(new Parameter { ParameterId = 10, DataTypeId = 3 });

            // Service now standardizes "Name=John" to "Name = John" internally if it uses logical ops, 
            // but for a single rule it relies on ValidateEntity splitting by condition.
            // Our regex @"\s*" + Regex.Escape(conditionValue) + @"\s*" handles this!
            var result = _service.ValidateFormErule("Name=John", new Dictionary<int, object> { { 10, "John" } });

            Assert.True(result.IsValidationPassed, "Should handle missing spaces around '='");
        }
        [Fact]
        public void ValidateCard_WithRangeRule_Success()
        {
            SetupBasics();
            // ECard 1 contains Rule 1.
            // Rule 1: "Age Range 18-25"
            var rule = new Erule { EruleId = 1, ExpShown = "Age Range 18-25" };
            var eCard = new Ecard { EcardId = 1, Expression = "1" }; // Just Rule 1
            
            var factors = new List<Factor> { new Factor { FactorName = "Age", ParameterId = 1, Value1 = "18-25" } };
            
            _mockUow.Setup(u => u.EcardRepository.GetAll()).Returns(new List<Ecard> { eCard });
            _mockUow.Setup(u => u.EruleRepository.GetById(1)).Returns(rule);
            _mockUow.Setup(u => u.FactorRepository.GetAll()).Returns(factors);
            _mockUow.Setup(u => u.ParameterRepository.GetById(1)).Returns(new Parameter { ParameterId = 1, DataTypeId = 1 });
            _mockUow.Setup(u => u.DataTypeRepository.GetById(1)).Returns(new DataType { DataTypeId = 1, DataTypeName = "int" });
            _mockUow.Setup(u => u.ConditionRepository.GetAll()).Returns(new List<Condition> { new Condition { ConditionValue = "Range" } });

            var result = _service.ValidateFormECard("1", new Dictionary<int, object> { { 1, 22 } });

            Assert.True(result.IsValidationPassed, "ECard should correctly validate a Rule containing a Range.");
        }
    }
}
