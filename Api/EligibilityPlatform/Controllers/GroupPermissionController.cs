using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing group permission operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GroupPermissionController"/> class.
    /// </remarks>
    /// <param name="groupPermissionService">The group permission service.</param>
    [Route("api/grouppermission")]
    [ApiController]
    public class GroupPermissionController(IGroupPermissionService groupPermissionService) : ControllerBase
    {
        /// <summary>
        /// The group permission service instance.
        /// </summary>
        private readonly IGroupPermissionService _groupPermissionService = groupPermissionService;

        /// <summary>
        /// Retrieves all group permission records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="GroupPermissionModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.GroupPermission.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all group permission records
            List<GroupPermissionModel> result = _groupPermissionService.GetAll();
            // Returns success response with the retrieved group permission list
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Adds a new group permission record.
        /// </summary>
        /// <param name="groupPermissionModel">The <see cref="GroupPermissionModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        //[RequirePermission("Add Group Permission")]
        [Authorize(Policy = Permissions.GroupPermission.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(GroupPermissionModel groupPermissionModel)
        {
            groupPermissionModel.TenantId = User.GetTenantId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new group permission record
            await _groupPermissionService.Add(groupPermissionModel);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Deletes a group permission record.
        /// </summary>
        /// <param name="groupPermissionModel">The <see cref="GroupPermissionModel"/> to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.GroupPermission.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(GroupPermissionModel groupPermissionModel)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Removes the group permission record
            await _groupPermissionService.Remove(groupPermissionModel);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Retrieves assigned permissions by group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the assigned permissions if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.GroupPermission.View)]
        [HttpGet("getassignedpermissionsbygroupid")]
        public async Task<IActionResult> GetAssignedPermissionsByGroupId(int groupId)
        {
            // Retrieves assigned permissions by group ID
            var result = await _groupPermissionService.GetAssignedPermissions(groupId);
            // Checks if any assigned permissions were found
            if (result.Any())
            {
                // Returns success response with the retrieved assigned permissions
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when no assigned permissions exist
                return NotFound(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Retrieves unassigned permissions by group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the unassigned permissions if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.GroupPermission.View)]
        [HttpGet("getunassignedpermissionsbygroupid")]
        public async Task<IActionResult> GetUnAssignedPermissionsByGroupId(int groupId)
        {
            // Retrieves unassigned permissions by group ID
            var result = await _groupPermissionService.GetUnAssignedPermissions(groupId);
            // Checks if any unassigned permissions were found
            if (result.Any())
            {
                // Returns success response with the retrieved unassigned permissions
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when no unassigned permissions exist
                return NotFound(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
        }
    }
}
