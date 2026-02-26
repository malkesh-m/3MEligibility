using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Numerics;
using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ValidationResult = MEligibilityPlatform.Domain.Models.ValidationResult;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing eligibility operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EligibilityService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    public class EligibilityService(IUnitOfWork uow) : IEligibilityService
    {
        // Declares a private readonly field for unit of work
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// Validates the eligibility of a user for a product based on provided key values.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>A <see cref="ValidationResult"/> containing the validation outcome and details.</returns>
        public async Task<ValidationResult> ValidAsync(int userId, int productId, Dictionary<int, object> keyValues)
        {
            // Retrieves product details from repository by productId
            var productDetails = _uow.ProductRepository.GetById(productId) ?? throw new Exception("Product not found.");

            // Initializes exceptionManagement variable as null
            ExceptionManagement? exceptionManagement = null;
            // Queries ExceptionProduct repository to find product exception with included ExceptionManagement
            var exceptionProduct = await _uow.ExceptionProductRepository.Query().Include(i => i.ExceptionManagement).FirstOrDefaultAsync(f => f.ProductId == productId);
            // If exception product exists, assigns its ExceptionManagement
            if (exceptionProduct != null)
            {
                exceptionManagement = exceptionProduct.ExceptionManagement;
            }

            // Creates a new ValidationResult instance
            ValidationResult result = new();

            // Queries Pcard repository to find Pcard associated with the product
            var pCard = await _uow.PcardRepository.Query()
                     .FirstOrDefaultAsync(p => p.ProductId == productId) ?? throw new Exception("Pcard not found for the given product.");

            // Checks if exception management exists and has Product Eligibility scope
            if (exceptionManagement != null && exceptionManagement.Scope == "Product Eligibility")
            {
                // Gets current date and time
                DateTime currentDate = DateTime.Now;

                // Checks if current date is within exception date range
                bool isWithinDateRange =
                    exceptionManagement.StartDate.HasValue &&
                    exceptionManagement.EndDate.HasValue &&
                    currentDate >= exceptionManagement.StartDate &&
                    currentDate <= exceptionManagement.EndDate;

                // Processes exception if within date range or temporary
                if (isWithinDateRange || exceptionManagement.IsTemporary == true)
                {
                    // Retrieves all factors from repository
                    var factors = _uow.FactorRepository.GetAll();
                    // Processes the exception expression for validation
                    result = ProcessExpression(exceptionManagement.Expression, factors, keyValues, "Rule");
                }
                else
                {
                    // Throws exception if product exception is expired or disabled
                    throw new Exception("the given ProductException is Expired or Disabled");
                }
            }
            else
            {
                // Retrieves all Ecards from repository
                var eCards = _uow.EcardRepository.GetAll();
                // Processes the Pcard expression for validation
                result = ProcessExpression(pCard.Expression, eCards, keyValues, "ECard");
            }

            // Groups validation details and removes duplicates
            result.ValidationDetails = [.. result.ValidationDetails
                .GroupBy(detail => new
                {
                    detail.IsValid,
                    detail.FactorName,
                    detail.Condition,
                    detail.FactorValue,
                    detail.ProvidedValue,
                    detail.ErrorMessage,
                    detail.ParameterId
                })
                .Select(group => group.First())];

            // Calculates percentage value per validation detail
            double percentailValue = 100.0 / result.ValidationDetails.Count;
            // Calculates eligibility percentage based on valid details
            double eligibilityPercentage = result.ValidationDetails.Count(d => d.IsValid) * percentailValue;

            // Creates a new HistoryPc record for tracking
            HistoryPc model = new()
            {
                PcardId = pCard?.PcardId ?? 0,
                Expression = pCard?.Expression ?? null,
                ProductId = pCard?.ProductId ?? 0,
                TenantId = pCard?.TenantId ?? 0,
                UserId = userId,
                TransReference = Guid.NewGuid().ToString()[..8],
                TransactionDate = DateTime.Now,
                Result = result.IsValidationPassed,
                UpdatedByDateTime = DateTime.Now
            };

            // Adds the history record to repository
            _uow.HistoryPcRepository.Add(model);

            // Returns validation result with eligibility percentage
            return new ValidationResult
            {
                IsValidationPassed = result.IsValidationPassed,
                ValidationDetails = result.ValidationDetails,
                EligibilityPercentage = eligibilityPercentage
            };
        }

        /// <summary>
        /// Processes the given expression recursively.
        /// </summary>
        /// <param name="expression">The logical expression to process.</param>
        /// <param name="entities">A collection of entities to validate against.</param>
        /// <param name="keyValues">A dictionary of key-value pairs for validation.</param>
        /// <param name="type">The type of entity being validated.</param>
        /// <returns>A <see cref="ValidationResult"/> containing the validation result.</returns>
        private ValidationResult ProcessExpression(string expression, IEnumerable<object> entities, Dictionary<int, object> keyValues, string type)
        {
            // Initializes list for validation details
            var validationDetails = new List<ValidationDetail>();

            // Removes spaces from the expression
            expression = RemoveSpaces(expression);

            // Processes nested expressions recursively
            while (expression.Contains('('))
            {
                // Gets the innermost expression within parentheses
                var innerMostExpression = GetInnerMostExpression(expression);
                // Recursively processes the inner expression
                var innerResult = ProcessExpression(innerMostExpression, entities, keyValues, type);

                // Replaces the inner expression with its boolean result
                expression = expression.Replace($"({innerMostExpression})", innerResult.IsValidationPassed ? "true" : "false");
                // Adds inner validation details to the main list
                validationDetails.AddRange(innerResult.ValidationDetails);
            }

            // Evaluates the final processed expression
            var finalResult = EvaluateExpression(expression, entities, keyValues, type);
            // Adds final validation details to the list
            validationDetails.AddRange(finalResult.ValidationDetails);

            // Returns the combined validation result
            return new ValidationResult
            {
                IsValidationPassed = finalResult.IsValidationPassed,
                ValidationDetails = validationDetails
            };
        }

        /// <summary>
        /// Extracts the innermost expression within parentheses.
        /// </summary>
        /// <param name="expression">The expression string.</param>
        /// <returns>The innermost expression.</returns>
        private static string GetInnerMostExpression(string expression)
        {
            // Finds the last occurrence of '('
            var start = expression.LastIndexOf('(');
            // Finds the corresponding closing ')' after the start
            var end = expression.IndexOf(')', start);
            // Extracts the substring between parentheses
            return expression.Substring(start + 1, end - start - 1);
        }

        /// <summary>
        /// Evaluates a complex logical expression.
        /// </summary>
        /// <param name="expression">A logical expression.</param>
        /// <param name="entities">A collection of entities.</param>
        /// <param name="keyValues">A dictionary of key-value pairs.</param>
        /// <param name="type">The type of entity.</param>
        /// <returns>A <see cref="ValidationResult"/>.</returns>
        private ValidationResult EvaluateExpression(string expression, IEnumerable<object> entities, Dictionary<int, object> keyValues, string type)
        {
            // Initializes list for validation details
            var validationDetails = new List<ValidationDetail>();

            // Removes spaces from the expression
            expression = RemoveSpaces(expression);
            // Initializes list for OR operation results
            var orList = new List<bool>();
            // Splits expression by "OR" operator
            var orParts = expression.ToLower().Split(["or"], StringSplitOptions.None);
            // Processes each OR part
            foreach (var orPart in orParts)
            {
                // Splits OR part by "AND" operator
                var andParts = orPart.ToLower().Split(["and"], StringSplitOptions.None);
                // Initializes list for AND operation results
                var andList = new List<bool>();
                // Processes each AND part
                foreach (var andPart in andParts)
                {
                    // Handles boolean literals
                    if (andPart.Equals("true", StringComparison.OrdinalIgnoreCase) || andPart.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        // Adds boolean value to AND list
                        andList.Add(andPart.Equals("true", StringComparison.OrdinalIgnoreCase));
                        continue;
                    }

                    // Validates the entity in the AND part
                    var result = ValidateEntity(andPart, entities, keyValues, type);
                    // Adds validation result to AND list
                    andList.Add(result.IsValidationPassed);
                    // Adds validation details to the list
                    validationDetails.AddRange(result.ValidationDetails);
                }
                // Adds AND result (all must be true) to OR list
                orList.Add(andList.All(x => x));
            }

            // Returns result where any OR condition is true
            return new ValidationResult
            {
                IsValidationPassed = orList.Any(x => x),
                ValidationDetails = validationDetails
            };
        }

        /// <summary>
        /// Removes all whitespace characters from the expression.
        /// </summary>
        /// <param name="expression">The expression string.</param>
        /// <returns>The expression without whitespace.</returns>
        private static string RemoveSpaces(string expression)
        {
            // Removes all whitespace characters from the string
            return string.Concat(expression.Where(c => !char.IsWhiteSpace(c)));
        }

        /// <summary>
        /// Validates an entity based on its type.
        /// </summary>
        /// <param name="tenantIdStr">The entity ID string.</param>
        /// <param name="entities">A collection of entities.</param>
        /// <param name="keyValues">A dictionary of key-value pairs.</param>
        /// <param name="type">The type of entity.</param>
        /// <returns>A <see cref="ValidationResult"/>.</returns>
        private ValidationResult ValidateEntity(string tenantIdStr, IEnumerable<object> entities, Dictionary<int, object> keyValues, string type)
        {
            // Initializes list for validation details
            var validationDetails = new List<ValidationDetail>();
            // Initializes validation flag as false
            var isValidationPassed = false;

            // Switches based on entity type
            switch (type)
            {
                case "ECard":
                    // Casts entities to Ecard collection
                    var list = (IEnumerable<Ecard>)entities;
                    // Finds Ecard by ID
                    var eCard = list.FirstOrDefault(x => x.EcardId == int.Parse(tenantIdStr));
                    if (eCard != null)
                    {
                        // Validates the Ecard
                        var validateCardResult = ValidateCard(eCard, keyValues);
                        // Sets validation result
                        isValidationPassed = validateCardResult.IsValidationPassed;
                        // Adds validation details
                        validationDetails.AddRange(validateCardResult.ValidationDetails);
                    }
                    break;

                case "Card":
                    // Validates rule by ID
                    var cardResult = ValidateRule(int.Parse(tenantIdStr), keyValues);
                    // Sets validation result
                    isValidationPassed = cardResult.IsValidationPassed;
                    // Adds validation details
                    validationDetails.AddRange(cardResult.ValidationDetails);
                    break;

                case "Rule":
                    // Retrieves all conditions from repository
                    var conditions = _uow.ConditionRepository.GetAll();
                    // Processes conditions in descending order of value length
                    foreach (var condition in conditions.OrderByDescending(c => c.ConditionValue!.Length))
                    {
                        // Splits entity ID string by condition value
                        var facts = tenantIdStr.Split([condition.ConditionValue!.ToLower().Replace(" ", "")], StringSplitOptions.None);

                        // Checks if split resulted in two parts
                        if (facts.Length == 2)
                        {
                            // Casts entities to Factor collection
                            var factors = (IEnumerable<Factor>)entities;
                            // Finds factor by ID from second part
                            var factor = factors.FirstOrDefault(x => x.FactorId == int.Parse(facts[1]));
                            if (factor != null)
                            {
                                // Retrieves parameter associated with factor
                                var parameter = _uow.ParameterRepository.GetById((int)factor.ParameterId!);
                                // Retrieves data type of parameter
                                var datatype = _uow.DataTypeRepository.GetById((int)parameter.DataTypeId!);

                                // Validates the factor with condition and value
                                var detail = Validate(condition, factor, keyValues.FirstOrDefault(x => x.Key == factor.ParameterId).Value, datatype.DataTypeName!);
                                // Assigns parameter ID to validation detail
                                detail.ParameterId = factor.ParameterId;
                                // Assigns factor name to validation detail
                                detail.FactorName = factor.FactorName;
                                // Adds validation detail to list
                                validationDetails.Add(detail);
                                // Sets validation result
                                isValidationPassed = detail.IsValid;
                            }
                            break;
                        }
                    }
                    break;
            }

            // Returns validation result
            return new ValidationResult
            {
                IsValidationPassed = isValidationPassed,
                ValidationDetails = validationDetails
            };
        }

        /// <summary>
        /// Validates an ECard entity.
        /// </summary>
        /// <param name="card">The ECard entity.</param>
        /// <param name="keyValues">A dictionary of key-value pairs.</param>
        /// <returns>A <see cref="ValidationResult"/>.</returns>
        private ValidationResult ValidateCard(Ecard card, Dictionary<int, object> keyValues)
        {
            // Processes the card's expression for validation
            return ProcessExpression(card.Expression, [], keyValues, "Card");
        }

        /// <summary>
        /// Validates a Rule entity.
        /// </summary>
        /// <param name="ruleId">The rule ID.</param>
        /// <param name="keyValues">A dictionary of key-value pairs.</param>
        /// <returns>A <see cref="ValidationResult"/>.</returns>
        private ValidationResult ValidateRule(int ruleId, Dictionary<int, object> keyValues)
        {
            // Retrieves rule by ID from repository
            var rule = _uow.EruleRepository.GetById(ruleId);
            // Retrieves all factors with tracking
            var factors = _uow.FactorRepository.GetAll(true);
            // Processes the rule's expression for validation
            return ProcessExpression(rule.Expression, factors, keyValues, "Rule");
        }

        /// <summary>
        /// Validates a condition against a factor and value.
        /// </summary>
        /// <param name="condition">The condition to validate.</param>
        /// <param name="factor">The factor to validate against.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="datatype">The data type for validation.</param>
        /// <returns>A <see cref="ValidationDetail"/>.</returns>
        private static ValidationDetail Validate(Condition condition, Factor factor, object value, string? datatype = null)
        {
            // Creates a new validation detail instance
            var validationResult = new ValidationDetail
            {
                Condition = condition.ConditionValue,
                FactorValue = factor?.Value1,
                ProvidedValue = value?.ToString(),
                ParameterId = factor?.ParameterId,
                ErrorMessage = []
            };

            // Checks for null arguments and returns invalid result if found
            if (value == null || condition == null || factor == null)
            {
                validationResult.IsValid = false;
                validationResult?.ErrorMessage?.Add("Arguments cannot be null");
                return validationResult!;
            }

            // Switches based on condition value
            switch (condition.ConditionValue)
            {
                case "=":
                    // Validates equality (case-insensitive)
                    validationResult.IsValid = string.Equals(factor.Value1, validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase);
                    break;
                case "!=":
                    // Validates inequality (case-insensitive)
                    validationResult.IsValid = !string.Equals(factor.Value1, validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase);
                    break;
                case "<":
                    // Validates less than condition
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a < b, datatype ?? "");
                    break;
                case ">":
                    // Validates greater than condition
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a > b, datatype ?? "");
                    break;
                case "<=":
                    // Validates less than or equal condition
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a <= b, datatype ?? "");
                    break;
                case ">=":
                    // Validates greater than or equal condition
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a >= b, datatype ?? "");
                    break;
                case "Range":
                    // Validates range condition if second value exists
                    if (factor.Value2 != null)
                    {
                        if (datatype == "Date")
                        {
                            // Validates date range
                            if (DateTime.TryParse(factor.Value1, out var lowerDate) &&
                                DateTime.TryParse(factor.Value2, out var upperDate) &&
                                DateTime.TryParse(validationResult.ProvidedValue, out var providedDate))
                            {
                                validationResult.IsValid = lowerDate <= providedDate && providedDate <= upperDate;
                            }
                        }
                        else if (double.TryParse(factor.Value1, out var lowerBound) &&
                                 double.TryParse(factor.Value2, out var upperBound) &&
                                 double.TryParse(validationResult.ProvidedValue, out var provided))
                        {
                            // Validates numeric range
                            validationResult.IsValid = lowerBound <= provided && provided <= upperBound;
                        }
                    }
                    break;
                case "In List":
                    // Validates if value is in comma-separated list
                    var listValues = factor.Value1!.Split(',');
                    validationResult.IsValid = listValues.Any(item => string.Equals(item.Trim(), validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase));
                    break;
                case "Not In List":
                    // Validates if value is not in comma-separated list
                    var notInListValues = factor.Value1!.Split(',');
                    validationResult.IsValid = !notInListValues.Any(item => string.Equals(item.Trim(), validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase));
                    break;
                default:
                    // Handles invalid condition
                    validationResult.IsValid = false;
                    validationResult.ErrorMessage!.Add($"Invalid condition: {condition.ConditionValue}");
                    break;
            }

            // Adds default error message if validation failed without specific message
            if (!validationResult.IsValid && (validationResult.ErrorMessage == null || validationResult.ErrorMessage.Count == 0))
            {
                validationResult!.ErrorMessage!.Add($"Validation failed for condition: {condition.ConditionValue}");
            }

            return validationResult;
        }

        /// <summary>
        /// Compares two values based on the given comparison function.
        /// </summary>
        /// <param name="factorValue">The factor value.</param>
        /// <param name="providedValue">The provided value.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <param name="datatype">The data type.</param>
        /// <returns>True if comparison is valid, otherwise false.</returns>
        private static bool CompareValues(string factorValue, string providedValue, Func<double, double, bool> comparison, string datatype)
        {
            // Handles date comparison
            if (datatype == "Date")
            {
                if (DateTime.TryParse(factorValue, out var factorDate) &&
                    DateTime.TryParse(providedValue, out var providedDate))
                {
                    return comparison(providedDate.Ticks, factorDate.Ticks);
                }
                return false;
            }

            // Handles numeric comparison
            return double.TryParse(factorValue, out var factor) &&
                   double.TryParse(providedValue, out var provided) &&
                   comparison(provided, factor);
        }

        /// <summary>
        /// Gets the best fit products for a user based on provided key values.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>A collection of <see cref="BestFitProductModel"/> representing the best fit products.</returns>
        public async Task<IEnumerable<BestFitProductModel>> GetBestFitProductsAsync(int userId, Dictionary<int, object> keyValues)
        {
            // Initializes list for best fit products
            var resultList = new List<BestFitProductModel>();

            // Retrieves all active Pcards from repository
            var pCardIdDetails = await _uow.PcardRepository.Query().Where(w => w.Pstatus == "Active").ToListAsync();

            // Processes each active Pcard
            foreach (var pCardId in pCardIdDetails)
            {
                // Validates eligibility for the product
                ValidationResult result = await ValidAsync(userId, pCardId.ProductId , keyValues);

                // If validation passed, adds product to best fit list
                if (result.IsValidationPassed)
                {
                    // Retrieves product details
                    var productDetails = _uow.ProductRepository.GetById(pCardId.ProductId);
                    // Creates best fit product model
                    var bestFitProduct = new BestFitProductModel
                    {
                        ProductId = productDetails.ProductId,
                        ProductName = productDetails.ProductName,
                        PcardId = pCardId.PcardId,
                        PcardName = pCardId.PcardName,
                    };
                    // Adds to result list
                    resultList.Add(bestFitProduct);
                }
            }

            // Returns list of best fit products
            return resultList;
        }




        //public static string GenerateReferenceId()
        //{
        //    const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //    const string numbers = "0123456789";
        //    var random = new Random();

        //    // Generate 3 random letters
        //    string letterPart = new string(Enumerable.Repeat(letters, 3)
        //        .Select(s => s[random.Next(s.Length)]).ToArray());

        //    // Generate 3 random digits
        //    string numberPart = new string(Enumerable.Repeat(numbers, 3)
        //        .Select(s => s[random.Next(s.Length)]).ToArray());

        //    return letterPart + numberPart; // Example: XRT572
        //}
    }

}
