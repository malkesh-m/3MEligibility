using MEligibilityPlatform.Application.Attributes;
using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing API parameters operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiParametersController"/> class.
    /// </remarks>
    /// <param name="apiParameters">The API parameters service.</param>
    [Route("api/apiparameters")]
    [ApiController]
    public class ApiParametersController(IApiParametersService apiParameters) : ControllerBase
    {
        /// <summary>
        /// The API parameters service instance for parameter operations.
        /// </summary>
        private readonly IApiParametersService _apiParameters = apiParameters;

        /// <summary>
        /// Retrieves all API parameters.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing all API parameters.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameters.View)]

        [HttpGet]
        public IActionResult Getall()
        {
            var tenantId = User.GetTenantId();
            /// <summary>
            /// Returns successful response with all API parameters.
            /// </summary>
            return Ok(new ResponseModel { Data = _apiParameters.GetAll(tenantId), Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves an API parameter by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter.</param>
        /// <returns>An <see cref="IActionResult"/> containing the API parameter if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameters.View)]

        [HttpGet("id")]
        public IActionResult Getbyid(int id)
        {
            /// <summary>
            /// Retrieves a specific API parameter by ID from the service.
            /// </summary>
            var result = _apiParameters.GetById(id);

            /// <summary>
            /// Checks if the API parameter was found.
            /// </summary>
            if (result == null)
            {
                /// <summary>
                /// Returns not found response when the API parameter does not exist.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }

            /// <summary>
            /// Returns successful response with the retrieved API parameter.
            /// </summary>
            return Ok(new ResponseModel { Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Adds a new API parameter.
        /// </summary>
        /// <param name="model">The <see cref="ApiParametersCreateUpdateModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameters.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(ApiParametersCreateUpdateModel model)
        {
            var tenantId = User.GetTenantId();
            model.TenantId = tenantId;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Call service to add new API parameter and get inserted entity
            var addedEntity = await _apiParameters.Add(model);

            // Return inserted entity with its ID
            return Ok(new ResponseModel
            {
                IsSuccess = true,
                Message = GlobalcConstants.Created,
                Data = new
                {
                    apiParamterId = addedEntity.ApiParamterId // <-- return the generated ID
                }
            });
        }
        [Authorize(Policy = Permissions.ApiParameterMaps.View)]

        [HttpGet("getById")]
        public async Task<IActionResult> GetByApiId(int id)
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
            /// Calls the service to add a new API parameter.
            /// </summary>
            var data = await _apiParameters.GetByApiId(id);


            /// <summary>
            /// Returns successful response indicating the API parameter was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created, Data = data });
        }
        /// <summary>
        /// Updates an existing API parameter.
        /// </summary>
        /// <param name="model">The <see cref="ApiParametersCreateUpdateModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameters.Edit)]

        [HttpPut]
        public async Task<IActionResult> Update(ApiParametersCreateUpdateModel model)
        {
            var tenantId = User.GetTenantId();
            model.TenantId = tenantId;
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
            /// Calls the service to update an existing API parameter.
            /// </summary>
            await _apiParameters.Update(model);

            /// <summary>
            /// Returns successful response indicating the API parameter was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes an API parameter by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameters.Delete)]

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete an API parameter.
            /// </summary>
            await _apiParameters.Remove(id);

            /// <summary>
            /// Returns successful response indicating the API parameter was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        [Authorize(Policy = Permissions.ApiParameters.Delete)]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> DeleteMultiple(List<int> ids)
        {
            /// <summary>
            /// Calls the service to delete an API parameter.
            /// </summary>
            await _apiParameters.RemoveMultiple(ids);

            /// <summary>
            /// Returns successful response indicating the API parameter was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
