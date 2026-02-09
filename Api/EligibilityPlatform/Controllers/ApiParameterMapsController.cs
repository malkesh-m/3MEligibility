using MEligibilityPlatform.Application.Attributes;
using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing API parameter map operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiParameterMapsController"/> class.
    /// </remarks>
    /// <param name="ApiParameterMaps">The API parameter map service.</param>
    [Route("api/apiparametermaps")]
    [ApiController]
    public class ApiParameterMapsController(IApiParameterMapservice ApiParameterMaps) : ControllerBase
    {
        /// <summary>
        /// The API parameter map service instance for parameter map operations.
        /// </summary>
        private readonly IApiParameterMapservice _ApiParameterMaps = ApiParameterMaps;

        /// <summary>
        /// Retrieves all API parameter maps.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing all API parameter maps.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameterMaps.View)]

        [HttpGet]
        public IActionResult Getall()
        {
            int tenantId = User.GetTenantId();
            /// <summary>
            /// Returns successful response with all API parameter maps.
            /// </summary>
            return Ok(new ResponseModel { Data = _ApiParameterMaps.GetAll(tenantId), Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves an API parameter map by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter map.</param>
        /// <returns>An <see cref="IActionResult"/> containing the API parameter map if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameterMaps.View)]

        [HttpGet("id")]
        public IActionResult Getbyid(int id)
        {
            var tenantId = User.GetTenantId();
            /// <summary>
            /// Retrieves a specific API parameter map by ID from the service.
            /// </summary>
            var result = _ApiParameterMaps.GetById(id,tenantId);

            /// <summary>
            /// Checks if the API parameter map was found.
            /// </summary>
            if (result == null)
            {
                /// <summary>
                /// Returns not found response when the API parameter map does not exist.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }

            /// <summary>
            /// Returns successful response with the retrieved API parameter map.
            /// </summary>
            return Ok(new ResponseModel { Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Adds a new API parameter map.
        /// </summary>
        /// <param name="model">The <see cref="ApiParameterCreateUpdateMapModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameterMaps.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(ApiParameterCreateUpdateMapModel model)
        {
            /// <summary>
            /// Validates the model state before processing.
            /// </summary>
            /// 
            var tenantId = User.GetTenantId();
            model.TenantId = tenantId;
            if (!ModelState.IsValid)
            {
                /// <summary>
                /// Returns bad request response for invalid model state.
                /// </summary>
                return BadRequest(ModelState);
            }

            /// <summary>
            /// Calls the service to add a new API parameter map.
            /// </summary>
            await _ApiParameterMaps.Add(model);

            /// <summary>
            /// Returns successful response indicating the API parameter map was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing API parameter map.
        /// </summary>
        /// <param name="model">The <see cref="ApiParameterCreateUpdateMapModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameterMaps.Edit)]
        [HttpPut]
        public async Task<IActionResult> Update(ApiParameterCreateUpdateMapModel model)
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
            /// Calls the service to update an existing API parameter map.
            /// </summary>
            await _ApiParameterMaps.Update(model);

            /// <summary>
            /// Returns successful response indicating the API parameter map was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }
        [Authorize(Policy = Permissions.ApiParameterMaps.View)]

        [HttpGet("getbyapi/{apiId}")]
        public IActionResult GetByApi(int apiId)
        {
            try
            {
                var mappings = _ApiParameterMaps.GetMappingsByApiId(apiId);
                return Ok(new { success = true, data = mappings });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes an API parameter map by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter map to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ApiParameterMaps.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete an API parameter map.
            /// </summary>
            await _ApiParameterMaps.Remove(id);

            /// <summary>
            /// Returns successful response indicating the API parameter map was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
