using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for role permission management operations.
    /// Provides methods for managing permission assignments to security roles.
    /// </summary>
    public interface IRolePermissionService
    {
        /// <summary>
        /// Retrieves all role permission assignments.
        /// </summary>
        /// <returns>A list of <see cref="RolePermissionModel"/> objects containing all role permission assignments.</returns>
        List<RolePermissionModel> GetAll();

        /// <summary>
        /// Adds a new role permission assignment.
        /// </summary>
        /// <param name="rolePermissionModel">The <see cref="RolePermissionModel"/> containing the role permission assignment details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(RolePermissionModel rolePermissionModel);

        /// <summary>
        /// Removes a role permission assignment.
        /// </summary>
        /// <param name="rolePermissionModel">The <see cref="RolePermissionModel"/> containing the role permission assignment details to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(RolePermissionModel rolePermissionModel);

        /// <summary>
        /// Checks if a security role has any permission assignments.
        /// </summary>
        /// <param name="roleId">The unique identifier of the security role to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean value indicating whether the role has permission assignments.</returns>
        Task<bool> GetBySecurityRoleId(int roleId);

        /// <summary>
        /// Retrieves all unassigned permissions for a specific security role.
        /// </summary>
        /// <param name="roleId">The unique identifier of the security role.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="AssignedPermissionModel"/> objects representing unassigned permissions.</returns>
        Task<IList<AssignedPermissionModel>> GetUnAssignedPermissions(int roleId);

        /// <summary>
        /// Retrieves all assigned permissions for a specific security role.
        /// </summary>
        /// <param name="roleId">The unique identifier of the security role.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="AssignedPermissionModel"/> objects representing assigned permissions.</returns>
        Task<IList<AssignedPermissionModel>> GetAssignedPermissions(int roleId);

        /// <summary>
        /// Removes all permission assignments for a specific security role.
        /// </summary>
        /// <param name="roleId">The unique identifier of the security role.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveByRoleId(int roleId, int tenantId);
    }
}
