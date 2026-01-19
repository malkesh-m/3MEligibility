using EligibilityPlatform.Application.Constants;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing API responses operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiResponsesController"/> class.
    /// </remarks>
    /// <param name="apiResponses">The API responses service.</param>
    [Route("api/apiresponses")]
    [ApiController]
    public class ApiResponsesController(IApiResponsesService apiResponses) : ControllerBase
    {
        /// <summary>
        /// The API responses service instance for response operations.
        /// </summary>
        private readonly IApiResponsesService _apiResponses = apiResponses;

        /// <summary>
        /// Retrieves all API responses.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing all API responses.</returns>
        [HttpGet]
        public IActionResult Getall()
        {
            /// <summary>
            /// Returns successful response with all API responses.
            /// </summary>
            return Ok(new ResponseModel { Data = _apiResponses.GetAll(), Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves an API response by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API response.</param>
        /// <returns>An <see cref="IActionResult"/>
        /// containing the API response if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.ApiResponses.View)]

        [HttpGet("id")]
        public IActionResult Getbyid(int id)
        {
            /// <summary>
            /// Retrieves a specific API response by ID from the service.
            /// </summary>
            var result = _apiResponses.GetById(id);

            /// <summary>
            /// Checks if the API response was found.
            /// </summary>
            if (result == null)
            {
                /// <summary>
                /// Returns not found response when the API response does not exist.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }

            /// <summary>
            /// Returns successful response with the retrieved API response.
            /// </summary>
            return Ok(new ResponseModel { Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Adds a new API response.
        /// </summary>
        /// <param name="model">The <see cref="ApiResponsesCreateUpdateModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.ApiResponses.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(ApiResponsesCreateUpdateModel model)
        {
            /// <summary>
            /// Validates the model state before processing.
            /// </summary>
            if (!ModelState.IsValid)
            {
                /// <summary>
                /// Returns bad request response for invalid model state.
                /// </summary>
                return BadRequest(ModelState);
            }

            /// <summary>
            /// Calls the service to add a new API response.
            /// </summary>
            await _apiResponses.Add(model);

            /// <summary>
            /// Returns successful response indicating the API response was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing API response.
        /// </summary>
        /// <param name="model">The <see cref="ApiResponsesCreateUpdateModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.ApiResponses.Edit)]

        [HttpPut]
        public async Task<IActionResult> Update(ApiResponsesCreateUpdateModel model)
        {
            /// <summary>
            /// Validates the model state before processing.
            /// </summary>
            if (!ModelState.IsValid)
            {
                /// <summary>
                /// Returns bad request response for invalid model state.
                /// </summary>
                return BadRequest(ModelState);
            }

            /// <summary>
            /// Calls the service to update an existing API response.
            /// </summary>
            await _apiResponses.Update(model);

            /// <summary>
            /// Returns successful response indicating the API response was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes an API response by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API response to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.ApiResponses.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete an API response.
            /// </summary>
            await _apiResponses.Remove(id);

            /// <summary>
            /// Returns successful response indicating the API response was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
