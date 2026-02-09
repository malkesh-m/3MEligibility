using System.Text.Json;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Attributes;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{

    /// <summary>
    /// API controller for handling eligibility-related operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EligibilityController"/> class.
    /// </remarks>
    /// <param name="eligibilityService">The eligibility service.</param>
    /// <param name="eligibleProductsService">The eligible products service.</param>
    [Route("api/eligibility")]
    [ApiController]
    [AllowAnonymous]

    public class EligibilityController(IEligibilityService eligibilityService, IEligibleProductsService eligibleProductsService, ILogger<EligibilityController> logger) : ControllerBase
    {
        private readonly IEligibilityService _eligibilityService = eligibilityService;
        private readonly IEligibleProductsService _eligibleProductsService = eligibleProductsService;
        private readonly ILogger<EligibilityController> _logger = logger;

        /// <summary>
        /// Validates a product asynchronously for the current user and product ID with provided key values.
        /// </summary>
        /// <param name="productId">The product ID to validate.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the validation result.</returns>
        [HttpPost("validateproduct")]
        public async Task<IActionResult> ValidateProduct(int productId, Dictionary<int, object> keyValues)
        {
            /// <summary>
            /// Returns the result of the product validation operation.
            /// </summary>
            return Ok(await _eligibilityService.ValidAsync(User.GetUserId(), productId, keyValues));
        }

        /// <summary>
        /// Gets the best fit products asynchronously for the current user with provided key values.
        /// </summary>
        /// <param name="keyValues">The key-value pairs for evaluation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the best fit products.</returns>
        [HttpPost("getbestfitproducts")]
        public async Task<IActionResult> GetBestFitProducts(Dictionary<int, object> keyValues)
        {
            /// <summary>
            /// Returns the best fit products for the current user.
            /// </summary>
            return Ok(await _eligibilityService.GetBestFitProductsAsync(User.GetUserId(), keyValues));
        }

        /// <summary>
        /// Gets all eligible products for the specified entity and key values.
        /// </summary>
        /// <param name="keyValues">The key-value pairs for eligibility check.</param>
        /// <param name="EntityId">The entity ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing all eligible products.</returns>
        [HttpPost("geteligibleproducts"), AllowAnonymous]
        public IActionResult GetEligibleProducts(Dictionary<int, object> keyValues, int EntityId)
        {
            /// <summary>
            /// Returns all eligible products for the specified entity.
            /// </summary>
            return Ok(_eligibleProductsService.GetAllEligibleProducts(EntityId, keyValues));
        }
        //[ApiKeyAuth]

        [HttpPost("breintegrationalignment")]
        public async Task<IActionResult> BREIntegrationAlignment([FromBody] Dictionary<string, object> KeyValues, [FromQuery] int TenantId, [FromHeader] string? RequestId)
        {
          
                var result = await _eligibleProductsService.ProcessBREIntegration(KeyValues, TenantId, RequestId);
                return Ok(result);
            
         }
        [ApiKeyAuth]
        // POST: api/moznapi/CallMOZNApi
        [HttpGet("moznapi")]
        public async Task<IActionResult> CallMOZNApiEndpoint([FromQuery] MOZNRequest request)
        {
            var evaluation = new EvaluationHistory();

            try
            {
                // Call your static service method
                var result = await _eligibleProductsService.CallMOZNApi(request, evaluation);

                // Optionally: save evaluation history to DB

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "ERROR", ex.Message });
            }
        }
        // POST: api/flipapi/CallFLIPApi
        [ApiKeyAuth]

        [HttpGet("flipapi")]
        public async Task<IActionResult> CallFLIPApiEndpoint(string NationalId)
        {

            var evaluation = new EvaluationHistory();

            try
            {
                var result = await _eligibleProductsService.CallFLIPApi(NationalId, evaluation);

                // Optionally: save evaluation history to DB

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "ERROR",
                    ex.Message
                });
            }
        }
        [ApiKeyAuth]

        [HttpGet("yaqeeneligibility")]
        public async Task<IActionResult> CheckYaqeenEligibility(string NationalId)
        {
            try
            {
                var evaluationHistory = new EvaluationHistory
                {

                    // Initialize other properties as needed
                };

                var result = await _eligibleProductsService.CallYaqeenApi(NationalId, evaluationHistory);

                // You might want to save the evaluationHistory to database here
                // await _context.EvaluationHistories.AddAsync(evaluationHistory);
                // await _context.SaveChangesAsync();

                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }
        }
        [ApiKeyAuth]

        [HttpGet("futurework")]
        public async Task<IActionResult> CallFutureWork(string NationalId)
        {
            try
            {
                var evaluationHistory = new EvaluationHistory
                {

                    // Initialize other properties as needed
                };

                var result = await _eligibleProductsService.CallFutureWorksApi(NationalId, evaluationHistory);

                // You might want to save the evaluationHistory to database here
                // await _context.EvaluationHistories.AddAsync(evaluationHistory);
                // await _context.SaveChangesAsync();

                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }

        }
        [ApiKeyAuth]

        [HttpGet("simah")]
        public async Task<IActionResult> CallSimah(string NationalId)
        {
            try
            {
                var evaluationHistory = new EvaluationHistory
                {

                    // Initialize other properties as needed
                };

                var result = await _eligibleProductsService.CallSIMAHApi(NationalId, evaluationHistory);

                // You might want to save the evaluationHistory to database here
                // await _context.EvaluationHistories.AddAsync(evaluationHistory);
                // await _context.SaveChangesAsync();

                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Error = ex.Message });
            }

        }
        [HttpPost("callexternalapi")]
        public async Task<IActionResult> CallExternalApi([FromBody] DynamicApiRequest request)
        {
            try
            {
                var result = await _eligibleProductsService.CallExternalApiWithMappingAsync(request);

                // Try to format JSON nicely if it’s valid
                try
                {
                    var json = JsonSerializer.Deserialize<JsonElement>(result);
                    return Ok(json);
                }
                catch
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}

