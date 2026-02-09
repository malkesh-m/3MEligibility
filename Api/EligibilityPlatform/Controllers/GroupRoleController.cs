using MEligibilityPlatform.Application.Attributes;
using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing group role operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GroupRoleController"/> class.
    /// </remarks>
    /// <param name="groupRoleService">The group role service.</param>
    [Route("api/grouprole")]
    [ApiController]
    public class GroupRoleController(IGroupRoleService groupRoleService) : ControllerBase
    {
        /// <summary>
        /// The group role service instance.
        /// </summary>
        private readonly IGroupRoleService _groupRoleService = groupRoleService;

        /// <summary>
        /// Retrieves all group role records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="GroupRoleModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.GroupRole.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all group role records
            List<GroupRoleModel> result = _groupRoleService.GetAll();
            // Returns success response with the retrieved group role list
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Adds a new group role record.
        /// </summary>
        /// <param name="groupRoleModel">The <see cref="GroupRoleModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        //[RequirePermission("Add Group Role")]
        [Authorize(Policy = Permissions.GroupRole.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(GroupRoleModel groupRoleModel)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new group role record
            await _groupRoleService.Add(groupRoleModel);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Deletes a group role record.
        /// </summary>
        /// <param name="groupRoleModel">The <see cref="GroupRoleModel"/> to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.GroupRole.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(GroupRoleModel groupRoleModel)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Removes the group role record
            await _groupRoleService.Remove(groupRoleModel);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Retrieves assigned roles by group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the assigned roles if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.GroupRole.View)]
        [HttpGet("getassignedrolesbygroupid")]
        public async Task<IActionResult> GetAssignedRolesByGroupId(int groupId)
        {
            // Retrieves assigned roles by group ID
            var result = await _groupRoleService.GetAssignedRoles(groupId);
            // Checks if any assigned roles were found
            if (result.Any())
            {
                // Returns success response with the retrieved assigned roles
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when no assigned roles exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Retrieves unassigned roles by group ID.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the unassigned roles if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.GroupRole.View)]
        [HttpGet("getunassignedrolesbygroupid")]
        public async Task<IActionResult> GetUnAssignedRolesByGroupId(int groupId)
        {
            // Retrieves unassigned roles by group ID
            var result = await _groupRoleService.GetUnAssignedRoles(groupId);
            // Checks if any unassigned roles were found
            if (result.Any())
            {
                // Returns success response with the retrieved unassigned roles
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when no unassigned roles exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }
    }
}