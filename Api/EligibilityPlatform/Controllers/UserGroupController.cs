using MEligibilityPlatform.Application.Attributes;
using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
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
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
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
            // Deletes the user group by user ID and group ID
            await _userGroupService.RemoveUserGroup(userId, groupId);

            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
