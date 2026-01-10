using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for executing API operations and retrieving API details.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiExcuteController"/> class.
    /// </remarks>
    /// <param name="apiExcuteService">The API execute service.</param>
    /// <param name="nodeApiService">The node API service.</param>
    /// <param name="parametersService">The API parameters service.</param>
    /// <param name="apiResponsesService">The API responses service.</param>
    [Route("api/apiexecute")]
    [ApiController]
    public class ApiExcuteController(IApiExcuteService apiExcuteService, INodeApiService nodeApiService, IApiParametersService parametersService, IApiResponsesService apiResponsesService) : ControllerBase
    {
        /// <summary>
        /// The API execute service instance for API execution operations.
        /// </summary>
        private readonly IApiExcuteService _apiExcuteService = apiExcuteService;

        /// <summary>
        /// The node API service instance for node-related API operations.
        /// </summary>
        private readonly INodeApiService _nodeApiService = nodeApiService;

        /// <summary>
        /// The API parameters service instance for parameter management operations.
        /// </summary>
        private readonly IApiParametersService _parametersService = parametersService;

        /// <summary>
        /// The API responses service instance for response management operations.
        /// </summary>
        private readonly IApiResponsesService _apiResponsesService = apiResponsesService;

        /// <summary>
        /// Retrieves REST API details for the specified node ID.
        /// </summary>
        /// <param name="nodeId">The node ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the REST API details.</returns>
        [HttpGet("getrestapis")]
        public async Task<IActionResult> GetRestApis(int nodeId)
        {
            /// <summary>
            /// Calls the service to retrieve REST API details for the specified node ID.
            /// </summary>
            var list = await _apiExcuteService.GetRestApiDetailsAsync(nodeId);

            /// <summary>
            /// Returns successful response with the retrieved REST API details.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = list });
        }
    }
}
