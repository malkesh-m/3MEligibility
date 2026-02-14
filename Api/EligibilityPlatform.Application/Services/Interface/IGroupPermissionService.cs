using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for group role management operations.
    /// Provides methods for managing role assignments to security groups.
    /// </summary>
    public interface IGroupPermissionService
    {
        /// <summary>
        /// Retrieves all group role assignments.
        /// </summary>
        /// <returns>A list of <see cref="GroupPermissionModel"/> objects containing all group role assignments.</returns>
        List<GroupPermissionModel> GetAll();

        /// <summary>
        /// Adds a new group role assignment.
        /// </summary>
        /// <param name="groupPermissionModel">The <see cref="GroupPermissionModel"/> containing the group role assignment details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(GroupPermissionModel groupPermissionModel);

        /// <summary>
        /// Removes a group role assignment.
        /// </summary>
        /// <param name="groupPermissionModel">The <see cref="GroupPermissionModel"/> containing the group role assignment details to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(GroupPermissionModel groupPermissionModel);

        /// <summary>
        /// Checks if a security group has any role assignments.
        /// </summary>
        /// <param name="groupId">The unique identifier of the security group to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean value indicating whether the group has role assignments.</returns>
        Task<bool> GetBySecurityGroupId(int groupId);

        /// <summary>
        /// Retrieves all unassigned roles for a specific security group.
        /// </summary>
        /// <param name="groupId">The unique identifier of the security group.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="AssignedPermissionModel"/> objects representing unassigned roles.</returns>
        Task<IList<AssignedPermissionModel>> GetUnAssignedPermissions(int groupId);

        /// <summary>
        /// Retrieves all assigned roles for a specific security group.
        /// </summary>
        /// <param name="groupId">The unique identifier of the security group.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="AssignedPermissionModel"/> objects representing assigned roles.</returns>
        Task<IList<AssignedPermissionModel>> GetAssignedPermissions(int groupId);

        /// <summary>
        /// Removes all role assignments for a specific security group.
        /// </summary>
        /// <param name="groupId">The unique identifier of the security group.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveByGroupId(int groupId, int tenantId);
    }
}

