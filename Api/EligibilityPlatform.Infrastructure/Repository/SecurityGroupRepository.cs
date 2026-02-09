using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="SecurityGroup"/> entities.
    /// Provides data access logic for security groups using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SecurityGroupRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class SecurityGroupRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<SecurityGroup>(context, userContext), ISecurityGroupRepository
    {

        /// <summary>
        /// Retrieves a security group by converting the user roles string to a group ID.
        /// </summary>
        /// <param name="userRoles">The user roles string to convert to a group ID.</param>
        /// <returns>The <see cref="SecurityGroup"/> entity matching the converted group ID, or null if not found.</returns>
        public async Task<SecurityGroup?> GetSecurityGroup(string userRoles)
        {   // Retrieves the first SecurityGroup entity where GroupId matches the integer conversion of userRoles parameter
            return await _context.SecurityGroups
                .FirstOrDefaultAsync(x => x.GroupId == Convert.ToInt32(userRoles));
        }
    }
}


