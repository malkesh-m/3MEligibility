using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Text.RegularExpressions;
using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for validating eligibility criteria.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ValidatorService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="historyPcService">The history PC service instance.</param>
    public partial class ValidatorService(IUnitOfWork uow, IHistoryPcService historyPcService) : IValidatorService
    {
        // The unit of work instance for database operations.
        private readonly IUnitOfWork _uow = uow;

        // The history PC service instance for logging validation history.
        private readonly IHistoryPcService _historyPcService = historyPcService;

        /// <summary>
        /// Validates a PCard asynchronously.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="pCardId">The PCard ID.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>A task representing the asynchronous operation, with a ValidationResult.</returns>
        public async Task<ValidationResult> ValidAsync(int userId, int pCardId, Dictionary<int, object> keyValues)
        {
            // Queries the database for the specified PCard.
            var pCard = await _uow.PcardRepository.Query()
                                    // Filters by PCard ID.
                                    .Where(p => p.PcardId == pCardId)
                                    // Orders by PCard ID.
                                    .OrderBy(p => p.PcardId)
                                    // Gets the first matching PCard or null.
                                    .FirstOrDefaultAsync() ?? throw new Exception("Pcard not found for the given product.");

            // Retrieves all ECards from repository.
            var eCards = _uow.EcardRepository.GetAll();

            // Processes the PCard expression asynchronously.
            var result = await Task.Run(() => ProcessExpression(pCard.Expression, eCards, keyValues, "ECard"));

            // Removes duplicate validation details by grouping and selecting first of each group.
            result.ValidationDetails = [.. result.ValidationDetails
                // Groups by multiple properties to identify duplicates.
                .GroupBy(detail => new { detail.IsValid, detail.FactorName, detail.Condition, detail.FactorValue, detail.ProvidedValue, detail.ErrorMessage, detail.ParameterId })
                // Selects the first item from each group.
                .Select(group => group.First())];

            // Creates a new history model for logging.
            HistoryPcModel model = new()
            {
                // Sets PCard ID.
                PcardId = pCard.PcardId,
                // Sets the expression that was validated.
                Expression = pCard.Expression,
                // Sets the product ID.
                ProductId = pCard.ProductId,
                // Sets the entity ID.
                TenantId = pCard.TenantId,
                // Sets the user ID who performed validation.
                UserId = userId,
                // Generates a unique transaction reference (first 8 chars of GUID).
                TransReference = Guid.NewGuid().ToString()[..8],
                // Sets current date/time as transaction date.
                TransactionDate = DateTime.Now,
                // Sets the validation result.
                Result = result.IsValidationPassed,
                // Sets the update timestamp.
                UpdatedByDateTime = DateTime.Now
            };

            // Adds the history record to the database.
            await _historyPcService.Add(model);

            // Returns the validation result.
            return new ValidationResult
            {
                // Sets overall validation result.
                IsValidationPassed = result.IsValidationPassed,
                // Sets the validation details.
                ValidationDetails = result.ValidationDetails
            };
        }

        /// <summary>
        /// Validates a form PCard expression asynchronously.
        /// </summary>
        /// <param name="expression">The expression to validate.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>A task representing the asynchronous operation, with a ValidationResult.</returns>
        public async Task<ValidationResult> ValidateFormPCard(string expression, Dictionary<int, object> keyValues)
        {
            // Retrieves all ECards from repository.
            var eCards = _uow.EcardRepository.GetAll();

            // Processes the expression asynchronously.
            var result = await Task.Run(() => ProcessExpression(expression, eCards, keyValues, "ECard"));

            // Removes duplicate validation details.
            result.ValidationDetails = [.. result.ValidationDetails
                // Groups by multiple properties.
                .GroupBy(detail => new { detail.IsValid, detail.FactorName, detail.Condition, detail.FactorValue, detail.ProvidedValue, detail.ErrorMessage, detail.ParameterId })
                // Selects first from each group.
                .Select(group => group.First())];

            // Returns the validation result.
            return new ValidationResult
            {
                // Sets overall validation result.
                IsValidationPassed = result.IsValidationPassed,
                // Sets validation details.
                ValidationDetails = result.ValidationDetails
            };
        }

        /// <summary>
        /// Processes a validation expression recursively.
        /// </summary>
        /// <param name="expression">The expression to process.</param>
        /// <param name="entities">The entities to use in validation.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <param name="type">The type of validation.</param>
        /// <returns>The ValidationResult of the processed expression.</returns>
        private ValidationResult ProcessExpression(string expression, IEnumerable<object> entities, Dictionary<int, object> keyValues, string type)
        {
            // Initializes list for validation details.
            var validationDetails = new List<ValidationDetail>();

            // Removes spaces from the expression.
            expression = RemoveSpaces(expression);

            // Processes all nested expressions (within parentheses) first.
            while (expression.Contains('('))
            {
                // Gets the innermost expression within parentheses.
                var innerMostExpression = GetInnerMostExpression(expression);
                // Recursively processes the inner expression.
                var innerResult = ProcessExpression(innerMostExpression, entities, keyValues, type);

                // Replaces the inner expression with its boolean result.
                expression = expression.Replace($"({innerMostExpression})", innerResult.IsValidationPassed ? "true" : "false");
                // Adds inner validation details to the main list.
                validationDetails.AddRange(innerResult.ValidationDetails);
            }

            // Evaluates the final processed expression.
            var finalResult = EvaluateExpression(expression, entities, keyValues, type);
            // Adds final validation details to the main list.
            validationDetails.AddRange(finalResult.ValidationDetails);

            // Returns the combined validation result.
            return new ValidationResult
            {
                // Sets overall validation result.
                IsValidationPassed = finalResult.IsValidationPassed,
                // Sets all validation details.
                ValidationDetails = validationDetails
            };
        }

        /// <summary>
        /// Gets the innermost expression within parentheses.
        /// </summary>
        /// <param name="expression">The expression to search.</param>
        /// <returns>The innermost expression as a string.</returns>
        private static string GetInnerMostExpression(string expression)
        {
            // Finds the last opening parenthesis.
            var start = expression.LastIndexOf('(');
            // Finds the corresponding closing parenthesis.
            var end = expression.IndexOf(')', start);
            // Extracts the substring between parentheses.
            return expression.Substring(start + 1, end - start - 1);
        }

        /// <summary>
        /// Evaluates a validation expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="entities">The entities to use in validation.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <param name="type">The type of validation.</param>
        /// <returns>The ValidationResult of the evaluated expression.</returns>
        private ValidationResult EvaluateExpression(string expression, IEnumerable<object> entities,
       Dictionary<int, object> keyValues, string type)
        {
            var validationDetails = new List<ValidationDetail>();

            expression = RemoveSpaces(expression);

            var orList = new List<bool>();
            var orParts = expression.ToLower().Split(["or"], StringSplitOptions.None);

            foreach (var orPart in orParts)
            {
                var andList = new List<bool>();
                var andParts = orPart.ToLower().Split(["and"], StringSplitOptions.None);

                foreach (var rawPart in andParts)
                {
                    var part = rawPart.Trim();

                    // Case 1: Literal boolean
                    if (part.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        part.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        andList.Add(part.Equals("true", StringComparison.OrdinalIgnoreCase));
                        continue;
                    }

                    // Case 2: Literal boolean/number comparison
                    var literalRegex = @"^(true|false|\d+\.?\d*)\s*(=|!=|<|>|<=|>=)\s*(true|false|\d+\.?\d*)$";
                    if (Regex.IsMatch(part, literalRegex, RegexOptions.IgnoreCase))
                    {
                        var match = Regex.Match(part, literalRegex, RegexOptions.IgnoreCase);
                        var left = match.Groups[1].Value;
                        var op = match.Groups[2].Value;
                        var right = match.Groups[3].Value;

                        andList.Add(EvaluateLiteral(left, op, right));
                        continue;
                    }

                    // Case 3: Factor/entity-based expression
                    var entityResult = ValidateEntity(part, entities, keyValues, type);
                    andList.Add(entityResult.IsValidationPassed);
                    validationDetails.AddRange(entityResult.ValidationDetails);
                }

                // AND evaluation
                orList.Add(andList.All(x => x));
            }

            return new ValidationResult
            {
                IsValidationPassed = orList.Any(x => x),   // OR evaluation
                ValidationDetails = validationDetails
            };
        }

        private static bool EvaluateLiteral(string left, string op, string right)
        {
            // boolean comparison
            if (bool.TryParse(left, out var lb) && bool.TryParse(right, out var rb))
            {
                return op switch
                {
                    "=" => lb == rb,
                    "!=" => lb != rb,
                    _ => false
                };
            }

            // numeric comparison
            if (double.TryParse(left, out var ln) && double.TryParse(right, out var rn))
            {
                return op switch
                {
                    "=" => ln == rn,
                    "!=" => ln != rn,
                    "<" => ln < rn,
                    ">" => ln > rn,
                    "<=" => ln <= rn,
                    ">=" => ln >= rn,
                    _ => false
                };
            }

            return false;
        }

        /// <summary>
        /// Removes all whitespace characters from an expression.
        /// </summary>
        /// <param name="expression">The expression to process.</param>
        /// <returns>The expression without whitespace.</returns>
        private static string RemoveSpaces(string expression)
        {
            // Removes all whitespace characters using LINQ.
            return string.Concat(expression.Where(c => !char.IsWhiteSpace(c)));
        }

        /// <summary>
        /// Validates an entity based on the provided type and key values.
        /// </summary>
        /// <param name="entityIdStr">The entity ID as a string.</param>
        /// <param name="entities">The entities to use in validation.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <param name="type">The type of validation.</param>
        /// <returns>The ValidationResult of the entity validation.</returns>
        private ValidationResult ValidateEntity(string entityIdStr, IEnumerable<object> entities, Dictionary<int, object> keyValues, string type)
        {
            // Initializes list for validation details.
            var validationDetails = new List<ValidationDetail>();
            // Initializes validation result flag.
            var isValidationPassed = false;
            // Switches based on validation type.
            switch (type)
            {
                case "ECard":
                    // Casts entities to Ecard list.
                    var list = (IEnumerable<Ecard>)entities;
                    // Finds the ECard by ID.
                    var eCard = list.FirstOrDefault(x => x.EcardId == int.Parse(entityIdStr));
                    // If ECard found, validates it.
                    if (eCard != null)
                    {
                        // Validates the ECard.
                        var validateCardResult = ValidateCard(eCard, keyValues);
                        // Sets validation result.
                        isValidationPassed = validateCardResult.IsValidationPassed;
                        // Adds validation details.
                        validationDetails.AddRange(validateCardResult.ValidationDetails);
                    }
                    // Breaks out of switch.
                    break;

                case "Card":
                    // Validates rule directly by ID.
                    var cardResult = ValidateRule(int.Parse(entityIdStr), keyValues);
                    // Sets validation result.
                    isValidationPassed = cardResult.IsValidationPassed;
                    // Adds validation details.
                    validationDetails.AddRange(cardResult.ValidationDetails);
                    // Breaks out of switch.
                    break;

                case "Rule":
                    // Gets all conditions from repository.
                    var conditions = _uow.ConditionRepository.GetAll();
                    // Processes conditions in descending order of value length.
                    foreach (var condition in conditions.OrderByDescending(c => c.ConditionValue?.Length))
                    {
                        // Splits entity string by condition value (spaces removed).
                        var conditionValue = (condition.ConditionValue ?? "").Replace(" ", "").ToLower();
                        var entityStr = entityIdStr.Replace(" ", "").ToLower();

                        // Split entity string by condition value
                        var facts = entityStr.Split([conditionValue], StringSplitOptions.None);
                        // Checks if split produced exactly 2 parts.
                        if (facts.Length == 2)
                        {
                            // Casts entities to Factor list.
                            var factors = (IEnumerable<Factor>)entities;

                            //var factor = factors.FirstOrDefault(x => x.Value1 == (facts[1]));

                            // Finds factor that matches the second part and has a corresponding key value.
                            var factor = factors.FirstOrDefault(x =>
    string.Equals(x.Value1?.Replace(" ", "").ToString(), facts[1]?.ToString(), StringComparison.OrdinalIgnoreCase) && keyValues.Any(y => y.Key == x.ParameterId)
);
                            // If factor found, validates it.
                            if (factor != null)
                            {
                                // Gets parameter associated with the factor.
                                var parameter = _uow.ParameterRepository.GetById((int)factor.ParameterId!);
                                // Gets datatype of the parameter.
                                var datatype = _uow.DataTypeRepository.GetById((int)parameter.DataTypeId!);
                                // Validates the condition, factor, and value.
                                var detail = Validate(condition, factor, keyValues.FirstOrDefault(x => x.Key == factor.ParameterId).Value, datatype.DataTypeName!);
                                // Assigns parameter ID to validation detail.
                                detail!.ParameterId = factor.ParameterId;  // Assign the ParameterId to the validation detail
                                // Assigns factor name to validation detail.
                                detail.FactorName = factor.FactorName;
                                // Adds validation detail to list.
                                validationDetails.Add(detail);
                                // Sets validation result.
                                isValidationPassed = detail.IsValid;
                            }
                            // Breaks out of foreach loop.
                            break;
                        }
                    }
                    // Breaks out of switch.
                    break;
            }

            // Returns the validation result.
            return new ValidationResult
            {
                // Sets overall validation result.
                IsValidationPassed = isValidationPassed,
                // Sets validation details.
                ValidationDetails = validationDetails
            };
        }

        /// <summary>
        /// Validates a card using its expression and key values.
        /// </summary>
        /// <param name="card">The Ecard to validate.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>The ValidationResult of the card validation.</returns>
        public ValidationResult ValidateCard(Ecard card, Dictionary<int, object> keyValues)
        {
            // Processes the card expression with empty entities list.
            return ProcessExpression(card.Expression, [], keyValues, "Card");
        }

        /// <summary>
        /// Validates a card asynchronously by ECard ID.
        /// </summary>
        /// <param name="eCardId">The ECard ID.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>A task representing the asynchronous operation, with a ValidationResult.</returns>
        public async Task<ValidationResult> ValidateCard(int eCardId, Dictionary<int, object> keyValues)
        {
            // Queries the database for the specified ECard.
            var eCard = await _uow.EcardRepository.Query()
                                    // Filters by ECard ID.
                                    .Where(p => p.EcardId == eCardId)
                                    // Orders by ECard ID.
                                    .OrderBy(p => p.EcardId)
                                    // Gets first matching ECard or null.
                                    .FirstOrDefaultAsync() ?? throw new Exception("Ecard not found for the given ECardId.");

            // Retrieves all ECards from repository.
            var eCards = _uow.EcardRepository.GetAll();

            // Processes the ECard expression.
            return ProcessExpression(eCard.Expression, eCards, keyValues, "Card");
        }

        /// <summary>
        /// Validates a form ECard expression asynchronously.
        /// </summary>
        /// <param name="expression">The expression to validate.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>A task representing the asynchronous operation, with a ValidationResult.</returns>
        public ValidationResult ValidateFormECard(string expression, Dictionary<int, object> keyValues)
        {
            // Retrieves all ECards from repository.
            var eCards = _uow.EcardRepository.GetAll();

            // Extracts all numbers (digits) from the expression using regex.
            var numberMatches = MyRegex().Matches(expression)
                                     // Casts matches to enumerable.
                                     .Cast<Match>()
                                     // Wraps each number in parentheses.
                                     .Select(m => $"({m.Value})")
                                     // Converts to list.
                                     .ToList();

            // Initializes validation result with default values.
            var Results = new Domain.Models.ValidationResult
            {
                // Sets default validation result to true.
                IsValidationPassed = true,
                // Initializes empty validation details list.
                ValidationDetails = [] // Make sure it's initialized
            };

            // Processes each extracted number expression.
            foreach (var exp in numberMatches)
            {
                // Processes the expression.
                var result = ProcessExpression(exp, eCards, keyValues, "Card");
                // If any expression fails, sets overall result to false.
                if (!result.IsValidationPassed)
                {
                    // Sets validation failed.
                    Results.IsValidationPassed = false;
                    // Sets error message from result.
                    Results.ErrorMessage = result.ErrorMessage;
                    // Sets validation details from result.
                    Results.ValidationDetails = result.ValidationDetails;
                    // Sets eligibility percentage from result.
                    Results.EligibilityPercentage = result.EligibilityPercentage;
                    // Returns early with failure result.
                    return Results;
                }

                // If validation details exist, adds them to results.
                if (result.ValidationDetails != null && result.ValidationDetails.Count != 0)
                {
                    // Adds all validation details to results.
                    Results.ValidationDetails.AddRange(result.ValidationDetails);
                }
            }

            // Returns successful validation result.
            return Results;
        }

        /// <summary>
        /// Validates a rule by rule ID and key values.
        /// </summary>
        /// <param name="ruleId">The rule ID.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>The ValidationResult of the rule validation.</returns>
        public ValidationResult ValidateRule(int ruleId, Dictionary<int, object> keyValues)
        {
            // Gets the rule by ID.
            var rule = _uow.EruleRepository.GetById(ruleId);
            // Gets all factors from repository.
            var factors = _uow.FactorRepository.GetAll();
            // Processes the rule expression.
            return ProcessExpression(rule.ExpShown ?? "", factors, keyValues, "Rule");
        }

        /// <summary>
        /// Validates a form Erule expression and key values.
        /// </summary>
        /// <param name="expression">The expression to validate.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>The ValidationResult of the form Erule validation.</returns>
        public ValidationResult ValidateFormErule(string expression, Dictionary<int, object> keyValues)
        {
            // Gets all factors from repository.
            var factors = _uow.FactorRepository.GetAll();
            // Processes the expression with factors.
            return ProcessExpression(expression, factors, keyValues, "Rule");
        }

        /// <summary>
        /// Validates a condition, factor, and value with an optional datatype.
        /// </summary>
        /// <param name="condition">The condition to validate.</param>
        /// <param name="factor">The factor to validate.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="datatype">The datatype for comparison (optional).</param>
        /// <returns>The ValidationDetail of the validation.</returns>
        private ValidationDetail? Validate(Condition condition, Factor factor, object value, string? datatype = null)
        {
            // Creates new validation detail with basic information.
            var validationResult = new ValidationDetail
            {
                // Sets condition value.
                Condition = condition.ConditionValue,
                // Sets factor value.
                FactorValue = factor?.Value1,
                // Sets provided value.
                ProvidedValue = value?.ToString(),
                // Sets parameter ID.
                ParameterId = factor?.ParameterId  // Include ParameterId in the result
            };

            // Checks for null arguments.
            if (value == null || condition == null || factor == null)
            {
                // Sets validation failed.
                validationResult.IsValid = false;
                // Adds error message for null arguments.
                validationResult.ErrorMessage!.Add("Arguments cannot be null");
                // Returns failed validation.
                return validationResult;
            }
            if (factor.Value1 != null && factor.Value1.Trim().Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                validationResult.IsValid = true;
                return validationResult;
            }

            // Switches based on condition type.
            switch (condition.ConditionValue)
            {
                case "=":
                    // Checks for equality (case insensitive).
                    validationResult.IsValid = string.Equals(factor.Value1, validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase);
                    // Breaks out of case.
                    break;
                case "!=":
                    // Checks for inequality (case insensitive).
                    validationResult.IsValid = !string.Equals(factor.Value1, validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase);
                    // Breaks out of case.
                    break;
                case "<":
                    // Checks if provided value is less than factor value.
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a < b, datatype ?? "");
                    // Breaks out of case.
                    break;
                case ">":
                    // Checks if provided value is greater than factor value.
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a > b, datatype ?? "");
                    // Breaks out of case.
                    break;
                case "<=":
                    // Checks if provided value is less than or equal to factor value.
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a <= b, datatype ?? "");
                    // Breaks out of case.
                    break;
                case ">=":
                    // Checks if provided value is greater than or equal to factor value.
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a >= b, datatype ?? "");
                    // Breaks out of case.
                    break;
                case "Range":
                    // Checks if value is within range (requires Value2).
                    if (factor.Value2 != null)
                    {
                        // Handles date range validation.
                        if (datatype == "Date")
                        {
                            // Parses dates and checks range.
                            if (DateTime.TryParse(factor.Value1, out var lowerDate) &&
                                DateTime.TryParse(factor.Value2, out var upperDate) &&
                                DateTime.TryParse(validationResult.ProvidedValue, out var providedDate))
                            {
                                // Checks if provided date is within range.
                                validationResult.IsValid = lowerDate <= providedDate && providedDate <= upperDate;
                            }
                        }
                        // Handles numeric range validation.
                        else if (double.TryParse(factor.Value1, out var lowerBound) &&
                                 double.TryParse(factor.Value2, out var upperBound) &&
                                 double.TryParse(validationResult.ProvidedValue, out var provided))
                        {
                            // Checks if provided value is within numeric range.
                            validationResult.IsValid = lowerBound <= provided && provided <= upperBound;
                        }
                    }
                    // Breaks out of case.
                    break;
                case "In List":
                    // Splits factor value by commas for list validation.
                    var listName = factor.Value1;
                    var listId = _uow.ManagedListRepository.Query().Where(x => x.ListName == listName).FirstOrDefault()!.ListId;

                    var listValue = _uow.ListItemRepository.GetAll().Where(x => x.ListId == listId).Select(x => x.ItemName).ToList();

                    // Checks if provided value is in the list (case insensitive).
                    var valid = listValue.Any(item => string.Equals(item!.Trim(), validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase));
                    validationResult.IsValid = true;
                    // Breaks out of case.
                    break;
                case "Not In List":
                    // Splits factor value by commas for list validation.
                    var notInListValues = factor.Value1;
                    // Checks if provided value is NOT in the list (case insensitive).
                    var notInListId = _uow.ManagedListRepository.Query().Where(x => x.ListName == notInListValues).FirstOrDefault()!.ListId;

                    var notInListValue = _uow.ListItemRepository.GetAll().Where(x => x.ListId == notInListId).Select(x => x.ItemName).ToList();

                    validationResult.IsValid = !notInListValue.Any(item => string.Equals(item!.Trim(), validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase));
                    // Breaks out of case.
                    break;
                default:
                    // Handles invalid condition values.
                    validationResult.IsValid = false;
                    // Adds error message for invalid condition.
                    validationResult.ErrorMessage!.Add($"Invalid condition: {condition.ConditionValue}");
                    // Breaks out of case.
                    break;
            }

            // Adds error message if validation failed and no message exists.
            if (validationResult != null && !validationResult.IsValid && condition != null)
            {
                // Initializes error message list if null.
                validationResult.ErrorMessage ??= [];

                // Adds default error message if none exists.
                if (validationResult.ErrorMessage.Count == 0)
                {
                    validationResult.ErrorMessage.Add($"Validation failed for condition: {condition?.ConditionValue}");
                }
            }

            // Returns the validation detail.
            return validationResult;
        }

        /// <summary>
        /// Compares two values using the provided comparison function and datatype.
        /// </summary>
        /// <param name="factorValue">The factor value.</param>
        /// <param name="providedValue">The provided value.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <param name="datatype">The datatype for comparison.</param>
        /// <returns>True if the comparison is valid; otherwise, false.</returns>
        private static bool CompareValues(string factorValue, string providedValue, Func<double, double, bool> comparison, string datatype)
        {
            // Handles date comparisons.
            if (datatype == "Date")
            {
                // Parses dates and compares using ticks.
                if (DateTime.TryParse(factorValue, out var factorDate) &&
                    DateTime.TryParse(providedValue, out var providedDate))
                {
                    // Uses the comparison function on date ticks.
                    return comparison(providedDate.Ticks, factorDate.Ticks);
                }
                // Returns false if date parsing fails.
                return false; // Invalid date format
            }

            // Handles numeric comparisons.
            return double.TryParse(factorValue, out var factor) &&
                   double.TryParse(providedValue, out var provided) &&
                   // Uses the comparison function on numeric values.
                   comparison(provided, factor);
        }

        [GeneratedRegex(@"\d+")]
        private static partial Regex MyRegex();
    }
}
