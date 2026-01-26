using MEligibilityPlatform.Application.Attributes;
using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing exception management operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ExceptionManagementController"/> class.
    /// </remarks>
    /// <param name="exceptionManagement">The exception management service.</param>
    [Route("api/exceptionmanagement")]
    [ApiController]
    public class ExceptionManagementController(IExceptionManagementService exceptionManagement) : ControllerBase
    {
        private readonly IExceptionManagementService _exceptionManagement = exceptionManagement;

        /// <summary>
        /// Retrieves all exception management records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the exception management records.</returns>
        /// 
        [Authorize(Policy = Permissions.Exception.View)]

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            // Retrieves all exception management records for the current entity
            var data = _exceptionManagement.GetAll(User.GetTenantId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = data });
        }

        /// <summary>
        /// Retrieves an exception management record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the exception management record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the record if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.Exception.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves specific exception management record by ID for the current entity
            var result = _exceptionManagement.GetById(User.GetTenantId(), id);
            if (result != null)
            {
                // Returns success response with the found record data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when record does not exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new exception management record.
        /// </summary>
        /// <param name="managementModel">The exception management model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Exception.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(ExceptionManagementCreateOrUpdateModel managementModel)
        {
            // Sets the current user as the creator of the record
            managementModel.CreatedBy = User.GetUserName();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request response if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new exception management record
            await _exceptionManagement.Add(User.GetTenantId(), managementModel);
            // Returns success response indicating record was created
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing exception management record.
        /// </summary>
        /// <param name="managementModel">The exception management model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Exception.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(ExceptionManagementCreateOrUpdateModel managementModel)
        {
            // Sets the current user as the updater of the record
            managementModel.CreatedBy = User.GetUserName();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request response if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing exception management record
            await _exceptionManagement.Update(User.GetTenantId(), managementModel);
            // Returns success response indicating record was updated
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes an exception management record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the exception management record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Exception.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            // Deletes the exception management record by ID for the current entity
            await _exceptionManagement.Delete(User.GetTenantId(), id);
            // Returns success response indicating record was deleted
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
