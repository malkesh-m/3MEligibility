using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
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
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class SecurityGroupRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<SecurityGroup>(context, httpContext), ISecurityGroupRepository
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
