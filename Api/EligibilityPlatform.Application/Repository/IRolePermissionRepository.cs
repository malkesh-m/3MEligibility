using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing role permission entities and their relationships.
    /// Extends the base repository interface with additional role-permission specific operations.
    /// </summary>
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        /// <summary>
        /// Retrieves a specific role permission relationship by role and permission identifiers.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role.</param>
        /// <param name="permissionId">The unique identifier of the permission.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="RolePermission"/> entity if found.</returns>
        Task<RolePermission?> GetRolePermission(int roleId, int permissionId);

        /// <summary>
        /// Checks if a security role exists by its identifier.
        /// </summary>
        /// <param name="roleId">The unique identifier of the security role to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the security role exists; otherwise, false.</returns>
        Task<bool> GetBySecurityRoleId(int roleId);

        /// <summary>
        /// Retrieves all permission identifiers associated with a specific role.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of permission identifiers associated with the role.</returns>
        Task<List<int>> GetRolePermissions(int roleId);
    }
}
