using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for group role management operations.
    /// Provides methods for managing role assignments to security groups.
    /// </summary>
    public interface IGroupRoleService
    {
        /// <summary>
        /// Retrieves all group role assignments.
        /// </summary>
        /// <returns>A list of <see cref="GroupRoleModel"/> objects containing all group role assignments.</returns>
        List<GroupRoleModel> GetAll();

        /// <summary>
        /// Adds a new group role assignment.
        /// </summary>
        /// <param name="groupRoleModel">The <see cref="GroupRoleModel"/> containing the group role assignment details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(GroupRoleModel groupRoleModel);

        /// <summary>
        /// Removes a group role assignment.
        /// </summary>
        /// <param name="groupRoleModel">The <see cref="GroupRoleModel"/> containing the group role assignment details to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(GroupRoleModel groupRoleModel);

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
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="AssignedRoleModel"/> objects representing unassigned roles.</returns>
        Task<IList<AssignedRoleModel>> GetUnAssignedRoles(int groupId);

        /// <summary>
        /// Retrieves all assigned roles for a specific security group.
        /// </summary>
        /// <param name="groupId">The unique identifier of the security group.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="AssignedRoleModel"/> objects representing assigned roles.</returns>
        Task<IList<AssignedRoleModel>> GetAssignedRoles(int groupId);
    }
}
