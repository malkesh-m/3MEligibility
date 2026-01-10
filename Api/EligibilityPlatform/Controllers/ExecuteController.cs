using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for executing SOAP and REST API operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ExecuteController"/> class.
    /// </remarks>
    /// <param name="apiService">The API service.</param>
    [Route("api/execute")]
    [ApiController]
    public class ExecuteController(IApiService apiService) : ControllerBase
    {
        private readonly IApiService _apiService = apiService;

        /// <summary>
        /// Retrieves SOAP API details for the specified node ID.
        /// </summary>
        /// <param name="nodeId">The node ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the SOAP API details.</returns>
        [HttpGet("getsoapapis")]
        public async Task<IActionResult> GetSoapApis(int nodeId)
        {
            // Retrieves SOAP API details for the specified node ID
            var list = await _apiService.GetApiDetailsAsync(nodeId);
            // Returns success response with the retrieved SOAP API details
            return Ok(new ResponseModel { IsSuccess = true, Data = list });
        }

        /// <summary>
        /// Retrieves REST API details for the specified node ID.
        /// </summary>
        /// <param name="nodeId">The node ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the REST API details.</returns>
        [HttpGet("getrestapis")]
        public async Task<IActionResult> GetRestApis(int nodeId)
        {
            // Retrieves REST API details for the specified node ID
            var list = await _apiService.GetRestApiDetailsAsync(nodeId);
            // Returns success response with the retrieved REST API details
            return Ok(new ResponseModel { IsSuccess = true, Data = list });
        }

        /// <summary>
        /// Calls a SOAP API with the provided model.
        /// </summary>
        /// <param name="soapApiModel">The SOAP API model.</param>
        /// <returns>An <see cref="IActionResult"/> containing the response from the SOAP API.</returns>
        [HttpPost("soapapi")]
        public async Task<IActionResult> SoapApi(SoapApiModel soapApiModel)
        {
            // Validates the model state before proceeding
            if (!ModelState.IsValid)
            {
                // Returns bad request response if model validation fails
                return BadRequest(ModelState);
            }

            // Calls the SOAP API with the provided model
            var response = await _apiService.CallSoapApi(soapApiModel);
            // Returns success response with the SOAP API response data
            return Ok(new ResponseModel { IsSuccess = true, Data = response });
        }

        /// <summary>
        /// Calls a REST API with the provided model.
        /// </summary>
        /// <param name="model">The execute API model.</param>
        /// <returns>An <see cref="IActionResult"/> containing the response from the REST API.</returns>
        [HttpPost("restapi")]
        public async Task<IActionResult> RestApi(ExecuteApiModel model)
        {
            // Calls the REST API with the provided model
            var result = await _apiService.CallRestApi(model);
            if (result.IsSuccess)
            {
                // Returns success response with the REST API result
                return Ok(result);
            }
            // Returns bad request response if the REST API call failed
            return BadRequest(result);
        }
    }
}
