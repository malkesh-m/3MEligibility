using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Constants;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing API detail operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiDetailController"/> class.
    /// </remarks>
    /// <param name="apiDetailService">The API detail service.</param>
    /// 

    [Route("api/apidetail")]
    [ApiController]
    public class ApiDetailController(IApiDetailService apiDetailService) : ControllerBase
    {
        /// <summary>
        /// The API detail service instance for business logic operations.
        /// </summary>
        private readonly IApiDetailService _apiDetailService = apiDetailService;

        /// <summary>
        /// Retrieves all API details.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of API details.</returns>
        ///  [HttpPost("TestApi")]
        [Authorize(Policy = Permissions.ApiDetails.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            /// <summary>
            /// Retrieves all API details from the service.
            /// </summary>
            List<ApiDetailListModel> result = _apiDetailService.GetAll();

            /// <summary>
            /// Returns successful response with the retrieved data.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves an API detail by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API detail.</param>
        /// <returns>An <see cref="IActionResult"/> containing the API detail if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.ApiDetails.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /// <summary>
            /// Retrieves a specific API detail by ID from the service.
            /// </summary>
            var result = _apiDetailService.GetById(id);

            /// <summary>
            /// Checks if the API detail was found.
            /// </summary>
            if (result != null)
            {
                /// <summary>
                /// Returns successful response with the retrieved data.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when the API detail does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new API detail.
        /// </summary>
        /// <param name="apiDetail">The API detail model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiDetails.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(ApiDetailCreateotUpdateModel apiDetail)
        {
            /// <summary>
            /// Validates the model state before processing.
            /// </summary>
            /// 
            if (!ModelState.IsValid)
            {
                /// <summary>
                /// Returns bad request response for invalid model state.
                /// </summary>
                return BadRequest(ModelState);
            }

            /// <summary>
            /// Calls the service to add a new API detail.
            /// </summary>
            await _apiDetailService.Add(apiDetail);

            /// <summary>
            /// Returns successful response indicating the API detail was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }
        [Authorize(Policy = Permissions.ApiDetails.View)]

        [HttpGet("getallendpoints")]
        public async Task<IActionResult> GetallEndpoints()
        {
            /// <summary>
            /// Validates the model state before processing.
            /// </summary>
            /// 
            if (!ModelState.IsValid)
            {
                /// <summary>
                /// Returns bad request response for invalid model state.
                /// </summary>
                return BadRequest(ModelState);
            }

            /// <summary>
            /// Calls the service to add a new API detail.
            /// </summary>
            var data = await _apiDetailService.GetAllApiDetailsWithNode();

            /// <summary>
            /// Returns successful response indicating the API detail was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created, Data = data });
        }


        /// <summary>
        /// Updates an existing API detail.
        /// </summary>
        /// <param name="apiDetail">The API detail model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiDetails.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(ApiDetailCreateotUpdateModel apiDetail)
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
            /// Calls the service to update an existing API detail.
            /// </summary>
            await _apiDetailService.Update(apiDetail);

            /// <summary>
            /// Returns successful response indicating the API detail was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes an API detail by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API detail to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiDetails.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete an API detail.
            /// </summary>
            await _apiDetailService.Delete(id);

            /// <summary>
            /// Returns successful response indicating the API detail was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple API details by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the API details to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiDetails.Delete)]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            /// <summary>
            /// Validates that IDs were provided in the request body.
            /// </summary>
            if (ids == null || ids.Count == 0)
            {
                /// <summary>
                /// Returns bad request response when no IDs are provided.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
            }

            /// <summary>
            /// Calls the service to delete multiple API details.
            /// </summary>
            await _apiDetailService.DeleteMultiple(ids);

            /// <summary>
            /// Returns successful response indicating the API details were deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
