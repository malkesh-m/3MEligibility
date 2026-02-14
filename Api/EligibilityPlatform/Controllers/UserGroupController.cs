using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing user group operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserGroupController"/> class.
    /// </remarks>
    /// <param name="userGroupService">The user group service.</param>
    [Route("api/usergroup")]
    [ApiController]
    public class UserGroupController(IUserGroupService userGroupService,IUserService userService,IMemoryCache cache) : ControllerBase
    {

        private readonly IUserGroupService _userGroupService = userGroupService;
        private readonly IUserService _userService = userService;
        private readonly IMemoryCache _cache = cache;
        /// <summary>
        /// Retrieves all user groups.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of user groups.</returns>
        /// 
        [Authorize(Policy = Permissions.UserGroup.View)]

        [HttpGet(Name = "getall")]
        public IActionResult GetAll()
        {
            // Retrieves all user groups from the service
            List<UserGroupModel> result = _userGroupService.GetAll();
            // Returns success response with the list of user groups
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a user group by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user group.</param>
        /// <returns>An <see cref="IActionResult"/> containing the user group if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.UserGroup.View)]

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var tenantId = User.GetTenantId();
            var users = await _userService.GetAll(tenantId);
            // Retrieves a specific user group by its ID
            var result = _userGroupService.GetUserByGroupId(id,users);
            // Checks if the user group was found
            if (result != null)
            {
                // Returns success response with the user group data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when user group doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new user group.
        /// </summary>
        /// <param name="userGroupModel">The user group model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.UserGroup.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(UserGroupCreateUpdateModel userGroupModel)
        {
            // Sets the created and updated by fields from the current user
            var UserName = User.GetUserName();
            userGroupModel.CreatedBy = UserName;
            userGroupModel.UpdatedBy = UserName;
            userGroupModel.TenantId = User.GetTenantId();
            var currentUserId = User.GetUserId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            var targetGroupName = await _userGroupService.GetGroupNameById(userGroupModel.GroupId, userGroupModel.TenantId);
            if (string.IsNullOrWhiteSpace(targetGroupName))
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "Group not found." });
            }

            var currentUserGroups = await _userGroupService.GetGroupNamesForUser(currentUserId, userGroupModel.TenantId);
            var currentRank = _userGroupService.GetHighestRank(currentUserGroups);
            var targetRank = _userGroupService.GetRank(targetGroupName);

            if (targetRank == Rank.SuperAdmin && currentRank != Rank.SuperAdmin)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "Only Super Admin can assign users to the Super Admin group." });
            }

            if (targetRank == Rank.Admin && currentRank != Rank.SuperAdmin)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "Only Super Admin and Admin can assign users to the Admin group." });
            }
            // Adds the new user group
            var message = await _userGroupService.Add(userGroupModel);

            // Returns success response after creation
            return Ok(new ResponseModel { IsSuccess = true, Message = message });
        }

        /// <summary>
        /// Deletes a user group by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user group to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.UserGroup.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the user group by ID
            await _userGroupService.Remove(id);
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes a user group by user ID and group ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="groupId">The group ID.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.UserGroup.Delete)]

        [HttpDelete("deletebyuseridandgroupid")]
        public async Task<IActionResult> Delete(int userId, int groupId)
        {
            var tenantId = User.GetTenantId();
            var currentUserId = User.GetUserId();

            var targetGroupName = await _userGroupService.GetGroupNameById(groupId, tenantId);
            if (string.IsNullOrWhiteSpace(targetGroupName))
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "Group not found." });
            }

            var currentUserGroups = await _userGroupService.GetGroupNamesForUser(currentUserId, tenantId);
            var currentRank =_userGroupService.GetHighestRank(currentUserGroups);
            var targetRank = _userGroupService.GetRank(targetGroupName);

            if (targetRank == 0)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "Invalid target group." });
            }

            if (targetRank == Rank.SuperAdmin && currentRank != Rank.SuperAdmin)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "Only Super Admin can remove users from the Super Admin group." });
            }

            if (targetRank == Rank.SuperAdmin)
            {
                var superAdminCount = await _userGroupService.GetUserCountByGroupId(groupId, tenantId);
                if (superAdminCount <= 1)
                {
                    return Ok(new ResponseModel { IsSuccess = false, Message = "You cannot remove the last Super Admin user." });
                }
            }

            if (targetRank == Rank.Admin && currentRank < Rank.Admin)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "Only Admin or Super Admin can remove users from the Admin group." });
            }

            // Deletes the user group by user ID and group ID
            await _userGroupService.RemoveUserGroup(userId, groupId);

            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Gets the number of groups a user belongs to within the current tenant.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The group count.</returns>
        [Authorize(Policy = Permissions.UserGroup.View)]
        [HttpGet("count")]
        public async Task<IActionResult> GetUserGroupCount(int userId)
        {
            var tenantId = User.GetTenantId();
            var count = await _userGroupService.GetGroupCountByUserId(userId, tenantId);
            return Ok(new ResponseModel { IsSuccess = true, Data = count, Message = GlobalcConstants.Success });
        }
    }
}
