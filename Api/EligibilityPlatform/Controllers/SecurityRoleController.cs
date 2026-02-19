using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{    /// <summary>
     /// API controller for managing security role operations.
     /// </summary>
     /// <remarks>
     /// Initializes a new instance of the <see cref="SecurityRoleController"/> class.
     /// </remarks>
     /// <param name="securityRoleService">The security role service for managing security roles.</param>
     /// <param name="userRoleService">The user role service for managing user-role relationships.</param>
     /// <param name="rolePermissionService">The role permission service for managing role-permission relationships.</param>
    [Route("api/securityrole")]
    [ApiController]
    public class SecurityRoleController(ISecurityRoleService securityRoleService, IUserRoleService userRoleService, IRolePermissionService rolePermissionService) : ControllerBase
    {

        private readonly ISecurityRoleService _securityRoleService = securityRoleService;
        private readonly IUserRoleService _userRoleService = userRoleService;
        private readonly IRolePermissionService _rolePermissionService = rolePermissionService;

        /// <summary>
        /// Retrieves all security roles from the system.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of security roles with success status.</returns>
        /// <response code="200">Returns the list of security roles successfully.</response>
        /// 
        [Authorize(Policy = Permissions.Role.View)]
        [HttpGet("getall")]
        public async Task<IActionResult> Get()
        {
            var tenantId = User.GetTenantId();
            // Retrieves all security roles from the security role service
            List<SecurityRoleModel> result = _securityRoleService.GetAll(tenantId);
            var currentUserId = User.GetUserId();
            var currentUserRoles = await _userRoleService.GetRoleNamesForUser(currentUserId, tenantId);
            var currentRank = _userRoleService.GetHighestRank(currentUserRoles);
            result = [.. result.Where(g => _userRoleService.GetRank(g.RoleName ?? "") <= currentRank)];
            foreach (var role in result)
            {
                role.UserCount = await _userRoleService.GetUserCountByRoleId(role.RoleId, tenantId);
            }
            // Returns success response with the list of security roles
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a specific security role by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the security role to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing the security role data if found.</returns>
        /// <response code="200">Returns the security role data successfully.</response>
        /// <response code="404">Returned when the security role with the specified ID is not found.</response>
        /// 
        [Authorize(Policy = Permissions.Role.View)]

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {  var tenantId = User.GetTenantId();
            // Retrieves a specific security role by its ID
            var result = _securityRoleService.GetById(id,tenantId);
            // Checks if the security role was found
            if (result != null)
            {
                var currentUserId = User.GetUserId();
                var currentUserRoles = await _userRoleService.GetRoleNamesForUser(currentUserId, tenantId);
                var currentRank = _userRoleService.GetHighestRank(currentUserRoles);
                if (_userRoleService.GetRank(result.RoleName ?? "") > currentRank)
                {
                    return Ok(new ResponseModel { IsSuccess = false, Message = "You are not allowed to view this role." });
                }
                result.UserCount = await _userRoleService.GetUserCountByRoleId(result.RoleId, tenantId);
                // Returns success response with the security role data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when security role doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Creates a new security role in the system.
        /// </summary>
        /// <param name="securityRoleModel">The security role model containing the data to create.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the creation operation.</returns>
        /// <response code="200">Returns when the security role is created successfully.</response>
        /// <response code="400">Returned when the model state is invalid or validation fails.</response>
        /// 
        [Authorize(Policy = Permissions.Role.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(SecurityRoleUpdateModel securityRoleModel)
        {
            // Sets the created and updated by fields with the current user's name
            var tenantId = User.GetTenantId();
            var UserName = User.GetUserName();
            securityRoleModel.TenantId = tenantId;
            securityRoleModel.CreatedBy = UserName;
            securityRoleModel.UpdatedBy = UserName;

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new security role
            try
            {
                await _securityRoleService.Add(securityRoleModel);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
            // Returns success response after creation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing security role with new data.
        /// </summary>
        /// <param name="securityRoleModel">The security role model containing the updated data.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
        /// <response code="200">Returns when the security role is updated successfully.</response>
        /// <response code="400">Returned when the model state is invalid or validation fails.</response>
        /// 
        [Authorize(Policy = Permissions.Role.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(SecurityRoleUpdateModel securityRoleModel)
        {
            var tenantId = User.GetTenantId();
            securityRoleModel.TenantId = tenantId;
            var userName = User.GetUserName();
            securityRoleModel.UpdatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing security role
            try
            {
                await _securityRoleService.Update(securityRoleModel);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
            // Returns success response after update
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a specific security role by its unique identifier after checking for dependencies.
        /// </summary>
        /// <param name="id">The unique identifier of the security role to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the deletion operation.</returns>
        /// <response code="200">Returns when the security role is deleted successfully or when validation prevents deletion.</response>
        /// 
        [Authorize(Policy = Permissions.Role.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = User.GetTenantId();
            // Retrieves the security role by ID to check if it exists
            var item = _securityRoleService.GetById(id, tenantId);
            if (item == null)
            {
                // Returns error response if security role is not found
                return Ok(new ResponseModel { IsSuccess = false, Message = "Security role not found." });
            }
            var currentUserId = User.GetUserId();
            var currentUserRoles = await _userRoleService.GetRoleNamesForUser(currentUserId, tenantId);
            var currentRank = _userRoleService.GetHighestRank(currentUserRoles);
            if (_userRoleService.GetRank(item.RoleName ?? "") > currentRank)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "You are not allowed to delete this role." });
            }

            // Checks if there are users assigned to this security role
            var usersAssigned = await _userRoleService.GetByUserRoleId(id);
            if (usersAssigned)
            {
                // Returns error response if users are assigned to the role
                return Ok(new ResponseModel { IsSuccess = false, Message = "The role cannot be deleted because there are users assigned to it." });
            }

            // Checks if there are permissions assigned to this security role
            var permissionsAssigned = await _rolePermissionService.GetBySecurityRoleId(id);
            if (permissionsAssigned)
            {
                // Auto-remove permissions when no users are assigned
                try
                {
                    await _rolePermissionService.RemoveByRoleId(id, tenantId);
                }
                catch (InvalidOperationException ex)
                {
                    return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
                }
            }

            // Deletes the security role by its ID
            try
            {
                await _securityRoleService.Remove(id);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple security roles by their unique identifiers after checking for dependencies.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the security roles to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the bulk deletion operation.</returns>
        /// <response code="200">Returns when all security roles are deleted successfully or when validation prevents deletion.</response>
        [Authorize(Policy = Permissions.Role.Delete)]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            var tenantId = User.GetTenantId();
            // Validates that IDs were provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }

            // Iterates through each ID to check for dependencies
            foreach (var id in ids)
            {
                // Retrieves the security role by ID to check if it exists
                var item = _securityRoleService.GetById(id,tenantId);
                if (item == null)
                {
                    // Returns error response if security role is not found
                    return Ok(new ResponseModel { IsSuccess = false, Message = "Security role not found." });
                }
                var currentUserId = User.GetUserId();
                var currentUserRoles = await _userRoleService.GetRoleNamesForUser(currentUserId, tenantId);
                var currentRank =_userRoleService. GetHighestRank(currentUserRoles);
                if ( _userRoleService.GetRank(item.RoleName ?? "")> currentRank)
                {
                    return Ok(new ResponseModel { IsSuccess = false, Message = "You are not allowed to delete this role." });
                }

                // Checks if there are users assigned to this security role
                var usersAssigned = await _userRoleService.GetByUserRoleId(id);
                if (usersAssigned)
                {
                    // Returns error response if users are assigned to the role
                    return Ok(new ResponseModel { IsSuccess = false, Message = "The role cannot be deleted because there are users assigned to it." });
                }

                // Checks if there are permissions assigned to this security role
                var permissionsAssigned = await _rolePermissionService.GetBySecurityRoleId(id);
                if (permissionsAssigned)
                {
                    // Auto-remove permissions when no users are assigned
                    try
                    {
                        await _rolePermissionService.RemoveByRoleId(id, tenantId);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
                    }
                }
            }

            // Deletes multiple security roles by their IDs
            try
            {
                await _securityRoleService.MultipleDelete(ids);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

    }
}
