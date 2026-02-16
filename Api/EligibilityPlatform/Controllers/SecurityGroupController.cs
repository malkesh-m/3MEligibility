using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{    /// <summary>
     /// API controller for managing security group operations.
     /// </summary>
     /// <remarks>
     /// Initializes a new instance of the <see cref="SecurityGroupController"/> class.
     /// </remarks>
     /// <param name="securityGroupService">The security group service for managing security groups.</param>
     /// <param name="userGroupService">The user group service for managing user-group relationships.</param>
     /// <param name="groupPermissionService">The group role service for managing group-role relationships.</param>
    [Route("api/securitygroup")]
    [ApiController]
    public class SecurityGroupController(ISecurityGroupService securityGroupService, IUserGroupService userGroupService, IGroupPermissionService groupPermissionService) : ControllerBase
    {

        private readonly ISecurityGroupService _securityGroupService = securityGroupService;
        private readonly IUserGroupService _userGroupService = userGroupService;
        private readonly IGroupPermissionService _groupPermissionService = groupPermissionService;

        /// <summary>
        /// Retrieves all security groups from the system.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of security groups with success status.</returns>
        /// <response code="200">Returns the list of security groups successfully.</response>
        /// 
        [Authorize(Policy = Permissions.Group.View)]
        [HttpGet("getall")]
        public async Task<IActionResult> Get()
        {
            var tenantId = User.GetTenantId();
            // Retrieves all security groups from the security group service
            List<SecurityGroupModel> result = _securityGroupService.GetAll(tenantId);
            var currentUserId = User.GetUserId();
            var currentUserGroups = await _userGroupService.GetGroupNamesForUser(currentUserId, tenantId);
            var currentRank = _userGroupService.GetHighestRank(currentUserGroups);
            result = [.. result.Where(g => _userGroupService.GetRank(g.GroupName ?? "") <= currentRank)];
            foreach (var group in result)
            {
                group.UserCount = await _userGroupService.GetUserCountByGroupId(group.GroupId, tenantId);
            }
            // Returns success response with the list of security groups
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a specific security group by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the security group to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing the security group data if found.</returns>
        /// <response code="200">Returns the security group data successfully.</response>
        /// <response code="404">Returned when the security group with the specified ID is not found.</response>
        /// 
        [Authorize(Policy = Permissions.Group.View)]

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {  var tenantId = User.GetTenantId();
            // Retrieves a specific security group by its ID
            var result = _securityGroupService.GetById(id,tenantId);
            // Checks if the security group was found
            if (result != null)
            {
                var currentUserId = User.GetUserId();
                var currentUserGroups = await _userGroupService.GetGroupNamesForUser(currentUserId, tenantId);
                var currentRank = _userGroupService.GetHighestRank(currentUserGroups);
                if (_userGroupService.GetRank(result.GroupName ?? "") > currentRank)
                {
                    return Ok(new ResponseModel { IsSuccess = false, Message = "You are not allowed to view this group." });
                }
                result.UserCount = await _userGroupService.GetUserCountByGroupId(result.GroupId, tenantId);
                // Returns success response with the security group data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when security group doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Creates a new security group in the system.
        /// </summary>
        /// <param name="securityGroupModel">The security group model containing the data to create.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the creation operation.</returns>
        /// <response code="200">Returns when the security group is created successfully.</response>
        /// <response code="400">Returned when the model state is invalid or validation fails.</response>
        /// 
        [Authorize(Policy = Permissions.Group.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(SecurityGroupUpdateModel securityGroupModel)
        {
            // Sets the created and updated by fields with the current user's name
            var tenantId = User.GetTenantId();
            var UserName = User.GetUserName();
            securityGroupModel.TenantId = tenantId;
            securityGroupModel.CreatedBy = UserName;
            securityGroupModel.UpdatedBy = UserName;

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new security group
            try
            {
                await _securityGroupService.Add(securityGroupModel);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
            // Returns success response after creation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing security group with new data.
        /// </summary>
        /// <param name="securityGroupModel">The security group model containing the updated data.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
        /// <response code="200">Returns when the security group is updated successfully.</response>
        /// <response code="400">Returned when the model state is invalid or validation fails.</response>
        /// 
        [Authorize(Policy = Permissions.Group.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(SecurityGroupUpdateModel securityGroupModel)
        {
            var tenantId = User.GetTenantId();
            securityGroupModel.TenantId = tenantId;
            var userName = User.GetUserName();
            securityGroupModel.UpdatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing security group
            try
            {
                await _securityGroupService.Update(securityGroupModel);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
            // Returns success response after update
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a specific security group by its unique identifier after checking for dependencies.
        /// </summary>
        /// <param name="id">The unique identifier of the security group to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the deletion operation.</returns>
        /// <response code="200">Returns when the security group is deleted successfully or when validation prevents deletion.</response>
        /// 
        [Authorize(Policy = Permissions.Group.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var tenantId = User.GetTenantId();
            // Retrieves the security group by ID to check if it exists
            var item = _securityGroupService.GetById(id, tenantId);
            if (item == null)
            {
                // Returns error response if security group is not found
                return Ok(new ResponseModel { IsSuccess = false, Message = "Security group not found." });
            }
            var currentUserId = User.GetUserId();
            var currentUserGroups = await _userGroupService.GetGroupNamesForUser(currentUserId, tenantId);
            var currentRank = _userGroupService.GetHighestRank(currentUserGroups);
            if (_userGroupService.GetRank(item.GroupName ?? "") > currentRank)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = "You are not allowed to delete this group." });
            }

            // Checks if there are users assigned to this security group
            var usersAssigned = await _userGroupService.GetByUserGroupId(id);
            if (usersAssigned)
            {
                // Returns error response if users are assigned to the group
                return Ok(new ResponseModel { IsSuccess = false, Message = "The group cannot be deleted because there are users assigned to it." });
            }

            // Checks if there are roles assigned to this security group
            var rolesAssigned = await _groupPermissionService.GetBySecurityGroupId(id);
            if (rolesAssigned)
            {
                // Auto-remove roles when no users are assigned
                try
                {
                    await _groupPermissionService.RemoveByGroupId(id, tenantId);
                }
                catch (InvalidOperationException ex)
                {
                    return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
                }
            }

            // Additional check for user group assignments
            //var userGroupAssigned = await _userGroupService.GetByUserGroupsId(id);
            //if (userGroupAssigned)
            //{
            //    // Returns error response if users are assigned to the group
            //    return Ok(new ResponseModel { IsSuccess = false, Message = "The group cannot be deleted because there are users assigned to it." });
            //}

            // Deletes the security group by its ID
            try
            {
                await _securityGroupService.Remove(id);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple security groups by their unique identifiers after checking for dependencies.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the security groups to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the bulk deletion operation.</returns>
        /// <response code="200">Returns when all security groups are deleted successfully or when validation prevents deletion.</response>
        [Authorize(Policy = Permissions.Group.Delete)]

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
                // Retrieves the security group by ID to check if it exists
                var item = _securityGroupService.GetById(id,tenantId);
                if (item == null)
                {
                    // Returns error response if security group is not found
                    return Ok(new ResponseModel { IsSuccess = false, Message = "Security group not found." });
                }
                var currentUserId = User.GetUserId();
                var currentUserGroups = await _userGroupService.GetGroupNamesForUser(currentUserId, tenantId);
                var currentRank =_userGroupService. GetHighestRank(currentUserGroups);
                if ( _userGroupService.GetRank(item.GroupName ?? "")> currentRank)
                {
                    return Ok(new ResponseModel { IsSuccess = false, Message = "You are not allowed to delete this group." });
                }

                // Checks if there are users assigned to this security group
                var usersAssigned = await _userGroupService.GetByUserGroupId(id);
                if (usersAssigned)
                {
                    // Returns error response if users are assigned to the group
                    return Ok(new ResponseModel { IsSuccess = false, Message = "The group cannot be deleted because there are users assigned to it." });
                }

                // Checks if there are roles assigned to this security group
                var rolesAssigned = await _groupPermissionService.GetBySecurityGroupId(id);
                if (rolesAssigned)
                {
                    // Auto-remove roles when no users are assigned
                    try
                    {
                        await _groupPermissionService.RemoveByGroupId(id, tenantId);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Ok(new ResponseModel { IsSuccess = false, Message = ex.Message });
                    }
                }

                // Additional check for user group assignments
                var userGroupAssigned = await _userGroupService.GetByUserGroupsId(id);
                if (userGroupAssigned)
                {
                    // Returns error response if users are assigned to the group
                    return Ok(new ResponseModel { IsSuccess = false, Message = "The group cannot be deleted because there are users assigned to it." });
                }
            }

            // Deletes multiple security groups by their IDs
            try
            {
                await _securityGroupService.MultipleDelete(ids);
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

