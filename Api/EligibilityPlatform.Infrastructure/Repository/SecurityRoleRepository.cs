using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="SecurityRole"/> entities.
    /// Provides data access logic for security roles using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SecurityRoleRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class SecurityRoleRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<SecurityRole>(context, userContext), ISecurityRoleRepository
    {

        /// <summary>
        /// Retrieves a security role by converting the user roles string to a role ID.
        /// </summary>
        /// <param name="userRoles">The user roles string to convert to a role ID.</param>
        /// <returns>The <see cref="SecurityRole"/> entity matching the converted role ID, or null if not found.</returns>
        public async Task<SecurityRole?> GetSecurityRole(string userRoles)
        {   // Retrieves the first SecurityRole entity where RoleId matches the integer conversion of userRoles parameter
            return await _context.SecurityRoles
                .FirstOrDefaultAsync(x => x.RoleId == Convert.ToInt32(userRoles));
        }
    }
}
