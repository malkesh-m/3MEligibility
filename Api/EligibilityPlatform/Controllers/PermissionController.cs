using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing permission operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PermissionController"/> class.
    /// </remarks>
    /// <param name="permissionService">The permission service.</param>
    [Route("api/permission")]
    [ApiController]
    public class PermissionController(IPermissionService permissionService) : ControllerBase
    {
        private readonly IPermissionService _permissionService = permissionService;

        /// <summary>
        /// Retrieves all permissions.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of permissions.</returns>
        /// 
        [Authorize(Policy = Permissions.Permission.View)]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all permissions from the permission service
            List<PermissionModel> result = _permissionService.GetAll();
            // Returns success response with the list of permissions
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a permission by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the permission.</param>
        /// <returns>An <see cref="IActionResult"/> containing the permission if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.Permission.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a specific permission by its ID
            var result = _permissionService.GetById(id);
            // Checks if the permission was found
            if (result != null)
            {
                // Returns success response with the permission data
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when permission doesn't exist
                return NotFound(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new permission.
        /// </summary>
        /// <param name="permissionModel">The permission model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Permission.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(PermissionCreateUpdateModel permissionModel)
        {

            var userName = User.GetUserName();
            permissionModel.CreatedBy = userName;
            permissionModel.UpdatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new permission
            await _permissionService.Add(permissionModel);
            // Returns success response after creation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing permission.
        /// </summary>
        /// <param name="permissionModel">The permission model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Permission.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(PermissionCreateUpdateModel permissionModel)
        {
            var userName = User.GetUserName();
            permissionModel.CreatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing permission
            await _permissionService.Update(permissionModel);
            // Returns success response after update
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a permission by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the permission to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Permission.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the permission by its ID
            await _permissionService.Remove(id);
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}

