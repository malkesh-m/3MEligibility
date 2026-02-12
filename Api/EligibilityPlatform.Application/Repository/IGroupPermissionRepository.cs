using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing group role entities and their relationships.
    /// Extends the base repository interface with additional group-role specific operations.
    /// </summary>
    public interface IGroupPermissionRepository : IRepository<GroupPermission>
    {
        /// <summary>
        /// Retrieves a specific group role relationship by group and role identifiers.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <param name="permissionId">The unique identifier of the permission.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="GroupPermission"/> entity if found.</returns>
        Task<GroupPermission?> GetGroupPermission(int groupId, int permissionId);

        /// <summary>
        /// Checks if a security group exists by its identifier.
        /// </summary>
        /// <param name="groupId">The unique identifier of the security group to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the security group exists; otherwise, false.</returns>
        Task<bool> GetBySecurityGroupId(int groupId);

        /// <summary>
        /// Retrieves all role identifiers associated with a specific group.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of role identifiers associated with the group.</returns>
        Task<List<int>> GetGroupPermissions(int groupId);

        /// <summary>
        /// Retrieves all role identifiers associated with a specific group, filtered by tenant ID.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <param name="tenantId">The tenant ID for multi-tenant isolation.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of role identifiers associated with the group within the tenant.</returns>
    }
}


