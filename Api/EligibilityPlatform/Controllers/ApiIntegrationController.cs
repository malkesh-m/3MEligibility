using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing API integration operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiIntegrationController"/> class.
    /// </remarks>
    /// <param name="apiIntegrationService">The API integration service.</param>
    /// <param name="nodeApiService">The node API service.</param>
    /// <param name="nodeService">The node service.</param>
    [Route("api/apiintegration")]
    [ApiController]
    public class ApiIntegrationController(IApiIntegrationService apiIntegrationService, INodeApiService nodeApiService, INodeService nodeService) : ControllerBase
    {
        /// <summary>
        /// The API integration service instance for API integration operations.
        /// </summary>
        private readonly IApiIntegrationService _apiIntegrationService = apiIntegrationService;

        /// <summary>
        /// The node API service instance for node-related API operations.
        /// </summary>
        private readonly INodeApiService _nodeApiService = nodeApiService;

        /// <summary>
        /// The node service instance for node management operations.
        /// </summary>
        private readonly INodeService _nodeService = nodeService;

        /// <summary>
        /// Retrieves REST API details for the specified API URL.
        /// </summary>
        /// <param name="ApiUrl">The API URL to retrieve details for.</param>
        /// <returns>An <see cref="IActionResult"/> containing the API details.</returns>
        [HttpGet("getrestapis")]
        public async Task<IActionResult> GetRestApis(string ApiUrl)
        {
            /// <summary>
            /// Calls the service to retrieve API details for the specified URL.
            /// </summary>
            var list = await _apiIntegrationService.GetApiDetailsAsync(ApiUrl);

            /// <summary>
            /// Returns successful response with the retrieved API details.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = list });
        }

        ///// <summary>
        ///// Analyzes an API endpoint with the specified request parameters.
        ///// </summary>
        ///// <param name="request">The API analysis request containing URL, method, and body.</param>
        ///// <returns>An <see cref="IActionResult"/> containing the analysis results.</returns>
        //[HttpPost("analyze")]
        //public async Task<IActionResult> AnalyzeApi([FromBody] ApiAnalysisRequest request)
        //{
        //    /// <summary>
        //    /// Validates that the API URL is not empty or whitespace.
        //    /// </summary>
        //    if (string.IsNullOrWhiteSpace(request.ApiUrl))
        //        /// <summary>
        //        /// Returns bad request response for empty API URL.
        //        /// </summary>
        //        return BadRequest("API URL cannot be empty.");

        //    /// <summary>
        //    /// Wraps the API analysis in a try-catch block for error handling.
        //    /// </summary>
        //    try
        //    {
        //        /// <summary>
        //        /// Calls the service to analyze the API endpoint.
        //        /// </summary>
        //        var result = await _apiIntegrationService.AnalyzeApiAsync(request.ApiUrl, new HttpMethod(request.Method), request.RequestBody);
        //        
        //        /// <summary>
        //        /// Returns successful response with the analysis results.
        //        /// </summary>
        //        return Ok(result);
        //    }
        //    /// <summary>
        //    /// Catches any exceptions that occur during API analysis.
        //    /// </summary>
        //    catch (
        //
        //
        //
        //    Exception ex)
        //    {
        //        /// <summary>
        //        /// Returns server error response with the exception message.
        //        /// </summary>
        //        return StatusCode(500, $"Error analyzing API: {ex.Message}");
        //    }
        //}

        /// <summary>
        /// Executes an API request for the specified node ID and request body.
        /// </summary>
        /// <param name="id">The node ID.</param>
        /// <param name="requestBody">The request body containing parameters and request data.</param>
        /// <returns>An <see cref="ActionResult{object}"/> containing the result of the API execution.</returns>
        [HttpPost("{id}/execute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> Execute(int id, [FromBody] ExecuteRequestBody? requestBody = null)
        {
            /// <summary>
            /// Retrieves node API details for the specified node ID.
            /// </summary>
            var NodeApiDetails = _nodeApiService.GetByNodeIdSingle(id);

            /// <summary>
            /// Retrieves API details for the current user's entity and specified node ID.
            /// </summary>
            var apiDetails = _nodeService.GetById(User.GetTenantId(), id);

            /// <summary>
            /// Extracts parameters from the request body or initializes an empty dictionary.
            /// </summary>
            var parameters = requestBody?.Parameters ?? [];

            /// <summary>
            /// Calls the service to execute the API request with the provided details and parameters.
            /// </summary>
            var result = await _apiIntegrationService.ExecuteRequestAsync(apiDetails, NodeApiDetails, parameters);

            /// <summary>
            /// Returns successful response with the API execution result.
            /// </summary>
            return Ok(result);
        }

        //[HttpPost("testapi")]
        //public async Task<IActionResult> TestApi([FromBody] TestApiRequest request)
        //{
        //    try
        //    {
        //        var result = await _apiIntegrationService.TestApiAsync(request);
        //        return Ok(new { success = true, result });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { success = false, error = ex.Message });
        //    }
        //}
    }

    /// <summary>
    /// Represents a sample request body for demonstration purposes.
    /// </summary>
    public class SampleRequestBody
    {
        /// <summary>
        /// Gets or sets the request body content.
        /// </summary>
        public string? RequestBody { get; set; }
    }

    /// <summary>
    /// Represents the request body for executing an API request.
    /// </summary>
    public class ExecuteRequestBody
    {
        /// <summary>
        /// Gets or sets the parameters for the API request.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the request data as a string.
        /// </summary>
        public string? RequestData { get; set; }
    }
}
