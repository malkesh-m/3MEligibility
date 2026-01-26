using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing group role entities and their relationships.
    /// Extends the base repository interface with additional group-role specific operations.
    /// </summary>
    public interface IGroupRoleRepository : IRepository<GroupRole>
    {
        /// <summary>
        /// Retrieves a specific group role relationship by group and role identifiers.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <param name="roleId">The unique identifier of the role.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="GroupRole"/> entity if found.</returns>
        Task<GroupRole?> GetGroupRole(int groupId, int roleId);

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
        Task<List<int>> GetGroupRoles(int groupId);
    }
}

