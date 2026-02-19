using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing role permission operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RolePermissionController"/> class.
    /// </remarks>
    /// <param name="rolePermissionService">The role permission service.</param>
    /// <param name="userRoleService">The user role service.</param>
    [Route("api/rolepermission")]
    [ApiController]
    public class RolePermissionController(IRolePermissionService rolePermissionService, IUserRoleService userRoleService) : ControllerBase
    {
        /// <summary>
        /// The role permission service instance.
        /// </summary>
        private readonly IRolePermissionService _rolePermissionService = rolePermissionService;
        private readonly IUserRoleService _userRoleService = userRoleService;

        /// <summary>
        /// Retrieves all role permission records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="RolePermissionModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.RolePermission.View)]
        [HttpGet("getall")]
        public async Task<IActionResult> Get()
        {
            var result = _rolePermissionService.GetAll();

            var tenantId = User.GetTenantId();
            var currentUserId = User.GetUserId();

            var currentUserRoles = await _userRoleService
                .GetRoleNamesForUser(currentUserId, tenantId);

            var currentRank = _userRoleService.GetHighestRank(currentUserRoles);

            // Collect distinct role IDs
            var roleIds = result
                .Select(r => r.RoleId)
                .Distinct()
                .ToList();

            // Fetch all required role names in ONE call
            var roleDictionary = await _userRoleService
                .GetRoleNamesByIds(roleIds, tenantId);

            // Filter in memory 
            result = [.. result
                .Where(rp =>
                {
                    if (!roleDictionary.TryGetValue(rp.RoleId, out var roleName))
                        return false;

                    return _userRoleService.GetRank(roleName) <= currentRank;
                })];

            return Ok(new ResponseModel
            {
                IsSuccess = true,
                Data = result,
                Message = GlobalcConstants.Success
            });
        }

        /// <summary>
        /// Adds a new role permission record.
        /// </summary>
        /// <param name="rolePermissionModel">The <see cref="RolePermissionModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.RolePermission.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(RolePermissionModel rolePermissionModel)
        {
            rolePermissionModel.TenantId = User.GetTenantId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new role permission record
            try
            {
                await _rolePermissionService.Add(rolePermissionModel);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Deletes a role permission record.
        /// </summary>
        /// <param name="rolePermissionModel">The <see cref="RolePermissionModel"/> to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.RolePermission.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(RolePermissionModel rolePermissionModel)
        {
            var tenantId = User.GetTenantId();
            rolePermissionModel.TenantId = tenantId;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Removes the role permission record
            try
            {
                await _rolePermissionService.Remove(rolePermissionModel);
            }
            catch (InvalidOperationException ex)
            {
                var message = ex.Message == "Role not found."
                    ? "You cannot remove permissions from this role."
                    : ex.Message;
                return Ok(new ResponseModel { IsSuccess = false, Message = message });
            }
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Retrieves assigned permissions by role ID.
        /// </summary>
        /// <param name="roleId">The role ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the assigned permissions if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.RolePermission.View)]
        [HttpGet("getassignedpermissionsbyroleid")]
        public async Task<IActionResult> GetAssignedPermissionsByRoleId(int roleId)
        {
            var tenantId = User.GetTenantId();
            var currentUserId = User.GetUserId();
            var currentUserRoles = await _userRoleService.GetRoleNamesForUser(currentUserId, tenantId);
            var currentRank = _userRoleService.GetHighestRank(currentUserRoles);
            var targetRoleName = await _userRoleService.GetRoleNameById(roleId, tenantId) ?? "";
            if (_userRoleService.GetRank(targetRoleName) > currentRank)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "You are not allowed to view this role's permissions." });
            }
            // Retrieves assigned permissions by role ID
            var result = await _rolePermissionService.GetAssignedPermissions(roleId,tenantId);
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
        /// Retrieves unassigned permissions by role ID.
        /// </summary>
        /// <param name="roleId">The role ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the unassigned permissions if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.RolePermission.View)]
        [HttpGet("getunassignedpermissionsbyroleid")]
        public async Task<IActionResult> GetUnAssignedPermissionsByRoleId(int roleId)
        {
            var tenantId = User.GetTenantId();
            var currentUserId = User.GetUserId();
            var currentUserRoles = await _userRoleService.GetRoleNamesForUser(currentUserId, tenantId);
            var currentRank = _userRoleService.GetHighestRank(currentUserRoles);
            var targetRoleName = await _userRoleService.GetRoleNameById(roleId, tenantId) ?? "";
            if (_userRoleService.GetRank(targetRoleName) > currentRank)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "You are not allowed to view this role's permissions." });
            }
            // Retrieves unassigned permissions by role ID
            var result = await _rolePermissionService.GetUnAssignedPermissions(roleId,tenantId);
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
