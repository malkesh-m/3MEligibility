using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing eligibility API operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EligibilityAPIController"/> class.
    /// </remarks>
    /// <param name="eligibilityApiService">The eligibility API service.</param>
    [Route("api/eligibilityaPI")]
    [ApiController]
    public class EligibilityAPIController(IEligibilityApiService eligibilityApiService) : ControllerBase
    {
        /// <summary>
        /// The eligibility API service instance for eligibility validation operations.
        /// </summary>
        private readonly IEligibilityApiService _eligibilityApiService = eligibilityApiService;

        /// <summary>
        /// Validates a product using the API for the specified user and product ID with provided key values.
        /// </summary>
        /// <param name="productId">The product ID to validate.</param>
        /// <param name="keyValues">The key-value pairs for validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the validation result.</returns>
        [HttpPost("validateproductApi")]
        public async Task<IActionResult> ValidateProductApi(int productId, Dictionary<int, object> keyValues)
        {
            /// <summary>
            /// Calls the service to validate a product using the API with the current user's ID, product ID, and key values.
            /// </summary>
            return Ok(await _eligibilityApiService.ValidAsyncAPI(User.GetUserId(), productId, keyValues));
        }
    }
}
