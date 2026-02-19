using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing security role entities.
    /// Extends the base repository interface with additional security role-specific operations.
    /// </summary>
    public interface ISecurityRoleRepository : IRepository<SecurityRole>
    {
        /// <summary>
        /// Retrieves a security role based on user roles.
        /// </summary>
        /// <param name="userRoles">The user roles to match against security role configurations.</param>
        /// <returns>A task that represents the asynchronous operation, containing the matching <see cref="SecurityRole"/> entity.</returns>
        Task<SecurityRole?> GetSecurityRole(string userRoles);
    }
}
