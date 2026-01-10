using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing parameter mapping operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ParametersMapController"/> class.
    /// </remarks>
    /// <param name="paramtersMapService">The parameters map service.</param>
    [Route("api/parametersmap")]
    [ApiController]
    public class ParametersMapController(IParamtersMapService paramtersMapService) : ControllerBase
    {
        private readonly IParamtersMapService _paramtersMapService = paramtersMapService;

        /// <summary>
        /// Retrieves all parameter mappings.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of parameter mappings.</returns>
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all parameter mappings
            List<ParamtersMapModel> result = _paramtersMapService.GetAll();
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a parameter mapping by API ID, node ID, and parameter ID.
        /// </summary>
        /// <param name="apiId">The API ID.</param>
        /// <param name="nodeId">The node ID.</param>
        /// <param name="parameterId">The parameter ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the parameter mapping if found; otherwise, not found.</returns>
        [HttpGet("getbyid")]
        public IActionResult Get(int apiId, int nodeId, int parameterId)
        {
            // Retrieves a parameter mapping by API ID, node ID, and parameter ID
            var result = _paramtersMapService.GetById(apiId, nodeId, parameterId);
            // Checks if the mapping was found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when mapping doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new parameter mapping.
        /// </summary>
        /// <param name="paramtersMap">The parameter mapping model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Post(ParamtersMapModel paramtersMap)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new parameter mapping
            await _paramtersMapService.Add(paramtersMap);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing parameter mapping.
        /// </summary>
        /// <param name="paramtersMap">The parameter mapping model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPut]
        public async Task<IActionResult> Put(ParamtersMapModel paramtersMap)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing parameter mapping
            await _paramtersMapService.Update(paramtersMap);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a parameter mapping by API ID, node ID, and parameter ID.
        /// </summary>
        /// <param name="apiId">The API ID.</param>
        /// <param name="nodeId">The node ID.</param>
        /// <param name="parameterId">The parameter ID.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int apiId, int nodeId, int parameterId)
        {
            // Deletes the parameter mapping by API ID, node ID, and parameter ID
            await _paramtersMapService.Delete(apiId, nodeId, parameterId);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
