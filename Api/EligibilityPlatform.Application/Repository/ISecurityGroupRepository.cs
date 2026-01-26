using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing security group entities.
    /// Extends the base repository interface with additional security group-specific operations.
    /// </summary>
    public interface ISecurityGroupRepository : IRepository<SecurityGroup>
    {
        /// <summary>
        /// Retrieves a security group based on user roles.
        /// </summary>
        /// <param name="userRoles">The user roles to match against security group configurations.</param>
        /// <returns>A task that represents the asynchronous operation, containing the matching <see cref="SecurityGroup"/> entity.</returns>
        Task<SecurityGroup?> GetSecurityGroup(string userRoles);
    }
}
