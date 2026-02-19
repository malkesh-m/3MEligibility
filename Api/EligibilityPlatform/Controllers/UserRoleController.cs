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
    /// API controller for managing user role operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserRoleController"/> class.
    /// </remarks>
    /// <param name="userRoleService">The user role service.</param>
    [Route("api/userrole")]
    [ApiController]
    public class UserRoleController(IUserRoleService userRoleService,IUserService userService,IMemoryCache cache) : ControllerBase
    {

        private readonly IUserRoleService _userRoleService = userRoleService;
        private readonly IUserService _userService = userService;
        private readonly IMemoryCache _cache = cache;
        /// <summary>
        /// Retrieves all user roles.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of user roles.</returns>
        /// 
        [Authorize(Policy = Permissions.UserRole.View)]

        [HttpGet(Name = "getallroles")]
        public IActionResult GetAll()
        {
            // Retrieves all user roles from the service
            List<UserRoleModel> result = _userRoleService.GetAll();
            // Returns success response with the list of user roles
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a user role by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user role (RoleId).</param>
        /// <returns>An <see cref="IActionResult"/> containing the users in the role if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.UserRole.View)]

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var tenantId = User.GetTenantId();
            var users = await _userService.GetAll(tenantId);
            // Retrieves users belonging to a specific role by its ID
            var result = _userRoleService.GetUserByRoleId(id,users);
            // Checks if the user role was found
            if (result != null)
            {
                // Returns success response with the user role data
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when user role doesn't exist
                return NotFound(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new user role.
        /// </summary>
        /// <param name="userRoleModel">The user role model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.UserRole.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(UserRoleCreateUpdateModel userRoleModel)
        {
            // Sets the created and updated by fields from the current user
            var UserName = User.GetUserName();
            userRoleModel.CreatedBy = UserName;
            userRoleModel.UpdatedBy = UserName;
            userRoleModel.TenantId = User.GetTenantId();
            var currentUserId = User.GetUserId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            var (IsValid, ErrorMessage) = await _userRoleService.EnsureCanManageUserRole(
                        userRoleModel.RoleId,
                        userRoleModel.TenantId,
                        currentUserId,
                        "assign users to");

            if (!IsValid)
            {
                return Ok(new ResponseModel
                {
                    IsSuccess = false,
                    Message = ErrorMessage??""
                });
            }
            // Adds the new user role
            var message = await _userRoleService.Add(userRoleModel);

            // Returns success response after creation
            return Ok(new ResponseModel { IsSuccess = true, Message = message });
        }

        /// <summary>
        /// Deletes a user role record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user role record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.UserRole.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the user role record by ID
            await _userRoleService.Remove(id);
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes a user role by user ID and role ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="roleId">The role ID.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.UserRole.Delete)]

        [HttpDelete("deletebyuseridandroleid")]
        public async Task<IActionResult> Delete(int userId, int roleId)
        {
            var tenantId = User.GetTenantId();
            var currentUserId = User.GetUserId();

            var (IsValid, ErrorMessage) = await _userRoleService.EnsureCanManageUserRole(
                        roleId,
                        tenantId,
                        currentUserId,
                        "remove users from");

            if (!IsValid)
            {
                return Ok(new ResponseModel
                {
                    IsSuccess = false,
                    Message = ErrorMessage??""
                });
            }

            // Deletes the user role relationship by user ID and role ID
            await _userRoleService.RemoveUserRole(userId, roleId);

            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Gets the number of roles a user belongs to within the current tenant.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The role count.</returns>
        [Authorize(Policy = Permissions.UserRole.View)]
        [HttpGet("count")]
        public async Task<IActionResult> GetUserRoleCount(int userId)
        {
            var tenantId = User.GetTenantId();
            var count = await _userRoleService.GetRoleCountByUserId(userId, tenantId);
            return Ok(new ResponseModel { IsSuccess = true, Data = count, Message = GlobalcConstants.Success });
        }
    }
}
