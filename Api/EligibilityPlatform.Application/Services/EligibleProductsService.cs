using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CsvHelper.Configuration;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharpYaml.Tokens;
using Parameter = MEligibilityPlatform.Domain.Entities.Parameter;
using RuleResult = MEligibilityPlatform.Domain.Models.RuleResult;
using ValidationResult = MEligibilityPlatform.Domain.Models.ValidationResult;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for determining eligible products based on business rules, exceptions, and scoring criteria.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EligibleProductsService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance for database operations.</param>
    /// <param name="evaluationHistoryService">The evaluation history service instance for tracking evaluations.</param>
    public partial class EligibleProductsService(IUnitOfWork uow, ILogger<EligibleProductsService> logger) : IEligibleProductsService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<EligibleProductsService> _logger = logger;

        /// <summary>
        /// Retrieves all eligible products for a given entity based on provided key-value parameters.
        /// </summary>
        /// <param name="tenantId">The ID of the entity for which to retrieve eligible products.</param>
        /// <param name="keyValues">A dictionary of parameter IDs and their corresponding values used for eligibility evaluation.</param>
        /// <returns>An <see cref="EligibleAmountResults"/> object containing eligible products with their amounts and validation details.</returns>
        /// <remarks>
        /// This method performs a comprehensive evaluation process including:
        /// - Retrieving all products for the entity
        /// - Processing business rules and card validations
        /// - Handling product exceptions
        /// - Calculating eligible amounts based on caps and scoring
        /// - Determining probability of default
        /// </remarks>
        /// 

        public EligibleAmountResults GetAllEligibleProducts(int tenantId, Dictionary<int, object> keyValues)
        {
            // Starts a stopwatch to measure execution time
            var stopwatch = Stopwatch.StartNew();
            // Creates a new EligibleAmountResult instance
            var result = new EligibleAmountResult();

            // Retrieves all products for the specified entity from the repository
            var allProducts = _uow.ProductRepository.Query()
                 .Where(p => p.TenantId == tenantId).Select(p => new Product
                 {
                     ProductId = p.ProductId,
                     ProductName = p.ProductName,
                     Description = p.Description,
                     MaxEligibleAmount = p.MaxEligibleAmount,
                     Code = p.Code,
                 })
                 .ToList();

            // Extracts all product IDs from the retrieved products
            var allProductIds = allProducts.Select(p => p.ProductId).ToList();

            // Retrieves all product caps for the specified entity from the repository
            var productCap = _uow.ProductCapRepository.Query()
                .Where(p => p.Product.TenantId == tenantId)
                .ToList();

            // Retrieves exception products with their related data for the specified entity
            var exceptionProducts = _uow.ExceptionProductRepository.Query()
           .Include(e => e.ExceptionManagement).AsNoTracking()
           .Select(e => new ExceptionProduct
           {
               ExceptionProductId = e.ExceptionProductId,
               ExceptionManagementId = e.ExceptionManagementId,
               ProductId = e.ProductId,
               ExceptionManagement = e.ExceptionManagement,
               Product = new Product
               {
                   ProductId = e.Product.ProductId,
                   ProductName = e.Product.ProductName,
                   CategoryId = e.Product.CategoryId,
                   TenantId = e.Product.TenantId,
                   Code = e.Product.Code,
                   Narrative = e.Product.Narrative,
                   Description = e.Product.Description,
                   MimeType = e.Product.MimeType,
                   UpdatedByDateTime = e.Product.UpdatedByDateTime,
                   CreatedBy = e.Product.CreatedBy,
                   CreatedByDateTime = e.Product.CreatedByDateTime,
                   UpdatedBy = e.Product.UpdatedBy,
                   IsImport = e.Product.IsImport,
                   MaxEligibleAmount = e.Product.MaxEligibleAmount,
                   Category = e.Product.Category,
                   //Entity = e.Product.Entity,
                   ExceptionProducts = e.Product.ExceptionProducts,
                   HistoryPcs = e.Product.HistoryPcs,
                   Pcard = e.Product.Pcard,
                   ProductCaps = e.Product.ProductCaps,
                   ProductParams = e.Product.ProductParams,
                   ProductCapAmounts = e.Product.ProductCapAmounts
               }
           })
           .ToList();

            // Processes business rules and card validations to determine valid products
            var (validProductIds, ruleResults) = ProcessRulesAndCards(tenantId, keyValues);

            // Retrieves products that don't have associated PCARD configurations
            var nonPCardEligibilityResults = GetNonPCardProducts(allProducts, tenantId);

            // Identifies products that failed PCARD validation
            var pcardFailProducts = GetFailedPCardProducts(
                allProducts,
                validProductIds,
                ruleResults,
                tenantId);
            var exceptionRules = _uow.ExceptionManagementRepository.Query();
            // Checks for products with exception handling
            var exceptionHandledProducts = CheckProductWithException(tenantId, keyValues, exceptionRules);

            // Processes eligible and non-eligible products based on validation results
            var (eligibleProducts, nonEligibleProducts) = ProcessEligibleProducts(
                tenantId,
                allProducts,
                productCap,
                validProductIds,
                (List<ProductEligibilityResult>)exceptionHandledProducts,
                nonPCardEligibilityResults,
                pcardFailProducts,
                ruleResults,
                keyValues);

            // Combines all product results into a single list
            var allProductsList = eligibleProducts
                .Concat(nonEligibleProducts)
                .Concat(nonPCardEligibilityResults)
                .Concat(pcardFailProducts)
                .ToList();

            // Creates a dictionary to store product eligibility results by product ID
            var productDict = new Dictionary<int, ProductEligibilityResult>();

            // Processes exception-handled products to update their eligibility information
            foreach (var exProd in exceptionHandledProducts)
            {
                // Finds matching product in the combined list
                var match = allProductsList.FirstOrDefault(p => p.ProductId == exProd.ProductId);
                if (match != null)
                {
                    // Updates exception product with eligibility details from the match
                    exProd.EligibleAmount = match.EligibleAmount;
                    exProd.MaxEligibleAmount = match.MaxEligibleAmount;
                    exProd.ErrorMessage = match.ErrorMessage;
                    exProd.IsEligible = match.IsEligible;
                    exProd.EligibilityPercent = match.EligibilityPercent;
                }

                // Adds or updates the product in the dictionary
                productDict[exProd.ProductId] = exProd;

                // Generates a random probability of default for the product
                Random probability = new();
                exProd.ProbabilityOfDefault = probability.Next(1, 101);
            }

            // Processes all other products that weren't handled by exceptions
            foreach (var prod in allProductsList)
            {
                // Adds product to dictionary if not already present
                if (!productDict.ContainsKey(prod.ProductId))
                    productDict[prod.ProductId] = prod;

                // Generates a random probability of default for the product
                Random probability = new();
                prod.ProbabilityOfDefault = probability.Next(1, 101);
            }

            // Assigns the processed products to the result
            result.Products = [.. productDict.Values];

            // Stops the stopwatch and outputs the elapsed time
            stopwatch.Stop();
            Console.WriteLine(stopwatch);
            Console.WriteLine("stopwatch");

            // Returns the final eligibility results
            return new EligibleAmountResults
            {
                Score = result.Score,
                Products = [.. result.Products.Select(p => new ProductEligibilityResults
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductCapAmount = p.MaxEligibleAmount,
                    EligibleAmount = p.EligibleAmount,
                    ProbabilityOfDefault = p.ProbabilityOfDefault,
                    MaximumProductCapPercentage = p.EligibilityPercent,
                    Message = p.ErrorMessage,
                    Iseligible = p.IsEligible,
                    ProductCode=p.ProductCode

                })]
            };
        }

        /// <summary>
        /// Processes business rules and card validations to determine which products are valid based on the provided parameters.
        /// </summary>
        /// <param name="tenantId">The ID of the entity for which to process rules.</param>
        /// <param name="keyValues">A dictionary of parameter IDs and their corresponding values.</param>
        /// <returns>A tuple containing valid product IDs and rule validation results.</returns>
        private (List<int?> validProductIds, List<RuleResult> ruleResults) ProcessRulesAndCards(int tenantId, Dictionary<int, object> keyValues)
        {
            // Retrieves rules that match the provided key values
            var matchedRules = GetMatchRules(tenantId, keyValues);
            // Validates the matched rules against the provided values
            var ruleResults = ValidateERules(tenantId, matchedRules, keyValues);
            // Extracts rule IDs from the validation results
            var ruleIds = ruleResults.Select(r => r.RuleID).ToList();

            // Retrieves ECARDs associated with the validated rules
            var eCards = GetEcardByEruleId(tenantId, ruleIds);
            // Validates the ECARDs against the rule results
            var eCardResults = ValidateEcards(eCards, ruleResults);
            // Extracts ECARD IDs from the validation results
            var eCardIds = eCardResults.Select(r => r.EcardID).ToList();

            // Retrieves PCARD IDs associated with the validated ECARDs
            var pCardIds = GetPcardIdByEcards(tenantId, eCardIds);
            // Validates the PCARDs against the ECARD results
            var pCardResults = ValidatePcards(pCardIds, eCardResults);
            // Extracts valid PCARD IDs from the validation results
            var validPCardIds = pCardResults.Where(p => p.Result).Select(p => p.PcardID).ToList();

            // Retrieves valid PCARD entities from the repository
            var validPCards = _uow.PcardRepository.GetByIds(validPCardIds);
            // Extracts distinct product IDs from the valid PCARDs
            var validProductIds = validPCards.Select(p => p.ProductId).Distinct().ToList();

            // Returns both valid product IDs and rule validation results
            return (validProductIds, ruleResults);
        }

        /// <summary>
        /// Retrieves products that do not have associated PCARD configurations.
        /// </summary>
        /// <param name="allProducts">The complete list of products to filter.</param>
        /// <param name="tenantId">The ID of the entity for which to retrieve products.</param>
        /// <returns>A list of <see cref="ProductEligibilityResult"/> objects for products without PCARDs.</returns>
        private List<ProductEligibilityResult> GetNonPCardProducts(List<Product> allProducts, int tenantId)
        {
            // Retrieves all product IDs that have associated PCARD configurations
            var productsWithPcard = _uow.PcardRepository.Query()
                .Where(pc => pc.Product!.TenantId == tenantId)
                .Select(pc => pc.ProductId)
                .Distinct()
                .ToList();

            // Filters products without PCARDs and creates eligibility results for them
            return [.. allProducts
                .Where(p => !productsWithPcard.Contains(p.ProductId))
                .Select(p => new ProductEligibilityResult
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    IsEligible = false,
                    EligibleAmount = 0,
                    IsProcessedByException = false,
                    ErrorMessage = "Product does not have any Product CARD.",
                    ProductCode= p.Code!
                })];
        }

        /// <summary>
        /// Identifies products that failed PCARD validation based on rule evaluation results.
        /// </summary>
        /// <param name="allProducts">The complete list of products to evaluate.</param>
        /// <param name="validProductIds">Product IDs that passed validation.</param>
        /// <param name="ruleResults">Results from rule validation.</param>
        /// <param name="tenantId">The ID of the entity for which to evaluate products.</param>
        /// <returns>A list of <see cref="ProductEligibilityResult"/> objects for products that failed PCARD validation.</returns>
        private List<ProductEligibilityResult> GetFailedPCardProducts(
            List<Product> allProducts,
            List<int?> validProductIds,
            List<RuleResult> ruleResults,
            int tenantId)
        {
            // Retrieves all product IDs that have PCARD configurations for the entity
            var productIdsWithPcard = _uow.PcardRepository.Query()
                .Where(p => p.Product != null && p.Product.TenantId == tenantId)
                .Select(p => p.ProductId)
                .Distinct()
                .ToList();

            // Filters and converts valid product IDs to a list of integers
            var validIds = (validProductIds ?? [])
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            // Converts PCARD product IDs to a list of integers
            var pcardIds = productIdsWithPcard
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            // Identifies product IDs that have PCARDs but failed validation
            var failedPcardProductIds = pcardIds
                .Except(validIds)
                .ToList();

            // Retrieves all PCARDs for the entity
            var allPcards = _uow.PcardRepository.Query()
                .Where(p => p.ProductId != null && p.Product!.TenantId == tenantId)
                .ToList();

            // Retrieves all ECARDs for the entity
            var allEcards = _uow.EcardRepository.Query()
                .Where(e => e.TenantId == tenantId)
                .ToList();

            // Creates eligibility results for failed products with appropriate error messages
            var failedProducts = failedPcardProductIds
                .Select(id => allProducts.FirstOrDefault(p => p.ProductId == id))
                .Where(p => p != null)
                .Select(p => new ProductEligibilityResult
                {
                    ProductCode = p!.Code!,
                    ProductId = p!.ProductId,
                    ProductName = p.ProductName,
                    IsEligible = false,
                    EligibleAmount = 0,
                    IsProcessedByException = false,
                    ErrorMessage = GetErrorMessagesForProduct(p.ProductId, ruleResults, allPcards, allEcards)
                })
                .ToList();

            return failedProducts;
        }

        /// <summary>
        /// Processes products to determine their eligibility status and amounts based on various criteria.
        /// </summary>
        /// <param name="tenantId">The ID of the entity for which to process products.</param>
        /// <param name="allProducts">The complete list of products to evaluate.</param>
        /// <param name="productCap">Product cap information for eligibility calculations.</param>
        /// <param name="validProductIds">Product IDs that passed initial validation.</param>
        /// <param name="exceptionHandledProducts">Products that were processed through exception handling.</param>
        /// <param name="nonPCardEligibilityResults">Products without PCARD configurations.</param>
        /// <param name="pcardFailProducts">Products that failed PCARD validation.</param>
        /// <param name="ruleResults">Results from rule validation.</param>
        /// <param name="keyValues">A dictionary of parameter IDs and their corresponding values.</param>
        /// <returns>A tuple containing eligible and non-eligible product results.</returns>
        private (List<ProductEligibilityResult> eligibleProducts, List<ProductEligibilityResult> nonEligibleProducts) ProcessEligibleProducts(
            int tenantId,
            List<Product> allProducts,
            List<ProductCap> productCap,
            List<int?> validProductIds,
            List<ProductEligibilityResult> exceptionHandledProducts,
            List<ProductEligibilityResult> nonPCardEligibilityResults,
            List<ProductEligibilityResult> pcardFailProducts,
            List<RuleResult> ruleResults,
            Dictionary<int, object> keyValues)
        {
            // Creates product eligibility results for products that passed rule validation
            var ruleMatchedProducts = validProductIds
                .Select(id => allProducts.FirstOrDefault(p => p.ProductId == id))
                .Where(p => p != null)
                .Select(p => new ProductEligibilityResult
                {
                    ProductId = p!.ProductId,
                    ProductName = p.ProductName,
                    IsProcessedByException = false
                }).ToList();

            // Combines exception-handled and rule-matched products, removing duplicates
            var productsForEligibilityCheck = exceptionHandledProducts
                .Concat(ruleMatchedProducts)
                .GroupBy(p => p.ProductId)
                .Select(g => g.First())
                .ToList();

            // Checks eligible amounts for the combined product list
            var eligibleAmountResult = CheckEligibleAmount(tenantId, productsForEligibilityCheck, allProducts, productCap, keyValues);

            // Extracts eligible product IDs from the result
            var eligibleProductIds = eligibleAmountResult.Products!.Select(p => p.ProductId).Distinct().ToList();

            // Creates a set of all product IDs that have been processed in some way
            var handledProductIds = eligibleProductIds
                .Concat(exceptionHandledProducts.Select(p => p.ProductId))
                .Concat(nonPCardEligibilityResults.Select(p => p.ProductId))
                .Concat(pcardFailProducts.Select(p => p.ProductId))
                .Distinct()
                .ToHashSet();

            // Identifies products that haven't been processed yet
            var remainingProducts = allProducts
                .Where(p => !handledProductIds.Contains(p.ProductId))
                .ToList();

            // Retrieves all PCARDs for the entity
            var allPcards = _uow.PcardRepository.Query()
                .Where(p => p.Product!.TenantId == tenantId)
                .ToList();

            // Retrieves all ECARDs for the entity
            var allEcards = _uow.EcardRepository.Query()
                .Where(e => e.TenantId == tenantId)
                .ToList();

            // Creates eligibility results for non-eligible products with appropriate error messages
            var nonEligibleProducts = remainingProducts
                .Select(p => new ProductEligibilityResult
                {
                    ProductCode = p.Code!,
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    IsEligible = false,
                    EligibleAmount = 0,
                    IsProcessedByException = false,
                    ErrorMessage = GetErrorMessagesForProduct(p.ProductId, ruleResults, allPcards, allEcards)
                }).ToList();

            // Returns both eligible and non-eligible product results
            return (eligibleAmountResult.Products!, nonEligibleProducts);
        }

        /// <summary>
        /// Generates error messages for a product based on rule validation results and card configurations.
        /// </summary>
        /// <param name="productId">The ID of the product for which to generate error messages.</param>
        /// <param name="ruleResults">Results from rule validation.</param>
        /// <param name="allPcards">All PCARD configurations for the entity.</param>
        /// <param name="allEcards">All ECARD configurations for the entity.</param>
        /// <returns>A string containing error messages for the product, or an empty string if no errors found.</returns>
        private static string GetErrorMessagesForProduct(
            int productId,
            List<RuleResult> ruleResults,
            List<Pcard> allPcards,
            List<Ecard> allEcards)
        {
            // Returns error message if no rule results are provided
            if (ruleResults == null || ruleResults.Count == 0)
                return $"No rule results provided for Product ID{productId}";

            // Finds PCARDs associated with the product
            var pCards = allPcards.Where(p => p.ProductId == productId).ToList();
            // Returns error message if no PCARDs found
            if (pCards.Count == 0)
                return $"No PCARDs found for Product ID{productId}";

            // Extracts ECARD IDs from PCARD expressions
            var eCardIds = pCards
                .SelectMany(p => ParseIdsFromExpression(p.Expression))
                .Distinct()
                .ToList();

            // Returns error message if no ECARD IDs found
            if (eCardIds.Count == 0)
                return $"No valid ECard IDs found in PCARD expressions for Product ID{productId}";

            // Finds ECARDs associated with the extracted IDs
            var eCards = allEcards.Where(e => eCardIds.Contains(e.EcardId)).ToList();
            // Returns error message if no ECARDs found
            if (eCards.Count == 0)
                return $"No ECARDs found for Product ID: {productId}";

            // Extracts rule IDs from ECARD expressions
            var ruleIds = eCards
                .SelectMany(e => ParseIdsFromExpression(e.Expression))
                .Distinct()
                .ToList();

            // Gets rule IDs from rule results
            var resultRuleIds = ruleResults.Select(r => r.RuleID).ToList();

            // Checks if all rule IDs from ECARDs are present in rule results
            bool allMatch = ruleIds.All(id => resultRuleIds.Contains(id));

            // Returns error message if not all rules match
            if (!allMatch)
                return "No Rule Match";

            // Returns error message if no rule IDs found
            if (ruleIds.Count == 0)
                return $"No Rule IDs found in ECARD expressions for Product ID{productId}";

            // Creates a comma-separated list of rule IDs for error reporting
            var ruleIdList = string.Join(", ", ruleIds);

            // Finds rule results that match the extracted rule IDs
            var matchedRuleResults = ruleResults
                .Where(r => ruleIds.Contains(r.RuleID))
                .ToList();

            // Returns error message if no matching rule results found
            if (matchedRuleResults.Count == 0)
                return $"No matching RuleResults found for Rule IDs{ruleIdList} and Product ID{productId}";

            // Extracts error messages from matched rule results
            var matchedErrors = matchedRuleResults
                .SelectMany(r => r.ErrorMessage ?? [""])
                .ToList();

            // Returns concatenated error messages
            return $": {string.Join("; ", matchedErrors)}";
        }

        /// <summary>
        /// Parses numeric IDs from an expression string using regular expressions.
        /// </summary>
        /// <param name="expression">The expression string to parse for numeric IDs.</param>
        /// <returns>An enumerable collection of integer IDs found in the expression.</returns>
        private static List<int> ParseIdsFromExpression(string expression)
        {
            // Returns empty collection if expression is null or whitespace
            if (string.IsNullOrWhiteSpace(expression))
                return [];

            // Finds all numeric sequences in the expression
            var matches = MyRegex().Matches(expression);
            // Converts matched strings to integers and returns as list
            return [..matches
                .Select(m => int.Parse(m.Value))
                ];
        }

        /// <summary>
        /// Checks and calculates eligible amounts for products based on various criteria and parameters.
        /// </summary>
        /// <param name="validateProductIds">The list of product eligibility results to validate.</param>
        /// <param name="products">The complete list of products with their details.</param>
        /// <param name="productCaps">Product cap information for eligibility calculations.</param>
        /// <param name="keyValues">A dictionary of parameter IDs and their corresponding values.</param>
        /// <returns>An <see cref="EligibleAmountResult"/> object containing eligible amounts and scoring information.</returns>
        public EligibleAmountResult CheckEligibleAmount(
            int tenantId,
            IEnumerable<ProductEligibilityResult> validateProductIds,
            IEnumerable<Product> products,
            IEnumerable<ProductCap> productCaps,
            Dictionary<int, object> keyValues)
        {
            // Initializes parameter variables
            string Age = "";
            string Salary = "";
            int score = 0;

            // Resolves parameters using dynamic binding or fallbacks
            Age = GetBoundParameterValue("Age", tenantId, keyValues)?.ToString() ?? "";
            Salary = GetBoundParameterValue("Salary", tenantId, keyValues)?.ToString() ?? "";
            
            var scoreVal = GetBoundParameterValue("score", tenantId, keyValues);
            if (scoreVal != null && int.TryParse(scoreVal.ToString(), out var parsedScore))
            {
                score = parsedScore;
            }



            // Initializes results list
            List<ProductEligibilityResult> results = [];
            // Creates random number generator for probability of default
            Random probability = new();

            // Processes each product for eligibility calculation
            foreach (var item in validateProductIds)
            {
                // Generates random probability of default for the product
                int ProbabilityOfDefault = probability.Next(1, 101);
                // Tracks if product has valid cap amount criteria
                bool validProduct = false;

                // Retrieves product cap amounts for the current product
                var ProductCapAmount = _uow.ProductCapAmountRepository.Query()
                    .Where(p => p.ProductId == item.ProductId);

                // Evaluates each cap amount against provided parameters
                foreach (var cap in ProductCapAmount)
                {
                    bool isValid = true;

                    // Validates age criteria if not set to "All"
                    if (!string.IsNullOrWhiteSpace(cap?.Age) && cap?.Age != "All")
                    {
                        isValid &= MatchCondition(cap?.Age!, Age);
                    }
                    else
                    {
                        isValid &= true;
                    }

                    // Validates salary criteria if not set to "All"
                    if (!string.IsNullOrWhiteSpace(cap?.Salary) && cap?.Salary != "All")
                    {
                        isValid &= MatchCondition(cap?.Salary!, Salary);
                    }
                    else
                    {
                        isValid &= true; // No restriction => always valid
                    }

                    // If all criteria are valid, sets eligible amount and marks as valid
                    if (isValid)
                    {
                        item.EligibleAmount = (decimal)cap!.Amount;
                        validProduct = true;
                        break;
                    }
                }

                // Retrieves product caps for scoring evaluation
                var capsForProduct = productCaps.Where(p => p.ProductId == item.ProductId);
                // Gets product details for the current item
                var productDetails = products.First(r => r.ProductId == item.ProductId);

                // Initializes variables for eligibility calculation
                decimal eligibilityPercentage = 0;
                string productCapScore = "";
                int count = 0;

                // Evaluates each product cap against the score
                foreach (var cap in capsForProduct)
                {
                    if (score >= cap.MinimumScore && score <= cap.MaximumScore)
                    {
                        count++;
                        eligibilityPercentage = cap.ProductCapPercentage;
                        productCapScore = $"{cap.MinimumScore}-{cap.MaximumScore}";
                        break;
                    }
                }

                // Handles products not processed by exception with valid criteria and matching score range
                if (item.IsProcessedByException == false && validProduct && count > 0)
                {
                    _logger.LogError("652 - Eligible Product: {ProductId}, Score: {Score}, EligibilityPercent: {EligibilityPercent}",
                        productDetails.ProductId, score, eligibilityPercentage);
                    results.Add(new ProductEligibilityResult
                    {
                        ProductId = productDetails.ProductId,
                        ProductName = productDetails.ProductName,
                        EligibleAmount = (eligibilityPercentage / 100m) * item.EligibleAmount,
                        EligibilityPercent = eligibilityPercentage,
                        MaxEligibleAmount = item.EligibleAmount,
                        Score = score,
                        ProductCapScore = productCapScore,
                        ProbabilityOfDefault = ProbabilityOfDefault,
                        ProductCode = productDetails.Code!,
                    });
                }
                // Handles exception-processed products with valid criteria and matching score range
                else if (validProduct && count > 0)
                {
                    _logger.LogError("669 - Exception Eligible Product: {ProductId}, Score: {Score}, EligibilityPercent: {EligibilityPercent}",
                        productDetails.ProductId, score, item.EligibilityPercent);
                    results.Add(new ProductEligibilityResult
                    {
                        ProductId = productDetails.ProductId,
                        ProductName = productDetails.ProductName,
                        EligibleAmount = (item.EligibilityPercent / 100m) * item.EligibleAmount,
                        IsProcessedByException = item.IsProcessedByException,
                        Score = score,
                        MaxEligibleAmount = item.EligibleAmount,
                        ExceptionScope = item.ExceptionScope,
                        LimitAmountType = item.LimitAmountType,
                        LimitAmountPercent = item.LimitAmountPercent,
                        ProductCapScore = item.ProductCapScore,
                        ProductCapPercent = item.EligibilityPercent,
                        ProbabilityOfDefault = ProbabilityOfDefault,
                        EligibilityPercent = item.EligibilityPercent,
                        ProductCode = productDetails.Code!,
                    });
                }
                // Handles products not processed by exception with valid criteria but no matching score range
                else if (item.IsProcessedByException == false && validProduct && count <= 0)
                {
                    _logger.LogError("691 - Ineligible Product due to Score: {ProductId}, Score: {Score}",
                        productDetails.ProductId, score);
                    results.Add(new ProductEligibilityResult
                    {
                        ProductId = productDetails.ProductId,
                        ProductName = productDetails.ProductName,
                        EligibleAmount = 0,
                        IsEligible = false,
                        IsProcessedByException = false,
                        Score = score,
                        MaxEligibleAmount = 0,
                        ExceptionScope = null,
                        LimitAmountType = null,
                        LimitAmountPercent = item.LimitAmountPercent,
                        ProductCapScore = item.ProductCapScore,
                        ProductCapPercent = item.ProductCapPercent,
                        ProbabilityOfDefault = ProbabilityOfDefault,
                        ErrorMessage = "Could not find eligible Score criteria",
                        ProductCode = productDetails.Code!,
                    });
                }
                // Handles all other cases (invalid criteria)
                else
                {
                    _logger.LogError("714 - Ineligible Product due to Criteria: {ProductId}, Score: {Score}",
                        productDetails.ProductId, score);
                    results.Add(new ProductEligibilityResult
                    {
                        ProductId = productDetails.ProductId,
                        ProductName = productDetails.ProductName,
                        EligibleAmount = 0,
                        IsEligible = false,
                        IsProcessedByException = false,
                        Score = score,
                        MaxEligibleAmount = 0,
                        ExceptionScope = null,
                        LimitAmountType = null,
                        LimitAmountPercent = item.LimitAmountPercent,
                        ProductCapScore = item.ProductCapScore,
                        ProductCapPercent = item.ProductCapPercent,
                        ProbabilityOfDefault = ProbabilityOfDefault,
                        ErrorMessage = "Could not find eligible amount criteria",
                        ProductCode = productDetails.Code!,
                    });
                }
            }

            // Returns the final eligibility amount results
            return new EligibleAmountResult
            {
                Score = score,
                Products = results
            };
        }

        /// <summary>
        /// Matches an input value against an expression condition.
        /// </summary>
        /// <param name="expression">The expression to evaluate against.</param>
        /// <param name="input">The input value to test.</param>
        /// <returns>True if the input matches the expression condition, false otherwise.</returns>
        private static bool MatchCondition(string expression, string input)
        {
            // Returns false if either expression or input is null or whitespace
            if (string.IsNullOrWhiteSpace(expression) || string.IsNullOrWhiteSpace(input))
                return false;

            // Attempts to parse input as decimal for numeric comparisons
            bool isInputDecimal = decimal.TryParse(input, out decimal inputValue);
            // Trims whitespace from expression
            expression = expression.Trim();

            // Handles range expressions (e.g., "1000-2000")
            if (expression.Contains('-') && isInputDecimal)
            {
                var parts = expression.Split('-');
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out decimal minVal) &&
                    decimal.TryParse(parts[1], out decimal maxVal))
                {
                    return inputValue >= minVal && inputValue <= maxVal;
                }
            }

            // Handles less than or equal to expressions (e.g., "<=1000")
            if (expression.StartsWith("<=") && isInputDecimal)
            {
                if (decimal.TryParse(expression.AsSpan(2), out decimal val))
                    return inputValue <= val;
            }
            // Handles greater than or equal to expressions (e.g., ">=1000")
            else if (expression.StartsWith(">=") && isInputDecimal)
            {
                if (decimal.TryParse(expression.AsSpan(2), out decimal val))
                    return inputValue >= val;
            }
            // Handles less than expressions (e.g., "<1000")
            else if (expression.StartsWith('<') && isInputDecimal)
            {
                if (decimal.TryParse(expression.AsSpan(1), out decimal val))
                    return inputValue < val;
            }
            // Handles greater than expressions (e.g., ">1000")
            else if (expression.StartsWith('>') && isInputDecimal)
            {
                if (decimal.TryParse(expression.AsSpan(1), out decimal val))
                    return inputValue > val;
            }
            // Handles equality expressions (e.g., "=1000")
            else if (expression.StartsWith('=') && isInputDecimal)
            {
                if (decimal.TryParse(expression.AsSpan(1), out decimal val))
                    return inputValue == val;
            }

            // Fallback to string comparison if no numeric patterns matched
            return string.Equals(expression, input, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks products against exception rules to determine special eligibility handling.
        /// </summary>
        /// <param name="tenantId">The ID of the entity for which to check exceptions.</param>
        /// <param name="keyValues">A dictionary of parameter IDs and their corresponding values.</param>
        /// <param name="exceptionProducts">The list of exception products to evaluate.</param>
        /// <returns>A list of <see cref="ProductEligibilityResult"/> objects for products processed through exceptions.</returns>
        public IEnumerable<ProductEligibilityResult> CheckProductWithException(
       int tenantId,
       Dictionary<int, object> keyValues,
       IEnumerable<ExceptionManagement> exceptionRules
      )
        {
            var results = new List<ProductEligibilityResult>();
            var factors = _uow.FactorRepository.Query()
                .Where(f => f.TenantId == tenantId)
                .ToList();

            int score = GetScore(keyValues);

            foreach (var rule in exceptionRules)
            {
                if (!rule.IsActive) continue;

                if (rule.IsTemporary &&
                    (!rule.StartDate.HasValue || !rule.EndDate.HasValue ||
                     DateTime.Now < rule.StartDate || DateTime.Now > rule.EndDate))
                    continue;

                // Expression validation
                var keys = ExtractKeysFromExpression(rule.Expression, factors);
                if (keys.Any(k => !keyValues.ContainsKey(k))) continue;

                var validation = ProcessExpression(tenantId, rule.Expression, factors, keyValues, "Rule");
                if (!validation.IsValidationPassed) continue;

                double ruleEligibilityPercent = validation.ValidationDetails.Count != 0
                    ? validation.ValidationDetails.Count(v => v.IsValid) * 100.0 / validation.ValidationDetails.Count
                    : 0;

                // Decide target products
                List<int> targetProductIds = rule.Scope.Contains("Product Eligibility")
                    ? [.. _uow.ExceptionProductRepository.Query()
                        .Where(p => p.ExceptionManagementId == rule.ExceptionManagementId)
                        .Select(p => p.ProductId)
                        .Distinct()] : []
                    ;

                // Get product caps
                var caps = _uow.ProductCapRepository.Query()
                    .Where(p => targetProductIds.Contains(p.ProductId)
                             && p.MinimumScore <= score
                             && p.MaximumScore >= score)
                    .ToList();

                var products = _uow.ProductRepository.Query()
                    .Where(p => caps.Select(c => c.ProductId).Contains(p.ProductId))
                    .ToDictionary(p => p.ProductId);

                foreach (var cap in caps)
                {
                    if (!products.TryGetValue(cap.ProductId, out var product))
                        continue;

                    decimal baseAmount =
                        product.MaxEligibleAmount * (decimal)(cap.ProductCapPercentage / 100);

                    decimal eligibleAmount = baseAmount;
                    decimal eligibilityPercent = (decimal)cap.ProductCapPercentage;
                    string? limitType = null;
                    decimal limitPercent = 0;

                    // Apply Limit Amount if selected
                    if (rule.Scope.Contains("Limit Amount"))
                    {
                        if (rule.FixedPercentage > 0)
                        {
                            eligibleAmount = baseAmount * rule.FixedPercentage / 100;
                            eligibilityPercent = rule.FixedPercentage;
                            limitType = "Fixed";
                            limitPercent = rule.FixedPercentage;
                        }
                        else
                        {
                            eligibleAmount = baseAmount *
                                (rule.VariationPercentage + cap.ProductCapPercentage) / 100;

                            eligibilityPercent =
                                rule.VariationPercentage + cap.ProductCapPercentage;

                            limitType = "Variation";
                            limitPercent = rule.VariationPercentage;
                        }
                    }

                    results.Add(new ProductEligibilityResult
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName ?? "",
                        ProductCode = product.Code ?? "",
                        MaxEligibleAmount = product.MaxEligibleAmount,
                        EligibleAmount = eligibleAmount,
                        EligibilityPercent = eligibilityPercent,
                        LimitAmountType = limitType,
                        LimitAmountPercent = limitPercent,
                        ProductCapPercent = cap.ProductCapPercentage,
                        ProductCapScore = $"{cap.MinimumScore}-{cap.MaximumScore}",
                        IsProcessedByException = true,
                        ExceptionScope = rule.Scope
                    });
                }
            }

            return results;
        }
        private int GetScore(Dictionary<int, object> keyValues)
        {
            var scoreParam = _uow.ParameterRepository.Query()
                .FirstOrDefault(p =>
                    keyValues.Keys.Contains(p.ParameterId) &&
                    p.ParameterName!.Trim().ToLower() == "score");

            if (scoreParam != null &&
                keyValues.TryGetValue(scoreParam.ParameterId, out var value))
            {
                return Convert.ToInt32(value?.ToString() ?? "0");
            }

            return 0;
        }
        /// <summary>
        /// Validates a collection of eligibility rules against provided key values.
        /// </summary>
        /// <param name="tenantId">The ID of the entity to validate rules for.</param>
        /// <param name="erules">The collection of eligibility rules to validate.</param>
        /// <param name="keyValues">The dictionary of key-value pairs containing parameter values for validation.</param>
        /// <returns>A list of RuleResult objects containing validation results for each rule.</returns>
        public List<RuleResult> ValidateERules(int tenantId, List<Erule> erules, Dictionary<int, object> keyValues)
        {
            ValidationResult result = new();
            List<RuleResult> ruleResults = [];

            foreach (var rule in erules)
            {
                //var exceptionDetails = _uow.ExceptionManagementRepository.Query().FirstOrDefault(e => e.ExceptionID == rule.ExceptionId);

                //result = ValidateRule(ruleDetails.EruleId, keyValues);
                /// <summary>
                /// Validates a specific rule against the provided key values.
                /// </summary>
                var validateRuleResults = ValidateRule(tenantId, rule.EruleId, keyValues);

                /// <summary>
                /// Removes duplicate validation details by grouping on key properties and selecting the first occurrence.
                /// </summary>
                var distinctDetails = validateRuleResults.ValidationDetails
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
                    .Select(group => group.First())
                    .ToList();

                /// <summary>
                /// Determines if the rule is valid by checking if all validation details are valid.
                /// </summary>
                bool isRuleValid = distinctDetails.All(d => d.IsValid);

                /// <summary>
                /// Calculates the total number of conditions in the rule.
                /// </summary>
                int totalConditions = distinctDetails.Count;
                /// <summary>
                /// Counts the number of conditions that passed validation.
                /// </summary>
                int passedConditions = distinctDetails.Count(d => d.IsValid);
                /// <summary>
                /// Calculates the eligibility percentage for the rule.
                /// </summary>
                double ruleEligibilityPercentage = totalConditions > 0
                    ? (double)passedConditions / totalConditions * 100
                    : 0;

                //bool isRuleValid = passedConditions == totalConditions;

                /// <summary>
                /// Adds the validation details to the overall result.
                /// </summary>
                result.ValidationDetails.AddRange(validateRuleResults.ValidationDetails);

                /// <summary>
                /// Creates a new RuleResult object with the validation outcomes.
                /// </summary>
                ruleResults.Add(new RuleResult()
                {
                    RuleID = rule.EruleId,
                    IsValid = isRuleValid,
                    ValidationDetails = distinctDetails,
                    EligibilityPercentage = ruleEligibilityPercentage,
                    ErrorMessage = validateRuleResults.ErrorMessage ?? [],
                });
            }

            return ruleResults;
        }

        /// <summary>
        /// Retrieves Pcards that match the given Ecard IDs based on their expression evaluation.
        /// </summary>
        /// <param name="tenantId">The ID of the entity to filter Pcards.</param>
        /// <param name="ecardIds">The list of Ecard IDs to evaluate in Pcard expressions.</param>
        /// <returns>A list of Pcards whose expressions evaluate to true with the given Ecard IDs.</returns>
        public List<Pcard> GetPcardIdByEcards(int tenantId, List<int> ecardIds)
        {
            /// <summary>
            /// Retrieves all Pcards for the specified entity.
            /// </summary>
            var allPcards = _uow.PcardRepository.Query().Where(f => f.TenantId == tenantId);
            var matchedPcards = new List<Pcard>();

            foreach (var pcard in allPcards)
            {
                string expression = pcard.Expression;

                /// <summary>
                /// Replaces numeric values in the expression with "true" or "false" based on whether they exist in ecardIds.
                /// </summary>
                expression = MyRegex().Replace(expression, match =>
                {
                    int ecardId = int.Parse(match.Value);
                    return ecardIds.Contains(ecardId).ToString().ToLower(); // "true" or "false"
                });

                /// <summary>
                /// Replaces logical operators in the expression with C#-style operators.
                /// </summary>
                expression = expression.Replace("AND", "&&")
                                       .Replace("OR", "||")
                                       .Replace("NOT", "!");

                try
                {
                    /// <summary>
                    /// Evaluates the expression using DataTable.Compute method.
                    /// </summary>
                    if (!IsValidParentheses(expression))
                    {
                        continue;
                    }

                    bool finalResult = false;

                    try
                    {
                        var result = new DataTable().Compute(expression, null);

                        if (result is bool b)
                            finalResult = b;
                    }
                    catch
                    {
                        // If ANY error happens → invalid expression → false
                        finalResult = false;
                    }

                    if (finalResult)
                    {
                        matchedPcards.Add(pcard);
                    }
                }

                catch (Exception ex)
                {
                    /// <summary>
                    /// Handles invalid expressions by throwing a more descriptive exception.
                    /// </summary>
                    throw new Exception($"Invalid expression in Pcard ID {pcard.PcardId}:{pcard.Expression}", ex);
                }
            }

            return matchedPcards;
        }

        /// <summary>
        /// Validates a collection of Ecards against rule validation results.
        /// </summary>
        /// <param name="ecards">The collection of Ecards to validate.</param>
        /// <param name="ruleResults">The results of rule validations to use in Ecard expressions.</param>
        /// <returns>A list of EcardResult objects containing validation results for each Ecard.</returns>
        public List<EcardResult> ValidateEcards(List<Ecard> ecards, List<RuleResult> ruleResults)
        {
            var results = new List<EcardResult>();

            foreach (var ecard in ecards)
            {
                string expression = ecard.Expression;

                /// <summary>
                /// Replaces numeric values in the expression with "true" or "false" based on rule validation results.
                /// </summary>
                expression = MyRegex().Replace(expression, match =>
                {
                    int ruleId = int.Parse(match.Value);
                    var ruleResult = ruleResults.FirstOrDefault(r => r.RuleID == ruleId);
                    bool isValid = ruleResult != null && ruleResult.IsValid;
                    return isValid.ToString().ToLower(); // returns "true" or "false"
                });

                /// <summary>
                /// Replaces logical operators in the expression with C#-style operators.
                /// </summary>
                expression = expression.Replace("AND", "&&")
                                       .Replace("OR", "||")
                                       .Replace("NOT", "!")
                                       .Replace("and", "&&")
                                       .Replace("or", "||")
                                       .Replace("not", "!");

                bool result = false;
                try
                {
                    /// <summary>
                    /// Evaluates the expression using DataTable.Compute method.
                    /// </summary>
                    var evalResult = new DataTable().Compute(expression, null);
                    if (evalResult is bool boolResult)
                    {
                        result = boolResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error evaluating Ecard expression. Expression: {Expression}", expression);

                    /// <summary>
                    /// Sets result to false if expression evaluation fails.
                    /// </summary>
                    result = false; // if error in expression, assume false
                }

                /// <summary>
                /// Creates a new EcardResult with the validation outcome.
                /// </summary>
                results.Add(new EcardResult
                {
                    EcardID = ecard.EcardId,
                    Result = result
                });
            }

            return results;
        }

        /// <summary>
        /// Retrieves Ecards that reference the given rule IDs in their expressions.
        /// </summary>
        /// <param name="tenantId">The ID of the entity to filter Ecards.</param>
        /// <param name="ruleIds">The list of rule IDs to evaluate in Ecard expressions.</param>
        /// <returns>A list of Ecards whose expressions evaluate to true with the given rule IDs.</returns>
        public List<Ecard> GetEcardByEruleId(int tenantId, List<int> ruleIds)
        {
            /// <summary>
            /// Retrieves all Ecards for the specified entity.
            /// </summary>
            var allEcards = _uow.EcardRepository.Query().Where(f => f.TenantId == tenantId);
            var matchedEcards = new List<Ecard>();

            foreach (var ecard in allEcards)
            {
                string expression = ecard.Expression;

                /// <summary>
                /// Replaces numeric values in the expression with "true" or "false" based on whether they exist in ruleIds.
                /// </summary>
                expression = MyRegex().Replace(expression, match =>
                {
                    int ruleId = int.Parse(match.Value);
                    return ruleIds.Contains(ruleId).ToString().ToLower(); // "true" or "false"
                });

                /// <summary>
                /// Replaces logical operators in the expression with C#-style operators.
                /// </summary>
                expression = expression.Replace("AND", "&&")
                                      .Replace("OR", "||")
                                      .Replace("NOT", "!");

                //expression = Regex.Replace(expression, @"\bAND\b", "&&", RegexOptions.IgnoreCase);
                //expression = Regex.Replace(expression, @"\bOR\b", "||", RegexOptions.IgnoreCase);
                //expression = Regex.Replace(expression, @"\bNOT\b", "!", RegexOptions.IgnoreCase);

                /// <summary>
                /// Evaluates the expression using DataTable.Compute method.
                /// </summary>

                if (!IsValidParentheses(expression))
                {
                    continue; // Treat as false – skip this ecard
                }

                bool finalResult = false;

                try
                {
                    var result = new DataTable().Compute(expression, null);

                    if (result is bool b)
                        finalResult = b;
                }
                catch
                {
                    finalResult = false;
                }

                if (finalResult)
                {
                    matchedEcards.Add(ecard);
                }
                //var interpreter = new Interpreter();

                //bool result = interpreter.Eval<bool>(expression);
                //if (result)
                //{
                //    matchedEcards.Add(ecard);
                //}

            }

            return matchedEcards;
        }
        private static bool IsValidParentheses(string expr)
        {
            int balance = 0;

            foreach (char c in expr)
            {
                if (c == '(') balance++;
                else if (c == ')') balance--;

                if (balance < 0) return false;  // Closing without opening
            }

            return balance == 0; // Must be exactly balanced
        }
        /// <summary>
        /// Validates a collection of Pcards against Ecard validation results.
        /// </summary>
        /// <param name="pcards">The collection of Pcards to validate.</param>
        /// <param name="ecardResults">The results of Ecard validations to use in Pcard expressions.</param>
        /// <returns>A list of PcardResult objects containing validation results for each Pcard.</returns>
        public List<PcardResult> ValidatePcards(List<Pcard> pcards, List<EcardResult> ecardResults)
        {
            var results = new List<PcardResult>();

            foreach (var pcard in pcards)
            {
                string expression = pcard.Expression;

                /// <summary>
                /// Replaces numeric values in the expression with "true" or "false" based on Ecard validation results.
                /// </summary>
                expression = MyRegex().Replace(expression, match =>
                {
                    int ecardId = int.Parse(match.Value);
                    var ecardResult = ecardResults.FirstOrDefault(e => e.EcardID == ecardId);
                    bool isValid = ecardResult != null && ecardResult.Result;
                    return isValid.ToString().ToLower(); // "true" or "false"
                });

                /// <summary>
                /// Replaces logical operators in the expression with C#-style operators.
                /// </summary>
                expression = expression.Replace("AND", "&&")
                                       .Replace("OR", "||")
                                       .Replace("NOT", "!")
                                       .Replace("and", "&&")
                                       .Replace("or", "||")
                                       .Replace("not", "!");

                bool result = false;
                try
                {
                    /// <summary>
                    /// Evaluates the expression using DataTable.Compute method.
                    /// </summary>
                    var evalResult = new DataTable().Compute(expression, null);
                    if (evalResult is bool boolResult)
                    {
                        result = boolResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error evaluating Pcard expression. PcardId: {PcardId}, Expression: {Expression}", pcard.PcardId, expression);

                    /// <summary>
                    /// Sets result to false if expression evaluation fails.
                    /// </summary>
                    result = false; // invalid expression defaults to false
                }

                /// <summary>
                /// Creates a new PcardResult with the validation outcome.
                /// </summary>
                results.Add(new PcardResult
                {
                    PcardID = pcard.PcardId,
                    Result = result
                });
            }

            return results;
        }

        /// <summary>
        /// Processes an expression by evaluating nested expressions and then the final expression.
        /// </summary>
        /// <param name="tenantId">The ID of the entity being validated.</param>
        /// <param name="expression">The expression to process.</param>
        /// <param name="entities">The collection of entities used in validation.</param>
        /// <param name="keyValues">The dictionary of key-value pairs for validation.</param>
        /// <param name="type">The type of expression processing ("ECard", "Card", or "Rule").</param>
        /// <returns>A ValidationResult containing the outcome of expression evaluation.</returns>
        private ValidationResult ProcessExpression(int tenantId, string expression, IEnumerable<object> entities, Dictionary<int, object> keyValues, string type)
        {
            var validationDetails = new List<ValidationDetail>();

            /// <summary>
            /// Removes all spaces from the expression for consistent processing.
            /// </summary>
            expression = RemoveSpaces(expression);

            /// <summary>
            /// Processes all nested expressions (within parentheses) first.
            /// </summary>
            while (expression.Contains('('))
            {
                var innerMostExpression = GetInnerMostExpression(expression);
                var innerResult = ProcessExpression(tenantId, innerMostExpression, entities, keyValues, type);

                /// <summary>
                /// Replaces the inner expression with its boolean result ("true" or "false").
                /// </summary>
                expression = expression.Replace($"({innerMostExpression})", innerResult.IsValidationPassed ? "true" : "false");
                validationDetails.AddRange(innerResult.ValidationDetails);
            }

            /// <summary>
            /// Evaluates the final processed expression.
            /// </summary>
            var finalResult = EvaluateExpression(tenantId, expression, entities, keyValues, type);
            validationDetails.AddRange(finalResult.ValidationDetails);

            var errorMessages = validationDetails
        .Where(d => d.ErrorMessage != null)
        .SelectMany(d => d.ErrorMessage!)
        .ToList();

            /// <summary>
            /// If any "Not matched rule" error exists, return only that error.
            /// </summary>
            if (errorMessages.Any(e => e.Contains("No Rule Match", StringComparison.OrdinalIgnoreCase)))
            {
                return new ValidationResult
                {
                    IsValidationPassed = false,
                    ValidationDetails = validationDetails,
                    ErrorMessage = ["No Rule Match"]
                };
            }

            /// <summary>
            /// Otherwise return all distinct error messages.
            /// </summary>
            return new ValidationResult
            {
                IsValidationPassed = finalResult.IsValidationPassed,
                ValidationDetails = validationDetails,
                ErrorMessage = [.. errorMessages.Distinct()]
            };
        }

        /// <summary>
        /// Extracts the innermost expression from within parentheses.
        /// </summary>
        /// <param name="expression">The expression to parse.</param>
        /// <returns>The innermost expression without parentheses.</returns>
        private static string GetInnerMostExpression(string expression)
        {
            var start = expression.LastIndexOf('(');
            var end = expression.IndexOf(')', start);
            return expression.Substring(start + 1, end - start - 1);
        }

        /// <summary>
        /// Evaluates a boolean expression by splitting into OR and AND components.
        /// </summary>
        /// <param name="tenantId">The ID of the entity being validated.</param>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="entities">The collection of entities used in validation.</param>
        /// <param name="keyValues">The dictionary of key-value pairs for validation.</param>
        /// <param name="type">The type of expression evaluation ("ECard", "Card", or "Rule").</param>
        /// <returns>A ValidationResult containing the evaluation outcome.</returns>
        private ValidationResult EvaluateExpression(int tenantId, string expression, IEnumerable<object> entities, Dictionary<int, object> keyValues, string type)
        {
            var validationDetails = new List<ValidationDetail>();
            var Errors = new List<string>();
            /// <summary>
            /// Removes all spaces from the expression for consistent processing.
            /// </summary>
            expression = RemoveSpaces(expression);
            var orList = new List<bool>();
            /// <summary>
            /// Splits the expression into OR components.
            /// </summary>
            var orParts = MyRegex4().Split(expression);
            foreach (var orPart in orParts)
            {
                var andList = new List<bool>();
                /// <summary>
                /// Splits each OR component into AND components.
                /// </summary>
                var andParts = MyRegex5().Split(orPart);
                foreach (var andPart in andParts)
                {
                    if (andPart.Equals("true", StringComparison.OrdinalIgnoreCase) || andPart.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        /*validationDetails.Add(new ValidationDetail
                        {
                            IsValid = andPart.Equals("true", StringComparison.OrdinalIgnoreCase),
                            Condition = andPart
                        });*/

                        /// <summary>
                        /// Handles literal boolean values in the expression.
                        /// </summary>
                        andList.Add(andPart.Equals("true", StringComparison.OrdinalIgnoreCase));
                        continue;
                    }

                    /// <summary>
                    /// Validates the entity component of the expression.
                    /// </summary>
                    var result = ValidateEntity(tenantId, andPart, entities, keyValues, type);
                    andList.Add(result.IsValidationPassed);
                    validationDetails.AddRange(result.ValidationDetails);
                    Errors.AddRange(result.ErrorMessage ?? []);
                }
                /// <summary>
                /// Adds the result of AND evaluation (true only if all AND components are true).
                /// </summary>
                orList.Add(andList.All(x => x));
            }
            var finalErrorMessages = validationDetails
        .Where(d => d.ErrorMessage != null)
        .SelectMany(d => d.ErrorMessage!)
        .ToList();

            /// <summary>
            /// Filters error messages to only include "Not matched rule" if present.
            /// </summary>
            if (finalErrorMessages.Any(e => e.Contains("No Rule Match", StringComparison.OrdinalIgnoreCase)))
            {
                finalErrorMessages = [.. finalErrorMessages.Where(e => e.Contains("No Rule Match", StringComparison.OrdinalIgnoreCase))];
            }
            else
            {
                /// <summary>
                /// Otherwise returns distinct error messages.
                /// </summary>
                finalErrorMessages = [.. finalErrorMessages.Distinct()];
            }

            /// <summary>
            /// Returns the validation result (true if any OR component is true).
            /// </summary>
            return new ValidationResult
            {
                IsValidationPassed = orList.Any(x => x),
                ValidationDetails = validationDetails,
                ErrorMessage = finalErrorMessages
            };


        }


        /// <summary>
        /// Removes all whitespace characters from a string.
        /// </summary>
        /// <param name="expression">The string to remove whitespace from.</param>
        /// <returns>The string without any whitespace characters.</returns>
        private static string RemoveSpaces(string expression)
        {
            /// <summary>
            /// Filters out all whitespace characters from the expression.
            /// </summary>
            return string.Concat(expression.Where(c => !char.IsWhiteSpace(c)));
        }

        /// <summary>
        /// Validates an entity component of an expression based on its type.
        /// </summary>
        /// <param name="tenantId">The ID of the entity being validated.</param>
        /// <param name="tenantIdStr">The entity identifier string to validate.</param>
        /// <param name="entities">The collection of entities used in validation.</param>
        /// <param name="keyValues">The dictionary of key-value pairs for validation.</param>
        /// <param name="type">The type of entity validation ("ECard", "Card", or "Rule").</param>
        /// <returns>A ValidationResult containing the validation outcome.</returns>
        private ValidationResult ValidateEntity(int tenantId, string tenantIdStr, IEnumerable<object> entities, Dictionary<int, object> keyValues, string type)
        {
            var validationDetails = new List<ValidationDetail>();
            var isValidationPassed = false;
            var Errors = new List<string>();
            //var exceptionManagement = _uow.ExceptionManagementRepository.Query()
            //                                 //.Where(x => x.ProductId == productId)
            //                                 .First();

            //DateTime currentDate = DateTime.Now;

            //bool isWithinDateRange = exceptionManagement.StartDate != null && exceptionManagement.EndDate != null
            //                   && currentDate >= exceptionManagement.StartDate
            //                   && currentDate <= exceptionManagement.EndDate;

            /// <summary>
            /// Handles validation based on the entity type.
            /// </summary>
            switch (type)
            {
                case "ECard":
                    /// <summary>
                    /// Validates an ECard entity.
                    /// </summary>
                    var list = (IEnumerable<Ecard>)entities;
                    var eCard = list.FirstOrDefault(x => x.EcardId == int.Parse(tenantIdStr));
                    if (eCard != null)
                    {
                        var validateCardResult = ValidateCard(tenantId, eCard, keyValues);
                        isValidationPassed = validateCardResult.IsValidationPassed;
                        validationDetails.AddRange(validateCardResult.ValidationDetails);
                    }
                    break;

                case "Card":
                    /// <summary>
                    /// Validates a Card entity by rule ID.
                    /// </summary>
                    var cardResult = ValidateRule(tenantId, int.Parse(tenantIdStr), keyValues);
                    isValidationPassed = cardResult.IsValidationPassed;
                    validationDetails.AddRange(cardResult.ValidationDetails);
                    break;

                case "Rule":
                    {
                        /// <summary>
                        /// Validates a Rule entity by parsing conditions and factors.
                        /// </summary>
                        var conditions = _uow.ConditionRepository.GetAll();

                        var normalizedEntityStr = tenantIdStr
                            .Replace(" ", "")
                            .ToLowerInvariant();

                        var conditionBlocks = normalizedEntityStr
                            .Split("and", StringSplitOptions.RemoveEmptyEntries);

                        foreach (var block in conditionBlocks)
                        {
                            foreach (var condition in conditions.OrderByDescending(c => c.ConditionValue!.Length))
                            {
                                var match = MyRegex6().Match(block);
                                string[] facts = [];

                                if (condition.ConditionValue != null)
                                {
                                    var normalizedCondition = condition.ConditionValue
                                        .Replace(" ", "")
                                        .ToLowerInvariant();
                                    var op = match.Groups[2].Value.ToLowerInvariant();

                                    if (!string.Equals(op, normalizedCondition, StringComparison.OrdinalIgnoreCase))
                                        continue;

                                    facts =
                                    [
                                        match.Groups[1].Value,
                                        match.Groups[3].Value
                                    ];
                                }

                                if (facts.Length != 2)
                                    continue;

                                var factors = (IEnumerable<Factor>)entities;
                                Factor? factor = null;

                                var numberPart = new string([.. facts[0].TakeWhile(char.IsDigit)]);
                                if (!int.TryParse(numberPart, out int parsedValue))
                                    continue;

                                var compareValue = facts[1]?.Replace(" ", "") ?? "";

                                // Case 1: Range (e.g., "18000-20000")
                                if (compareValue.Contains('-'))
                                {
                                    var parts = compareValue.Split('-', StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length == 2 &&
                                        decimal.TryParse(parts[0], out var startVal) &&
                                        decimal.TryParse(parts[1], out var endVal))
                                    {
                                        factor = factors.FirstOrDefault(x =>
                                            x.ParameterId == parsedValue &&
                                            decimal.TryParse(x.Value1, out var fStart) &&
                                            decimal.TryParse(x.Value2, out var fEnd) &&
                                            fStart == startVal &&
                                            fEnd == endVal);
                                    }
                                }
                                else
                                {
                                    factor = factors.FirstOrDefault(x =>
                                        x.ParameterId == parsedValue &&
                                        string.Equals(
                                            x.Value1?.Replace(" ", ""),
                                            compareValue,
                                            StringComparison.OrdinalIgnoreCase));
                                }

                                if (factor == null)
                                    continue;


                                var parameter = _uow.ParameterRepository.GetById(factor.ParameterId!.Value);
                                var datatype = _uow.DataTypeRepository.GetById(parameter.DataTypeId!.Value);

                                string? codeValue = null;
                                string? nameValue = null;

                                string baseParameterName = parameter.ParameterName!;

                                if (baseParameterName.EndsWith("Name", StringComparison.OrdinalIgnoreCase))
                                    baseParameterName = baseParameterName[..^4];
                                else if (baseParameterName.EndsWith("Code", StringComparison.OrdinalIgnoreCase))
                                    baseParameterName = baseParameterName[..^4];

                                foreach (var kv in keyValues)
                                {
                                    var kvParam = _uow.ParameterRepository.GetById(kv.Key);
                                    if (kvParam?.ParameterName == null) continue;

                                    if (kvParam.ParameterName.Equals(baseParameterName + "Code",
                                        StringComparison.OrdinalIgnoreCase))
                                        codeValue = kv.Value?.ToString();
                                    else if (kvParam.ParameterName.Equals(baseParameterName + "Name",
                                        StringComparison.OrdinalIgnoreCase))
                                        nameValue = kv.Value?.ToString();
                                }

                                object valueToValidate;

                                if (!string.IsNullOrEmpty(codeValue) || !string.IsNullOrEmpty(nameValue))
                                {
                                    valueToValidate = new { Code = codeValue, Name = nameValue };
                                }
                                else
                                {
                                    keyValues.TryGetValue(factor.ParameterId!.Value, out var rawValue);
                                    valueToValidate = rawValue!;
                                }

                                var detail = Validate(condition, factor, valueToValidate, datatype.DataTypeName);
                                detail.ParameterId = factor.ParameterId;
                                detail.FactorName = factor.FactorName;
                                isValidationPassed = detail.IsValid;
                                validationDetails.Add(detail);
                            }
                        }

                        break;
                    }
            }

            Errors = [.. validationDetails
           .Where(d => d.ErrorMessage != null)
           .SelectMany(d => d.ErrorMessage!)];

            /// <summary>
            /// Filters error messages to only include "Not matched rule" if present.
            /// </summary>
            if (Errors.Any(e => e.Contains("No Rule Match", StringComparison.OrdinalIgnoreCase)))
            {
                Errors = [.. Errors.Where(e => e.Contains("No Rule Match", StringComparison.OrdinalIgnoreCase))];
            }
            /// <summary>
            /// Returns the validation result for the entity.
            /// </summary>
            return new ValidationResult
            {
                IsValidationPassed = isValidationPassed,
                ValidationDetails = validationDetails,
                ErrorMessage = Errors
            };
        }

        /// <summary>
        /// Validates an ECard by processing its expression.
        /// </summary>
        /// <param name="tenantId">The ID of the entity being validated.</param>
        /// <param name="card">The ECard to validate.</param>
        /// <param name="keyValues">The dictionary of key-value pairs for validation.</param>
        /// <returns>A ValidationResult containing the validation outcome.</returns>
        private ValidationResult ValidateCard(int tenantId, Ecard card, Dictionary<int, object> keyValues)
        {
            return ProcessExpression(tenantId, card.Expression, [], keyValues, "Card");
        }

        /// <summary>
        /// Validates a rule by processing its expression.
        /// </summary>
        /// <param name="tenantId">The ID of the entity being validated.</param>
        /// <param name="ruleId">The ID of the rule to validate.</param>
        /// <param name="keyValues">The dictionary of key-value pairs for validation.</param>
        /// <returns>A ValidationResult containing the validation outcome.</returns>
        private ValidationResult ValidateRule(int tenantId, int ruleId, Dictionary<int, object> keyValues)
        {
            var rule = _uow.EruleRepository.Query().First(f => f.EruleId == ruleId && f.TenantId == tenantId);
            var factors = _uow.FactorRepository.Query().Where(f => f.TenantId == tenantId);
            return ProcessExpression(tenantId, rule.Expression, factors, keyValues, "Rule");
        }

        /// <summary>
        /// Validates a condition against a factor and provided value.
        /// </summary>
        /// <param name="condition">The condition to validate against.</param>
        /// <param name="factor">The factor containing expected values.</param>
        /// <param name="value">The provided value to validate.</param>
        /// <param name="datatype">The data type of the values being compared.</param>
        /// <param name="productId">The ID of the product being validated (optional).</param>
        /// <returns>A ValidationDetail containing the validation outcome.</returns>
        private ValidationDetail Validate(Condition condition, Factor factor, object value, string? datatype = null, int? productId = null)
        {

            string? codeValue = null;
            string? nameValue = null;
            string? providedValue = null;

            // If value has Code/Name properties
            if (value != null)
            {
                var type = value.GetType();
                var codeProp = type.GetProperty("Code");
                var nameProp = type.GetProperty("Name");

                if (codeProp != null || nameProp != null)
                {
                    codeValue = codeProp?.GetValue(value)?.ToString();
                    nameValue = nameProp?.GetValue(value)?.ToString();

                    // For display and comparison purposes, combine them
                    providedValue = string.IsNullOrEmpty(codeValue) ? nameValue :
                                    string.IsNullOrEmpty(nameValue) ? codeValue :
                                    $"{codeValue}-{nameValue}";
                }
                else
                {
                    providedValue = value.ToString();
                }
            }
            var validationResult = new ValidationDetail
            {
                Condition = condition.ConditionValue,
                FactorValue = factor?.Value1,
                ProvidedValue = value?.ToString(),
                ParameterId = factor?.ParameterId,
                ErrorMessage = []// Include ParameterId in the result
                ,
                ProductId = productId
            };

            /// <summary>
            /// Returns invalid result if any required parameter is null.
            /// </summary>
            if (value == null || condition == null || factor == null)
            {
                validationResult.IsValid = false;
                validationResult.ErrorMessage.Add("No Rule Match");
                return validationResult;
            }
            if (!string.IsNullOrWhiteSpace(factor.Value1) &&
                 factor.Value1.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                validationResult.IsValid = true;
                return validationResult;
            }
            /// <summary>
            /// Validates based on the condition type.
            /// </summary>
            switch (condition.ConditionValue)
            {
                case "=":
                    validationResult.IsValid = string.Equals(factor.Value1, validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase);
                    break;
                case "!=":
                    validationResult.IsValid = !string.Equals(factor.Value1, validationResult.ProvidedValue, StringComparison.OrdinalIgnoreCase);
                    break;
                case "<":
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a < b, datatype ?? "");
                    break;
                case ">":
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a > b, datatype ?? "");
                    break;
                case "<=":
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a <= b, datatype ?? "");
                    break;
                case ">=":
                    validationResult.IsValid = CompareValues(factor.Value1!, validationResult.ProvidedValue!, (a, b) => a >= b, datatype ?? "");
                    break;
                case "Range":
                    if (factor.Value2 != null)
                    {
                        if (datatype == "Date")
                        {
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
                            validationResult.IsValid = lowerBound <= provided && provided <= upperBound;
                        }
                    }
                    break;
                case "In List":
                    {
                        var listName = factor.Value1?.Trim();

                        // If Value1 itself is empty, automatically fail validation
                        if (string.IsNullOrEmpty(listName))
                        {
                            validationResult.IsValid = false;
                            break;
                        }

                        // Try to find the list by its name
                        var list = _uow.ManagedListRepository.Query()
                            .FirstOrDefault(l => l.ListName == listName);

                        // If list doesn't exist, assume the value must match Value1 directly
                        if (list == null)
                        {
                            validationResult.IsValid =
                                string.Equals(listName, validationResult.ProvidedValue?.Trim(), StringComparison.OrdinalIgnoreCase);
                            break;
                        }

                        // Fetch items belonging to that list
                        var listId = list.ListId;
                        var listItems = _uow.ListItemRepository.Query()
                            .Where(l => l.ListId == listId)
                            .Select(l => l.ItemName)
                            .ToList();

                        // Validate provided value exists in list items
                        var existsInDb = listItems.Any(item =>
                              string.Equals(item?.Trim(), nameValue?.Trim(), StringComparison.OrdinalIgnoreCase));
                        var existsInMemory = _uow.ListItemRepository
                             .ExistsInMemory(listId, nameValue ?? "");

                        var exists = existsInDb || existsInMemory;

                        if (exists)
                        {
                            //  Already exists  validation passes
                            validationResult.IsValid = true;
                        }
                        else
                        {
                            //  Not found  Add new value to the list

                            var newItem = new ListItem
                            {
                                ListId = listId,
                                Code = codeValue ?? "",
                                ItemName = nameValue ?? "",
                                CreatedByDateTime = DateTime.Now,
                                UpdatedByDateTime = DateTime.Now,
                                CreatedBy = "System",

                            };

                            _uow.ListItemRepository.Add(newItem, true);
                            // commit to DB

                            //  Mark validation as successful after adding
                            validationResult.IsValid = true;
                        }

                        break;
                    }

                case "Not In List":
                    {
                        var listName = factor.Value1?.Trim();
                        if (string.IsNullOrEmpty(listName))
                        {
                            validationResult.IsValid =
                                !string.Equals(listName, validationResult.ProvidedValue?.Trim(), StringComparison.OrdinalIgnoreCase);// if no list, treat as not in list
                            break;
                        }

                        var list = _uow.ManagedListRepository.Query()
                            .FirstOrDefault(l => l.ListName == listName);

                        if (list == null)
                        {
                            validationResult.IsValid = true;
                            break;
                        }

                        var listId = list.ListId;
                        var listItems = _uow.ListItemRepository.Query()
                            .Where(l => l.ListId == listId)
                            .Select(l => l.ItemName)
                            .ToList();

                        validationResult.IsValid = !listItems.Any(item =>
                            string.Equals(item?.Trim(), validationResult.ProvidedValue?.Trim(), StringComparison.OrdinalIgnoreCase));

                        break;
                    }
                default:
                    validationResult.IsValid = false;
                    validationResult.ErrorMessage.Add($"Invalid condition: {condition.ConditionValue}");
                    break;
            }
            if (!validationResult.IsValid)
            {
                validationResult.ErrorMessage ??= [];

                /// <summary>
                /// Adds a descriptive error message for failed validation.
                /// </summary>
                validationResult.ErrorMessage.Add($"{factor.Parameter!.ParameterId} Factor {factor.FactorName}");
            }


            return validationResult;
        }

        /// <summary>
        /// Compares two values using the specified comparison function, handling different data types.
        /// </summary>
        /// <param name="factorValue">The expected value from the factor.</param>
        /// <param name="providedValue">The provided value to compare.</param>
        /// <param name="comparison">The comparison function to use.</param>
        /// <param name="datatype">The data type of the values being compared.</param>
        /// <returns>True if the comparison is successful, false otherwise.</returns>
        private static bool CompareValues(string factorValue, string providedValue, Func<double, double, bool> comparison, string datatype)
        {
            if (datatype == "Date")
            {
                if (DateTime.TryParse(factorValue, out var factorDate) &&
                    DateTime.TryParse(providedValue, out var providedDate))
                {
                    return comparison(providedDate.Ticks, factorDate.Ticks);
                }
                return false; // Invalid date format
            }

            return double.TryParse(factorValue, out var factor) &&
                   double.TryParse(providedValue, out var provided) &&
                   comparison(provided, factor);
        }

        /// <summary>
        /// Extracts parameter keys from an expression by parsing condition components.
        /// </summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="factors">The collection of factors to match against.</param>
        /// <returns>A list of parameter IDs found in the expression.</returns>
        private static List<int> ExtractKeysFromExpression(string expression, List<Factor> factors)
        {
            var keys = new List<int>();
            if (string.IsNullOrWhiteSpace(expression)) return keys;

            expression = expression.Replace("(", "").Replace(")", "");

            /// <summary>
            /// Splits the expression into individual condition components.
            /// </summary>
            var conditions = expression
                .Split(["and", "or"], StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());

            foreach (var cond in conditions)
            {
                foreach (var factor in factors)
                {
                    if (!string.IsNullOrWhiteSpace(factor.FactorName) &&
                        cond.StartsWith(factor.ParameterId?.ToString() ?? "", StringComparison.OrdinalIgnoreCase))
                    {
                        if (factor.ParameterId.HasValue)
                            keys.Add(factor.ParameterId.Value);
                    }
                }
            }

            return [.. keys.Distinct()];
        }

        /// <summary>
        /// Retrieves rules that match all required parameter keys in the provided key values.
        /// </summary>
        /// <param name="tenantId">The ID of the entity to filter rules.</param>
        /// <param name="keyValues">The dictionary of key-value pairs to match against rule requirements.</param>
        /// <returns>A list of rules where all required parameters are present in the key values.</returns>
        private List<Erule> GetMatchRules(int tenantId, Dictionary<int, object> keyValues)
        {
            var now = DateTime.Now;

            /// <summary>
            /// Retrieves active rules valid for the current time.
            /// </summary>
            var eRules = _uow.EruleRepository.Query()
         .Where(f => f.TenantId == tenantId
                     && f.EruleMaster != null
                     && f.EruleMaster.IsActive
                     && (f.ValidFrom <= now && (!f.ValidTo.HasValue || f.ValidTo >= now)))
         .GroupBy(f => f.EruleMasterId) // group by master id
         .Select(g => g.OrderByDescending(f => f.Version).First()) // pick highest version
         .ToList();
            /// <summary>
            /// Retrieves all factors for the entity to resolve FactorName to ParameterId.
            /// </summary>
            var factors = _uow.FactorRepository.Query()
                .Where(f => f.TenantId == tenantId && f.ParameterId.HasValue)
                .ToList();

            var matchRules = new List<Erule>();

            foreach (var rule in eRules)
            {
                /// <summary>
                /// Extracts required parameter keys from the rule expression.
                /// </summary>
                var ruleKeys = ExtractKeysFromExpression(rule.Expression, factors); // uses FactorName

                /// <summary>
                /// Adds rule only if all required parameter keys are present in input.
                /// </summary>
                if (ruleKeys.All(k => keyValues.ContainsKey(k)))
                {
                    matchRules.Add(rule);
                }
            }

            return matchRules;
        }

        [GeneratedRegex(@"\d+")]
        private static partial Regex MyRegex();



        public async Task<BREIntegrationResponses> ProcessBREIntegration(Dictionary<string, object> KeyValues, int TenantId, string? RequestId)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = RequestId ?? Guid.NewGuid().ToString();
            var allMandatoryParams = _uow.ParameterRepository.Query()
    .Where(p => p.IsMandatory && p.TenantId == TenantId)
    .ToList();

            // Count how many mandatory parameters are present in KeyValues (case-insensitive)
            var isMandatoryTrue = allMandatoryParams
                .Count(p => KeyValues.Keys.Any(k => string.Equals(k, p.ParameterName, StringComparison.OrdinalIgnoreCase)));

            var isMandatoryPassed = isMandatoryTrue == allMandatoryParams.Count;
            if (!isMandatoryPassed)
            {
                // Collect missing parameters
                var missingParams = allMandatoryParams
                    .Where(p => !KeyValues.ContainsKey(p.ParameterName!))

                    .ToList();

                return new BREIntegrationResponses
                {
                    RequestId = requestId,
                    Message = "Please fill in the mandatory fields.",
                    MandatoryParameters = [.. missingParams.Select(missingParams => missingParams.ParameterName!)],

                };
            }
            // Resolve NationalId dynamically
            var nationalId = GetBoundParameterValue("NationalId", TenantId, [], KeyValues)?.ToString();

            // Resolve LoanNo dynamically
            var loanNo = GetBoundParameterValue("LoanNo", TenantId, [], KeyValues)?.ToString();

            var mustFieldsMissing = new List<string>();

            if (string.IsNullOrWhiteSpace(nationalId))
                mustFieldsMissing.Add("NationalId");

            if (string.IsNullOrWhiteSpace(loanNo))
                mustFieldsMissing.Add("LoanNo");

            if (mustFieldsMissing.Count != 0)
            {
                return new BREIntegrationResponses
                {
                    RequestId = requestId,
                    Message = "Both LoanNo and NationalId are required.",
                    MandatoryParameters = mustFieldsMissing
                };
            }
            // Check duplicate evaluation record
            //if (!string.IsNullOrWhiteSpace(nationalId) && !string.IsNullOrWhiteSpace(loanNo))
            //{
            //    var alreadyExists = _uow.EvaluationHistoryRepository.Query()
            //        .Any(e => e.NationalId.ToLower() == nationalId.ToLower() && e.LoanNo.ToLower() == loanNo.ToLower());

            //    if (alreadyExists)
            //    {
            //        return new BREIntegrationResponses
            //        {
            //            RequestId = requestId,
            //            Message = $"This customer with National ID {nationalId} and Loan No {loanNo} has been evaluated already.",
            //        };
            //    }
            //}

            var listValidationResult = ValidateListTypeParameters(KeyValues, TenantId, requestId);
            if (listValidationResult != null)
            {
                return listValidationResult;
            }
  
            var evaluation = new EvaluationHistory
            {
                TenantId = TenantId,
                EvaluationTimeStamp = DateTime.UtcNow,
                NationalId = nationalId!,
                LoanNo = loanNo!
            };

            // Step: Save initial evaluation to get ID
            _uow.EvaluationHistoryRepository.Add(evaluation);
            await _uow.CompleteAsync();

            try
            {
                // Step 1: Load all internal parameters
                var parameters = _uow.ParameterRepository.GetAll().ToList();

                // Step 2: Build a dictionary (ParameterId => Value) from passed KeyValues
                var parameterDictionary = new Dictionary<int, object>();


                foreach (var kv in KeyValues)
                {
                    var parameterName = kv.Key?.Trim();
                    var parameterValue = kv.Value;

                    if (string.IsNullOrWhiteSpace(parameterName))
                        continue;

                    var normalizedInputName = new string([.. parameterName.Where(c => !char.IsWhiteSpace(c))]).ToLower();

                    var parameter = parameters.FirstOrDefault(p =>
                        p.TenantId == TenantId &&
                        !string.IsNullOrWhiteSpace(p.ParameterName) &&
                        new string([.. p.ParameterName.Where(c => !char.IsWhiteSpace(c))]).Equals(normalizedInputName
                        , StringComparison.CurrentCultureIgnoreCase)
                    );

                    if (parameter != null)
                    {
                        parameterDictionary[parameter.ParameterId] = parameterValue;
                    }
                }
                // Step 3: Load API Parameters + Mappings
                var apiParameters = _uow.ApiParametersRepository
                    .GetAll()
                    .ToList();

                var apiParameterIds = apiParameters.Select(ap => ap.ApiParamterId).ToList();

                var apiMappings = _uow.ApiParameterMapsRepository
                    .GetAll()
                    .Where(map => apiParameterIds.Contains(map.ApiParameterId))
                    .ToList();



                var externalApiResults = await CallAllExternalApisDynamic(parameterDictionary, evaluation);

                // Merge internal + external parameters into final key-value set
                var keyValuesForEligibility = new Dictionary<int, object>(parameterDictionary);
                string Normalize(string name)
                {
                    if (string.IsNullOrWhiteSpace(name)) return string.Empty;
                    return MyRegex2().Replace(name.Trim(), "").ToLowerInvariant();
                }
                var skipWords = new[] { "message", "error", "success" };

                bool ShouldSkip(string name)
                {
                    var norm = Normalize(name);
                    return skipWords.Any(sw => norm.Contains(sw));
                }



                //  Keep track of logs for debugging
                var mappingLogs = new List<string>();

                var updatedParamIds = new HashSet<int>();

                foreach (var apiResult in externalApiResults)
                {
                    var flattened = FlattenJson(apiResult.Value);

                    foreach (var kv in flattened)
                    {
                        var outputNameRaw = kv.Key?.Trim();
                        var outputValue = kv.Value;

                        if (string.IsNullOrWhiteSpace(outputNameRaw))
                            continue;

                        if (ShouldSkip(outputNameRaw))
                            continue;

                        var normalizedOutputName = Normalize(outputNameRaw);

                        //  Try to find API parameter (Output direction)
                        var apiOutputParam = apiParameters.FirstOrDefault(ap =>
                            ap.ParameterDirection.Equals("Output", StringComparison.OrdinalIgnoreCase) &&
                            Normalize(ap.ParameterName ?? "") == normalizedOutputName);

                        bool isMapped = false;

                        if (apiOutputParam != null)
                        {
                            //Try to find mapping for this API Output parameter
                            var apiMap = apiMappings.FirstOrDefault(m => m.ApiParameterId == apiOutputParam.ApiParamterId);
                            if (apiMap != null)
                            {
                                var mappedInternalParam = parameters.FirstOrDefault(p => p.ParameterId == apiMap.ParameterId);
                                if (mappedInternalParam != null)
                                {
                                    //: Only add if not already present from internal parameters
                                    if (!keyValuesForEligibility.ContainsKey(mappedInternalParam.ParameterId))
                                    {
                                        keyValuesForEligibility[mappedInternalParam.ParameterId] = outputValue!;
                                        updatedParamIds.Add(mappedInternalParam.ParameterId);
                                    }
                                    isMapped = true;
                                }
                            }

                            // If no mapping, try direct match
                            if (!isMapped)
                            {
                                var directInternal = parameters.FirstOrDefault(p =>
                                    Normalize(p.ParameterName ?? "") == Normalize(apiOutputParam.ParameterName ?? ""));

                                if (directInternal != null)
                                {
                                    // Only add if not already present from internal parameters
                                    if (!keyValuesForEligibility.ContainsKey(directInternal.ParameterId))
                                    {
                                        keyValuesForEligibility[directInternal.ParameterId] = outputValue!;
                                        updatedParamIds.Add(directInternal.ParameterId);
                                    }
                                    isMapped = true;
                                }
                            }
                        }

                        // Fallback to direct internal parameter name match
                        if (!isMapped)
                        {
                            var fallbackInternal = parameters.FirstOrDefault(p =>
                                Normalize(p.ParameterName ?? "") == normalizedOutputName);

                            if (fallbackInternal != null)
                            {
                                if (!keyValuesForEligibility.ContainsKey(fallbackInternal.ParameterId))
                                {
                                    keyValuesForEligibility[fallbackInternal.ParameterId] = outputValue!;
                                    updatedParamIds.Add(fallbackInternal.ParameterId);
                                }
                            }
                        }
                    }
                }

                //  Utility to safely find value in keyValues
                //object? FindValue(string target)
                //{
                //    var normalizedTarget = Normalize(target);

                //    // 1️⃣ Try to find from main keyValuesForEligibility first
                //    var kv = keyValuesForEligibility.FirstOrDefault(kv =>
                //    {
                //        var paramName = parameters.FirstOrDefault(p => p.ParameterId == kv.Key)?.ParameterName;
                //        return !string.IsNullOrWhiteSpace(paramName) &&
                //               Normalize(paramName).Contains(normalizedTarget);
                //    });

                //    if (!Equals(kv, default(KeyValuePair<int, object?>)) && kv.Value != null)
                //        return kv.Value;

                //    // If not found, fallback to external API results
                //    foreach (var apiResult in externalApiResults)
                //    {
                //        var flattened = FlattenJson(apiResult.Value);

                //        var match = flattened.FirstOrDefault(f =>
                //            Normalize(f.Key).Contains(normalizedTarget));

                //        if (!Equals(match, default(KeyValuePair<string, object?>)) && match.Value != null)
                //            return match.Value;
                //    }

                //    return null;
                //}


                var scoreResult = new ScoringResult();
 
                 // Resolve Score and ProbabilityOfDefault dynamically
                 var scoreValue = GetBoundParameterValue("score", TenantId, keyValuesForEligibility, KeyValues);
                 if (scoreValue != null && int.TryParse(scoreValue.ToString(), out var parsedScore))
                 {
                     scoreResult.CustomerScore = parsedScore;
                 }
 
                 var pdValue = GetBoundParameterValue("probabilityofdefault", TenantId, keyValuesForEligibility, KeyValues);
                 if (pdValue != null && int.TryParse(pdValue.ToString(), out var parsedPd))
                 {
                     scoreResult.ProbabilityOfDefault = parsedPd;
                 }

                // Replace or add in keyValuesForEligibility
                //         var parameterss = _uow.ParameterRepository.Query()
                //.Where(p => new[] { "score", "creditscore", "probabilityofdefault", "pd" }.Contains(p.ParameterName!.ToLower()))
                //.Select(p => new { p.ParameterId, p.ParameterName })
                //.ToList();

                //         // Find IDs by name
                //         var scoreParam = parameterss.FirstOrDefault(p => p.ParameterName!.Equals("score", StringComparison.OrdinalIgnoreCase)
                //             || p.ParameterName.Equals("creditscore", StringComparison.OrdinalIgnoreCase));

                //         var pdParam = parameterss.FirstOrDefault(p => p.ParameterName!.Equals("probabilityofdefault", StringComparison.OrdinalIgnoreCase)
                //             || p.ParameterName.Equals("pd", StringComparison.OrdinalIgnoreCase));

                //         // Update values using parameter IDs
                //         if (scoreParam != null && keyValuesForEligibility.ContainsKey(scoreParam.ParameterId))
                //             keyValuesForEligibility[scoreParam.ParameterId] = scoreResult.CustomerScore;

                //if (pdParam != null && keyValuesForEligibility.ContainsKey(pdParam.ParameterId))
                //    keyValuesForEligibility[pdParam.ParameterId] = scoreResult.ProbabilityOfDefault;
                // Step 8: Perform eligibility evaluation
                var eligibilityResult = GetAllEligibleProducts(TenantId, keyValuesForEligibility);

                // Step 9: Save Evaluation History
                evaluation.EvaluationTimeStamp = DateTime.UtcNow;
                evaluation.CreditScore = scoreResult.CustomerScore;

                evaluation.TenantId = TenantId;
                evaluation.ProcessingTime = Math.Round(stopwatch.Elapsed.TotalSeconds, 2);
                var breRequestWithNames = keyValuesForEligibility.ToDictionary(
                    kv => parameters.FirstOrDefault(p => p.ParameterId == kv.Key)?.ParameterName ?? kv.Key.ToString(),
                    kv => kv.Value
                );

                // Serialize using parameter names
                evaluation.BreRequest = JsonSerializer.Serialize(breRequestWithNames, new JsonSerializerOptions
                {
                    WriteIndented = true
                }); _uow.EvaluationHistoryRepository.Update(evaluation);
                var result = TransformToResponse(eligibilityResult!, stopwatch.ElapsedMilliseconds, requestId, evaluation, scoreResult);
                evaluation.BreResponse = JsonSerializer.Serialize(result);
                var eligibleCount = result.EligibleProducts?.Count ?? 0;
                var nonEligibleCount = result.NonEligibleProducts?.Count ?? 0;

                var status = eligibleCount > 0 ? "Approved" : "Rejected";
                evaluation.Outcome = $"{status}: {eligibleCount} Eligible and {nonEligibleCount} Non Eligible Products";
                await _uow.CompleteAsync();

                // Transform final response
                var finalResponse = new BREIntegrationResponses
                {
                    RequestId = result.RequestId,
                    CustomerScore = result.CustomerScore,
                    ProbabilityOfDefault = result.ProbabilityOfDefault,
                    ProcessingTimeMs = result.ProcessingTimeMs,
                    Timestamp = DateTime.UtcNow,

                    EligibleProducts = [.. result.EligibleProducts!.Select(p => new EligibleProducts
                    {
                        ProductCode = p.ProductCode,
                        ProductName = p.ProductName,
                        MaxFinancingPercentage = p.MaxFinancingPercentage,
                        ProductCapAmount = p.ProductCapAmount

                    })],

                    NonEligibleProducts = result.NonEligibleProducts ?? [] // same model, keep as is
                };

                // Return new formatted response
                return finalResponse;
            }
            //catch (Exception ex)
            //{
            //    throw new BREIntegrationException($"Internal server error - RequestId: {requestId}", ex);
            //}
            finally
            {
                stopwatch.Stop();
            }
        }

        private BREIntegrationResponses? ValidateListTypeParameters(Dictionary<string, object> keyValues, int tenantId, string requestId)
        {
            var listTypeParameters = (
                from f in _uow.FactorRepository.Query()
                join p in _uow.ParameterRepository.Query()
                    on f.ParameterId equals p.ParameterId
                join c in _uow.ConditionRepository.Query()
                    on f.ConditionId equals c.ConditionId
                where f.TenantId == tenantId
                      && (
                            c.ConditionValue!.ToLower() == "in list" ||
                            c.ConditionValue.ToLower() == "not in list"
                         )
                select p.ParameterName
            )
            .Distinct()
            .ToList();

            foreach (var param in listTypeParameters)
            {
                // Check if the parameter name from DB already ends with "Name" or "Code"
                bool paramEndsWithName = param.EndsWith("Name", StringComparison.OrdinalIgnoreCase);
                bool paramEndsWithCode = param.EndsWith("Code", StringComparison.OrdinalIgnoreCase);

                string codeKey, nameKey;

                if (paramEndsWithName)
                {
                    // Parameter already ends with "Name", so we need its Code counterpart
                    string baseName = param[..^4]; // Remove "Name"
                    codeKey = baseName + "Code";
                    nameKey = param; // Keep original
                }
                else if (paramEndsWithCode)
                {
                    string baseName = param[..^4];
                    codeKey = param;
                    nameKey = baseName + "Name";
                }
                else
                {

                    codeKey = param + "Code";
                    nameKey = param + "Name";
                }

                // Now check what exists in the input
                var hasOriginalParam = keyValues.Keys
                    .Any(k => k.Equals(param, StringComparison.OrdinalIgnoreCase));

                var hasCode = keyValues.Keys
                    .Any(k => k.Equals(codeKey, StringComparison.OrdinalIgnoreCase));

                var hasName = keyValues.Keys
                    .Any(k => k.Equals(nameKey, StringComparison.OrdinalIgnoreCase));
                if (!hasOriginalParam && !hasCode && !hasName)
                {
                    continue;
                }
                // If  parameter (without suffix) is provided, reject it
                if (hasOriginalParam && !paramEndsWithName && !paramEndsWithCode)
                {
                    return new BREIntegrationResponses
                    {
                        RequestId = requestId,
                        Message = $"'{param}' is a list-type parameter. Please provide '{param}Code' and '{param}Name',.",
                        MandatoryParameters = [codeKey, nameKey]
                    };
                }

                // Check if we have the required pair
                bool hasRequiredPair = false;

                if (paramEndsWithName)
                {
                    hasRequiredPair = hasCode && hasOriginalParam;
                }
                else if (paramEndsWithCode)
                {
                    hasRequiredPair = hasOriginalParam && hasName;
                }
                else
                {
                    hasRequiredPair = hasCode && hasName;
                }

                if (!hasRequiredPair)
                {
                    return new BREIntegrationResponses
                    {
                        RequestId = requestId,
                        Message = $"'{param}' is a list-type parameter. You need to provide both Code and Name values.",
                        MandatoryParameters = [codeKey, nameKey]
                    };
                }

                if (hasCode)
                {
                    var codeValue = keyValues.First(k => k.Key.Equals(codeKey, StringComparison.OrdinalIgnoreCase))
                        .Value?.ToString();
                    if (string.IsNullOrWhiteSpace(codeValue))
                    {
                        return new BREIntegrationResponses
                        {
                            RequestId = requestId,
                            Message = $"'{codeKey}' cannot be empty.",
                            MandatoryParameters = [codeKey, nameKey]
                        };
                    }
                }

                if (hasName)
                {
                    var nameValue = keyValues.First(k => k.Key.Equals(nameKey, StringComparison.OrdinalIgnoreCase))
                        .Value?.ToString();
                    if (string.IsNullOrWhiteSpace(nameValue))
                    {
                        return new BREIntegrationResponses
                        {
                            RequestId = requestId,
                            Message = $"'{nameKey}' cannot be empty.",
                            MandatoryParameters = [codeKey, nameKey]
                        };
                    }
                }
                var factor = _uow.FactorRepository.Query()
                    .FirstOrDefault(f => f.TenantId == tenantId && f.ParameterId ==
                        _uow.ParameterRepository.Query().First(p => p.ParameterName == param).ParameterId);

                var listName = factor?.Value1?.Trim();

                if (string.IsNullOrEmpty(listName))
                    continue;

                var list = _uow.ManagedListRepository.Query()
                    .FirstOrDefault(l => l.ListName == listName);

                if (list == null)
                    continue;

                var listId = list.ListId;

                if (hasCode)
                {
                    var codeValue = keyValues.First(k => k.Key.Equals(codeKey, StringComparison.OrdinalIgnoreCase))
                        .Value?.ToString();

                    var nameValue = keyValues.First(k => k.Key.Equals(nameKey, StringComparison.OrdinalIgnoreCase))
                        .Value?.ToString();

                    if (!string.IsNullOrWhiteSpace(codeValue))
                    {
                        // Check if code exists in the list with a different name
                        var duplicateCodeDifferentName = _uow.ListItemRepository.Query()
                            .Any(li => li.ListId == listId &&
                                       li.Code.ToLower() == codeValue!.ToLower() &&
                                       (li.ItemName ?? "").ToLower() != (nameValue ?? "").ToLower());

                        if (duplicateCodeDifferentName)
                        {
                            return new BREIntegrationResponses
                            {
                                RequestId = requestId,
                                Message = $"Duplicate code '{codeValue}' already exists in list '{listName}' with a different Item name.",
                                MandatoryParameters = [codeKey, nameKey]
                            };
                        }
                    }
                }
            }

            return null;
        }


        private static Dictionary<string, object?> FlattenJson(object? obj, string prefix = "")
        {
            var result = new Dictionary<string, object?>();
            if (obj == null) return result;

            if (obj is JsonElement jsonElement)
            {
                switch (jsonElement.ValueKind)
                {
                    case JsonValueKind.Object:
                        foreach (var property in jsonElement.EnumerateObject())
                        {
                            // keep prefix chain
                            var nested = FlattenJson(property.Value,
                                string.IsNullOrEmpty(prefix) ? property.Name : $"{property.Name}");

                            foreach (var kv in nested)
                                result[kv.Key] = kv.Value;
                        }
                        break;

                    case JsonValueKind.Array:
                        int index = 0;
                        foreach (var element in jsonElement.EnumerateArray())
                        {
                            var nested = FlattenJson(element, $"{index}");
                            foreach (var kv in nested)
                                result[kv.Key] = kv.Value;
                            index++;
                        }
                        break;

                    default:
                        result[prefix.Trim('_')] = jsonElement.ToString();
                        break;
                }
            }
            else if (obj is Dictionary<string, object?> dict)
            {
                foreach (var kv in dict)
                {
                    var nested = FlattenJson(kv.Value,
                        string.IsNullOrEmpty(prefix) ? kv.Key : $"{kv.Key}");

                    foreach (var nestedKv in nested)
                        result[nestedKv.Key] = nestedKv.Value;
                }
            }
            else
            {
                result[prefix.Trim('_')] = obj;
            }

            return result;
        }



        private static int? TryParseNullableInt(string? input)
        {
            return int.TryParse(input, out var result) ? result : (int?)null;
        }
        private static DateTime? TryParseNullableDateTime(string? input)
        {
            return DateTime.TryParse(input, out var result) ? result : (DateTime?)null;
        }

        private static decimal? TryParseNullableDecimal(string? input)
        {
            return decimal.TryParse(input, out var result) ? result : (decimal?)null;
        }
        public async Task<Dictionary<string, Dictionary<string, object>>> CallAllExternalApisDynamic(Dictionary<int, object> inputKeyValues, EvaluationHistory Evalution)
        {
            var results = new Dictionary<string, Dictionary<string, object>>();

            var apis = _uow.NodeApiRepository.GetAll().Where(api => api.IsActive).ToList();
            var nodes = _uow.NodeModelRepository.GetAll().ToList();

            var activeApis = (from api in apis
                              join node in nodes on api.NodeId equals node.NodeId
                              orderby api.ExecutionOrder
                              select new
                              {
                                  api.Apiid,
                                  api.Apiname,
                                  api.HttpMethodType,
                                  FullUrl = node.NodeUrl + "/" + api.Apiname,
                                  api.RequestBody,
                                  api.RequestParameters,
                                  Headers = api.Header
                              }).ToList();
            var allInternalParams = _uow.ParameterRepository.GetAll().ToList();

            foreach (var api in activeApis)
            {
                try
                {
                    // Step 2: Get API parameters & mappings
                    var parameters = _uow.ApiParametersRepository.GetAll().Where(p => p.ApiId == api.Apiid).ToList();
                    var mappings = _uow.ApiParameterMapsRepository.GetAll()
                        .Where(m => parameters.Select(p => p.ApiParamterId).Contains(m.ApiParameterId))
                        .ToList();

                    var requestBody = new Dictionary<string, object>();

                    // Step 3: Build API payload dynamically
                    foreach (var param in parameters.Where(p =>
                     p.ParameterDirection.Equals("Input", StringComparison.OrdinalIgnoreCase)))
                    {
                        object? value = null;

                        var map = mappings.FirstOrDefault(m => m.ApiParameterId == param.ApiParamterId);

                        if (map != null && inputKeyValues.TryGetValue(map.ParameterId, out var mappedVal))
                        {
                            value = mappedVal;
                        }
                        else
                        {

                            var normalizedParamName = Normalize(param.ParameterName);
                            var matchedInternal = allInternalParams.FirstOrDefault(p =>
                                Normalize(p.ParameterName) == normalizedParamName);

                            if (matchedInternal != null && inputKeyValues.TryGetValue(matchedInternal.ParameterId, out var internalVal))
                            {
                                value = internalVal;
                            }
                            else if (inputKeyValues.TryGetValue(param.ApiParamterId, out var directVal))
                            {
                                value = directVal; // fallback direct
                            }
                            else
                            {
                                value = param.DefaultValue; // fallback default
                            }
                        }

                        if (value is string strVal && string.IsNullOrWhiteSpace(strVal))
                            continue;

                        requestBody[param.ParameterName] = value ?? string.Empty;
                    }

                    //if (api.FullUrl.Contains("breintegration"))
                    //    continue;

                    //  Call API dynamically
                    var response = await CallExternalApiAsync(api.FullUrl, api.HttpMethodType, requestBody, api.Apiid, Evalution, api.Headers);

                    //  Deserialize response JSON
                    var outputData = JsonSerializer.Deserialize<Dictionary<string, object>>(response) ?? [];
                    results[api.Apiname] = outputData;
                }
                catch (Exception ex)
                {
                    results[api.Apiname] = new Dictionary<string, object> { { "Error", ex.Message } };
                }
            }

            return results;
        }
        private static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return new string([.. input.Where(c => !char.IsWhiteSpace(c) && c != '_' && c != '-' && c != '.')])
                .ToLowerInvariant();
        }
        public async Task<string> CallExternalApiAsync(string url, string httpMethod, object? payload, int nodeApiId, EvaluationHistory? evaluation = null, string? headersJson = null)
        {
            using var client = new HttpClient();
            HttpResponseMessage response;

            if (!string.IsNullOrWhiteSpace(headersJson))
            {
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersJson);

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        var key = header.Key?.Trim() ?? "";
                        var value = header.Value?.Trim() ?? "";

                        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                            continue;

                        if (key.Equals("x-api-key", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!client.DefaultRequestHeaders.Contains("x-api-key"))
                                client.DefaultRequestHeaders.Add("x-api-key", value);
                        }
                        else
                        {
                            if (!client.DefaultRequestHeaders.Contains(key))
                                client.DefaultRequestHeaders.Add(key, value);
                        }
                    }
                }
            }

            string responseBody = "{}";

            try
            {
                Dictionary<string, object>? rawPayload = null;

                if (payload != null)
                {
                    if (payload is JsonElement jsonElement)
                        rawPayload = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.GetRawText());
                    else if (payload is Dictionary<string, object> dict)
                        rawPayload = dict;
                    else
                        rawPayload = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(payload));
                }

                var normalizedPayload = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                if (rawPayload != null)
                {
                    foreach (var kv in rawPayload)
                    {
                        var key = kv.Key?.Trim().Replace(" ", string.Empty) ?? string.Empty;
                        normalizedPayload[key] = kv.Value;
                    }
                }

                string requestJson = JsonSerializer.Serialize(normalizedPayload);

                // Store request in EvaluationHistory
                //if (evaluation != null)
                //{
                //    if (url.Contains("simah", StringComparison.OrdinalIgnoreCase))
                //        evaluation.SIMAHApiRequest = requestJson;
                //    else if (url.Contains("mozn", StringComparison.OrdinalIgnoreCase))
                //        evaluation.MoznApiRequest = requestJson;
                //    else if (url.Contains("flip", StringComparison.OrdinalIgnoreCase))
                //        evaluation.FlipApiRequest = requestJson;
                //    else if (url.Contains("Yaqeen", StringComparison.OrdinalIgnoreCase))
                //        evaluation.YaqeenApiRequest = requestJson;
                //    else if (url.Contains("Future", StringComparison.OrdinalIgnoreCase))
                //        evaluation.FutureWorksApiRequest = requestJson;
                //}

                if (httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    if (normalizedPayload.Count > 0)
                    {
                        var query = string.Join("&", normalizedPayload.Select(kv =>
                            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value?.ToString() ?? string.Empty)}"));
                        url = url.Contains('?') ? $"{url}&{query}" : $"{url}?{query}";
                    }
                    response = await client.GetAsync(url);
                }
                else
                {
                    var json = (normalizedPayload.Count > 0) ? JsonSerializer.Serialize(normalizedPayload) : "{}";
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = httpMethod.ToUpper() switch
                    {
                        "POST" => await client.PostAsync(url, content),
                        "PUT" => await client.PutAsync(url, content),
                        "DELETE" => await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url) { Content = content }),
                        _ => await client.PostAsync(url, content)
                    };
                }

                responseBody = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    responseBody = $"No response returned. Status: {(int)response.StatusCode} {response.ReasonPhrase}";
                }
                //// Store response in EvaluationHistory
                //if (evaluation != null)
                //{
                //    if (url.Contains("simah", StringComparison.OrdinalIgnoreCase))
                //        evaluation.SIMAHApiResponse = responseBody;
                //    else if (url.Contains("mozn", StringComparison.OrdinalIgnoreCase))
                //        evaluation.MoznApiResponse = responseBody;
                //    else if (url.Contains("flip", StringComparison.OrdinalIgnoreCase))
                //        evaluation.FlipApiResponse = responseBody;
                //    else if (url.Contains("Yaqeen", StringComparison.OrdinalIgnoreCase))
                //        evaluation.YaqeenApiResponse = responseBody;
                //    else if (url.Contains("future", StringComparison.OrdinalIgnoreCase))
                //        evaluation.FutureWorksApiResponse = responseBody;
                //}

                if (_uow.IntegrationApiEvaluationRepository != null && evaluation != null)
                {
                    var log = new IntegrationApiEvaluation
                    {
                        NodeApiId = nodeApiId,
                        ApiResponse = responseBody,
                        ApiRequest = requestJson,
                        EvaluationHistoryId = evaluation.EvaluationHistoryId, // link to main evaluation
                        EvaluationTimeStamp = DateTime.Now
                    };
                    _uow.IntegrationApiEvaluationRepository.Add(log);
                    await _uow.CompleteAsync();
                }

                if (!response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Serialize(new
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        Url = url,
                        Error = responseBody
                    });
                }

                return string.IsNullOrWhiteSpace(responseBody) ? "{}" : responseBody;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling external API. Method: {HttpMethod}, URL: {Url}", httpMethod, url);

                return JsonSerializer.Serialize(new
                {
                    Success = false,
                    Error = ex.Message,
                    Url = url
                });
            }
        }



        public async Task<string> CallExternalApiWithMappingAsync(DynamicApiRequest request)
        {
            // Step 1: Get API configuration
            var apiInfo = (from api in _uow.NodeApiRepository.GetAll()
                           join node in _uow.NodeModelRepository.GetAll() on api.NodeId equals node.NodeId
                           where (node.NodeUrl + "/" + api.Apiname) == request.Url
                           select new
                           {
                               api.Apiid,
                               api.Apiname,
                               api.HttpMethodType,
                               FullUrl = node.NodeUrl + "/" + api.Apiname,
                               Headers = api.Header
                           }).FirstOrDefault();

            Dictionary<string, object> mappedPayload;
            string headersJson;
            string httpMethod;

            if (apiInfo != null)
            {
                // --- Configuration exists: use mapping logic ---

                // Step 2: Load parameter definitions and mappings
                var parameters = _uow.ApiParametersRepository.GetAll()
                    .Where(p => p.ApiId == apiInfo.Apiid)
                    .ToList();

                var mappings = _uow.ApiParameterMapsRepository.GetAll()
                    .Where(m => parameters.Select(p => p.ApiParamterId).Contains(m.ApiParameterId))
                    .ToList();

                var internalParams = _uow.ParameterRepository.GetAll().ToList();

                // Step 3: Build payload using mapping rules
                mappedPayload = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var param in parameters.Where(p => p.ParameterDirection.Equals("Input", StringComparison.OrdinalIgnoreCase)))
                {
                    object? value = null;
                    var map = mappings.FirstOrDefault(m => m.ApiParameterId == param.ApiParamterId);

                    if (map != null)
                    {
                        var internalParam = internalParams.FirstOrDefault(p => p.ParameterId == map.ParameterId);
                        if (internalParam != null && TryGetPayloadValue(request.Payload, internalParam.ParameterName!, out var mappedVal))
                            value = mappedVal;
                        else if (TryGetPayloadValue(request.Payload, param.ParameterName, out var byApiNameVal))
                            value = byApiNameVal;
                        else
                            value = param.DefaultValue;
                    }
                    else
                    {
                        if (TryGetPayloadValue(request.Payload, param.ParameterName, out var directVal))
                            value = directVal;
                        else
                        {
                            var normName = NormalizeKey(param.ParameterName);
                            var matchedInternal = internalParams.FirstOrDefault(p => NormalizeKey(p.ParameterName) == normName);
                            if (matchedInternal != null && TryGetPayloadValue(request.Payload, matchedInternal.ParameterName!, out var internalVal))
                                value = internalVal;
                            else
                                value = param.DefaultValue;
                        }
                    }

                    if (value != null && !(value is string str && string.IsNullOrWhiteSpace(str)))
                        mappedPayload[param.ParameterName] = value;
                }

                // Merge direct payload overrides
                if (request.Payload != null)
                {
                    foreach (var kv in request.Payload)
                        mappedPayload[kv.Key] = kv.Value;
                }

                // Headers & method from DB
                headersJson = !string.IsNullOrWhiteSpace(apiInfo.Headers)
                    ? apiInfo.Headers
                    : (request.Headers != null ? JsonSerializer.Serialize(request.Headers) : null)!;

                httpMethod = request.HttpMethod ?? apiInfo.HttpMethodType;
            }
            else
            {
                mappedPayload = request.Payload ?? [];
                headersJson = request.Headers != null ? JsonSerializer.Serialize(request.Headers) : null!;
                httpMethod = request.HttpMethod ?? "POST"; // default POST if not specified
            }

            // Step 4: Call API
            var result = await CallExternalApiAsync(
                request.Url,
                httpMethod,
                mappedPayload,
                apiInfo!.Apiid,
                null,
                headersJson
            );

            return result;
        }

        #region  Helper Methods

        private static string NormalizeKey(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return new string([.. s!.Where(c => !char.IsWhiteSpace(c) && c != '_' && c != '-' && c != '.')])
                .ToLowerInvariant();
        }

        private bool TryGetPayloadValue(object? payloadObj, string name, out object? value)
        {
            value = null;
            if (payloadObj == null || string.IsNullOrWhiteSpace(name))
                return false;

            // Convert to dictionary safely
            if (payloadObj is not Dictionary<string, object> payload)
            {
                try
                {
                    var json = JsonSerializer.Serialize(payloadObj);
                    payload = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error converting payload object to dictionary for key: {KeyName}", name);
                    return false;
                }
            }

            if (payload.TryGetValue(name, out var directVal))
            {
                value = directVal;
                return true;
            }

            var keyCaseInsensitive = payload.Keys.FirstOrDefault(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));
            if (keyCaseInsensitive != null)
            {
                value = payload[keyCaseInsensitive];
                return true;
            }

            var normalizedName = NormalizeKey(name);
            var normalizedKey = payload.Keys.FirstOrDefault(k => NormalizeKey(k) == normalizedName);
            if (normalizedKey != null)
            {
                value = payload[normalizedKey];
                return true;
            }

            return false;
        }

        private object? GetBoundParameterValue(string systemParameterName, int tenantId, Dictionary<int, object> mappedKeyValues, Dictionary<string, object>? rawKeyValues = null)
        {
            // Check if there is a binding for this system parameter (case-insensitive)
            var binding = _uow.ParameterBindingRepository.Query()
                .Include(b => b.SystemParameter)
                .FirstOrDefault(b => b.TenantId == tenantId && b.SystemParameter!.Name.ToLower() == systemParameterName.ToLower());

            if (binding?.MappedParameterId != null)
            {
                // If bound, try to get value from the mapped parameter ID
                if (mappedKeyValues.TryGetValue(binding.MappedParameterId.Value, out var val))
                    return val;

                // Also check rawKeyValues by mapped parameter name if provided
                if (rawKeyValues != null)
                {
                    var mappedParam = _uow.ParameterRepository.Query().FirstOrDefault(p => p.ParameterId == binding.MappedParameterId);
                    if (mappedParam != null)
                    {
                        var match = rawKeyValues.FirstOrDefault(k => string.Equals(k.Key, mappedParam.ParameterName, StringComparison.OrdinalIgnoreCase));
                        if (!match.Equals(default(KeyValuePair<string, object>)))
                            return match.Value;
                    }
                }
            }

            //   try to find the value by the systemParameterName itself in rawKeyValues
            if (rawKeyValues != null)
            {
                var match = rawKeyValues.FirstOrDefault(k => string.Equals(k.Key, systemParameterName, StringComparison.OrdinalIgnoreCase));
                if (!match.Equals(default(KeyValuePair<string, object>)))
                    return match.Value;
            }

            // Also check if any parameter with this name (case-insensitive) exists in mappedKeyValues
            var defaultParam = _uow.ParameterRepository.Query()
                .FirstOrDefault(p => p.TenantId == tenantId && p.ParameterName!.ToLower() == systemParameterName.ToLower());
            if (defaultParam != null && mappedKeyValues.TryGetValue(defaultParam.ParameterId, out var mappedVal))
            {
                return mappedVal;
            }

            return null;
        }


        #endregion

     
        public async Task<object> CallYaqeenApi(string nationalId, EvaluationHistory evaluation)
        {
            //var yaqeenRequest = new { NationalId = nationalId };

            try
            {
                // Uncomment for real API call
                // var client = _httpClientFactory.CreateClient("YAGEEN");
                // var response = await client.GetAsync($"/api/traffic-violations/{nationalId}");
                // response.EnsureSuccessStatusCode();
                // var yaqeenResponse = await response.Content.ReadFromJsonAsync<YaqeenResponse>() ?? new YaqeenResponse { Status = "ERROR" };

                await Task.Delay(100); // simulate API

                // Mock response
                var yaqeenResponse = new YaqeenResponse
                {
                    Status = "SUCCESS",
                    CitizenInfo = new YageenCitizenInfo
                    {
                        SaudiId = nationalId,
                        Firstname = "Ahmed",
                        Fathername = "Mohammed",
                        Grandfathername = "Hassan",
                        SubtribeName = "Al-Saud",
                        Familyname = "Al-Faisal",
                        FullName = "Ahmed Mohammed Hassan Al-Faisal",
                        Gender = "Male",
                        DateOfBirthH = "1400-01-01",
                        PlaceOfBirth = "Riyadh",
                        LastSocialEventDate = "2023-01-01",
                        LastSocialEventGregDate = "2023-01-01",
                        SocialStatusCode = "01",
                        OccupationCode = "1001",
                        LifeStatus = "Alive",
                        IdIssueDate = "2015-01-01",
                        Idexpirydate = "2025-01-01",
                        TrafficViolationAmount = 0,
                        UpdateidIssueDate = 20220101,
                        DeathDate = null,
                        SocialStatusDescription = "Single"
                    },
                    Error = null,
                    Validations =
            [
                new() { PropertyName = "SaudiId", ValidationMessage = ["Valid National ID."] },
                new() { PropertyName = "FullName", ValidationMessage = ["Name format is correct."] }
            ]
                };

                // Save request + response + no error
                //  evaluation.YaqeenApiRequest = JsonSerializer.Serialize(

                //       yaqeenRequest
                //  );
                //  evaluation.YaqeenApiResponse = JsonSerializer.Serialize(

                //yaqeenResponse
                //  );

                return yaqeenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mock Yaqeen API for NationalId: {NationalId}", nationalId);

                // Save request + error
                //evaluation.YaqeenApiResponse = JsonSerializer.Serialize(

                //   ex

                //);

                return new YaqeenResponse
                {
                    Status = "ERROR",
                    CitizenInfo = null,
                    Error = new ErrorDetail
                    {
                        ErrorCode = "500",
                        ErrorDescription = ex.Message,
                        SubError = "Exception in mock API",
                        Validations = null
                    },
                    Validations = null
                };
            }
        }
        public async Task<object> CallMOZNApi(MOZNRequest request, EvaluationHistory evaluation)
        {
            //MOZNRequest moznRequest; // declare outside
            await Task.Delay(100);

            try
            {
                var moznResponse = new MOZNResponse
                {
                    Status = "SUCCESS",
                    CustomerScore = 750m,
                    ProbabilityOfDefault = 87m,
                    Id = Guid.NewGuid().ToString()
                };



                return moznResponse;

                //response.EnsureSuccessStatusCode();

                //return await response.Content.ReadFromJsonAsync<MOZNResponse>() ?? new MOZNResponse { Status = "ERROR" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MOZN API call for NationalIdNumber: {NationalId}", request.NationalId);

                //evaluation.MoznApiResponse = JsonSerializer.Serialize(
                //    ex
                //);

                return new MOZNResponse { Status = "ERROR" };
            }
        }

        public async Task<object> CallFLIPApi(string nationalId, EvaluationHistory evaluation)
        {
            //var flipRequest = new { NationalId = nationalId }; // request object
            await Task.Delay(100);

            try
            {
                // Save initial request
                //evaluation.FlipApi = JsonSerializer.Serialize(new { Request = flipRequest, Response = (object?)null, Error = (object?)null });

                var mockResponse = new FLIPResponse
                {
                    Status = "SUCCESS",
                    Data =
       [
           new() {
               TransactionID="11",
               JsonContent={ },
                Id = 1,

                  TotalNumberOfHoursWorked= 450,
                    Cyclicality = "High",
                    ActiveOnPlatform = true,
                    Segment = "Premium",
                IngestedAt = DateTime.UtcNow,
                NationalId = nationalId,
                PlatformName = "FreelancePortal",
                Source = "MockSource"
            },


       ]
                };

                // Save request + response + no error in evaluation history
                //        evaluation.FlipApiRequest = JsonSerializer.Serialize(flipRequest);
                //        evaluation.FlipApiResponse = JsonSerializer.Serialize(

                //    mockResponse
                //);

                return mockResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mock FLIP API for NationalId: {NationalId}", nationalId);

                // Save request + null response + error
                // evaluation.FlipApiResponse = JsonSerializer.Serialize(ex

                //);

                return new FLIPResponse
                {
                    Status = "ERROR",
                    Error = new ErrorDetail
                    {
                        ErrorCode = "MOCK_FAIL",
                        ErrorDescription = "Mocked FLIP API failed unexpectedly."
                    }
                };
            }
        }

        public async Task<object> CallFutureWorksApi(string nationalId, EvaluationHistory evaluation)
        {
            //var futureWorksRequest = new { NationalId = nationalId };
            await Task.Delay(100);
            try
            {
                // Mock response or actual API call
                var mockResponse = new FutureWorksResponse
                {
                    Status = "SUCCESS",
                    Data = new FutureWorksData
                    {
                        EnglishName = "Ahmed Ali",
                        NationalId = nationalId,
                        ArabicName = "أحمد علي",
                        NationalIdExpiryDate = DateTime.UtcNow.AddYears(3).ToString("yyyy-MM-dd"),
                        Gender = "Male",
                        Certificate =
                [
                    new() {
                CertificateNumber = "CERT-1001",
                CertificateIndustry="IT",
                CertificateValidity="30-06-2026",
                Activity="Freelace",
                Status = "ACTIVE",
                ExpiryDate = DateTime.UtcNow.AddYears(2).ToString("yyyy-MM-dd"),
                IssueDate = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"),
                RevokedAt = null,
                CanceledAt = null,
                Speciality = new Speciality
                {
                    Code = "IT-001",
                    Name = "تطوير البرمجيات",
                    NameEn = "Software Development",
                    Category = new CategoryFutureWorks
                    {
                        Code = "TECH",
                        Name = "التقنية",
                        NameEn = "Technology"
                    }
                }
            },


                ]
                    }
                };

                // Save request + response + no error
                //evaluation.FutureWorksApiRequest = JsonSerializer.Serialize(futureWorksRequest);
                //evaluation.FutureWorksApiResponse = JsonSerializer.Serialize(mockResponse);

                return mockResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mock FutureWorks API for NationalId: {NationalId}", nationalId);

                // Save request + null response + error
                //evaluation.FutureWorksApiResponse = JsonSerializer.Serialize(ex);

                return new FutureWorksResponse { Status = "ERROR" };
            }
        }

        public async Task<object> CallSIMAHApi(string NationalID, EvaluationHistory evaluation)
        {
            try
            {
                await Task.Delay(200);

                // Mock response based on document with comprehensive obligation details
                var mockResponse = new SIMAHResponse
                {
                    Success = true,
                    DefaultBalance = 0,
                    DefaultSector = null,
                    IsCourtCasesExists = false,
                    IsDefault = false,
                    MemberRefNumber = "638887010273482483",
                    Salary = 6200.0000m,
                    Status = "OK",
                    TotalMonthlyIns = 1307.00m,

                    // Obligation Details - Current loans and credit facilities
                    ObligationDetails = "Details",


                    // PDO Details - Post Dated Orders or payment arrangements
                    PDODetails = "PDO Details",


                    // Court Orders - Legal cases (empty since IsCourtCasesExists = false)
                    CourtOrder = "No",

                    // Bounced Cheques - Cheque-related issues
                    BouncedCheques = "Bounced Cheques"
                };


                return mockResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mock SIMAH API for NationalID: {NationalID}", NationalID);

                return new SIMAHResponse
                {
                    Success = false,
                    Status = "ERROR",
                    DefaultBalance = 0,
                    DefaultSector = null,
                    IsCourtCasesExists = false,
                    IsDefault = false,
                    MemberRefNumber = null,
                    Salary = 0,
                    TotalMonthlyIns = 0,
                    ObligationDetails = null,
                    PDODetails = null,
                    CourtOrder = null,
                    BouncedCheques = null
                };
            }
        }

        private BREIntegrationResponse TransformToResponse(EligibleAmountResults eligibilityResult, long processingTimeMs, string requestId, EvaluationHistory evaluation, ScoringResult scoringResult)
        {
            var eligibleProducts = new List<EligibleProduct>();
            var nonEligibleProducts = new List<NonEligibleProduct>();

            var allRejectionReasons = _uow.RejectionReasonRepository.Query().ToList();
            var allParameters = _uow.ParameterRepository.Query().ToList(); // Load all parameters

            // Store unique failure reasons
            var uniqueFailureReasons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Normalize skip failure reasons ONCE
            var skipFailureReasonsNormalized = new HashSet<string>(
                new[]
                {
                   "No Rule Match",
                   "Could not find eligible amount criteria.",
                   "Customer's score does not satisfy eligibility requirements."
                }.Select(NormalizeForMatch)
            );

            if (eligibilityResult.Products != null)
            {
                foreach (var product in eligibilityResult.Products)
                {
                    if (product.Iseligible)
                    {
                        eligibleProducts.Add(new EligibleProduct
                        {
                            ProductCode = product.ProductCode,
                            ProductName = product.ProductName,
                            MaxFinancingPercentage = product.MaximumProductCapPercentage,
                            EligibleAmount = product.EligibleAmount,
                            ProductCapAmount = product.ProductCapAmount
                        });
                    }
                    else
                    {
                        var nonEligible = new NonEligibleProduct
                        {
                            ProductCode = product.ProductCode,
                            ProductName = product.ProductName,
                            RejectionReasons = []
                        };

                        var messages = (product.Message ?? "")
                            .Split([',', '|', ';'], StringSplitOptions.RemoveEmptyEntries)
                            .Select(m => m.Trim())
                            .ToList();

                        foreach (var msg in messages)
                        {
                            string cleanedMsg = NormalizeForMatch(msg);

                            // Extract parameterId and factorName from message
                            var parts = msg.Split("Factor", StringSplitOptions.RemoveEmptyEntries);
                            string parameterIdStr = parts.Length > 0 ? parts[0].Trim() : "";
                            string factorName = parts.Length > 1 ? NormalizeForMatch(parts[1]) : "";
                            string numericOnly = ExtractNumericValue(parameterIdStr);

                            // Try to parse parameterId
                            bool hasParameterId = int.TryParse(numericOnly, out int parameterId);

                            if (hasParameterId)
                            {
                                var parameter = allParameters.FirstOrDefault(p => p.ParameterId == parameterId);
                                if (parameter != null && !string.IsNullOrEmpty(parameter.RejectionReasonCode))
                                {
                                    nonEligible.RejectionReasons.Add(new RejectionReason
                                    {
                                        Code = parameter.RejectionReasonCode,
                                        Description = parameter.RejectionReason ?? parameter.ParameterName
                                    });

                                    if (eligibleProducts.Count == 0)
                                    {
                                        uniqueFailureReasons.Add(parameter.RejectionReason ?? parameter.ParameterName!);
                                    }

                                    continue;
                                }
                            }

                            string normalizedParamId = NormalizeForMatch(parameterIdStr);
                            var matched = allRejectionReasons.FirstOrDefault(r =>
                            {
                                if (string.IsNullOrEmpty(r.Description))
                                    return false;

                                var normalizedDesc = NormalizeForMatch(r.Description);

                                return
                                    (!string.IsNullOrEmpty(normalizedParamId) && normalizedDesc.Contains(normalizedParamId)) ||
                                    (!string.IsNullOrEmpty(factorName) && normalizedDesc.Contains(factorName)) ||
                                    (!string.IsNullOrEmpty(cleanedMsg) && normalizedDesc.Contains(cleanedMsg));
                            });

                            if (matched != null)
                            {
                                nonEligible.RejectionReasons.Add(new RejectionReason
                                {
                                    Code = matched.Code,
                                    Description = matched.Description
                                });

                                if (eligibleProducts.Count == 0 &&
                                    !skipFailureReasonsNormalized.Contains(
                                        NormalizeForMatch(matched.Description!)
                                    ))
                                {
                                    uniqueFailureReasons.Add(matched.Description!);
                                }

                                continue;
                            }

                            if (skipFailureReasonsNormalized.Contains(cleanedMsg))
                            {
                                nonEligible.RejectionReasons.Add(new RejectionReason
                                {
                                    Code = "BRE_MESSAGE",
                                    Description = msg
                                });

                                if (eligibleProducts.Count == 0)
                                {
                                    uniqueFailureReasons.Add(msg);
                                }

                                continue;
                            }

                            var defaultReason = "Does not meet eligibility criteria.";

                            nonEligible.RejectionReasons.Add(new RejectionReason
                            {
                                Code = "ELIGIBILITY_FAILED",
                                Description = defaultReason
                            });

                            if (eligibleProducts.Count == 0)
                            {
                                uniqueFailureReasons.Add(defaultReason);
                            }
                        }

                        nonEligibleProducts.Add(nonEligible);
                    }
                }
            }

            evaluation.FailurReason = uniqueFailureReasons.Count > 0
                ? string.Join(", ", uniqueFailureReasons)
                : null;

            return new BREIntegrationResponse
            {
                CustomerScore = scoringResult.CustomerScore,
                ProbabilityOfDefault = scoringResult.ProbabilityOfDefault,
                EligibleProducts = eligibleProducts,
                NonEligibleProducts = nonEligibleProducts,
                ProcessingTimeMs = processingTimeMs,
                RequestId = requestId,
                Timestamp = DateTime.Now
            };
        }

        // Normalize string: remove punctuation, lowercase, trim
        private static string NormalizeForMatch(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return new string([.. input.Where(char.IsLetterOrDigit)])
                .ToLowerInvariant();
        }
        private static string ExtractNumericValue(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return new string([.. input.Where(char.IsDigit)]);
        }


        /// <summary>
        /// Extracts the parameter or factor name from a validation message
        /// Example: "Validation failed for condition: Blacklist Expected Blacklist = False"
        /// Returns "Blacklist"
        /// </summary>


        /// <summary>
        /// Normalize a string: remove punctuation, lowercase, and trim



        [GeneratedRegex("([a-z])([A-Z])")]
        private static partial Regex MyRegex1();
        [GeneratedRegex(@"[^\p{L}\p{N}]+")]
        private static partial Regex MyRegex2();
        [GeneratedRegex(@"[^\p{L}\p{N}]+")]
        private static partial Regex MyRegex3();
        [GeneratedRegex(@"\bOR\b", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex4();
        [GeneratedRegex(@"\bAND\b", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex5();
        [GeneratedRegex(@"^\s*(\d+)\s*(>=|<=|>|<|=|inlist|notinlist|!=|range)\s*(.*)\s*$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex6();



    }


    // Custom Exception
    public class BREIntegrationException : Exception
    {
        public string? RequestId { get; }

        public BREIntegrationException(string message) : base(message) { }

        public BREIntegrationException(string message, Exception innerException) : base(message, innerException) { }

        public BREIntegrationException(string message, string requestId) : base(message)
        {
            RequestId = requestId;
        }
    }
}
