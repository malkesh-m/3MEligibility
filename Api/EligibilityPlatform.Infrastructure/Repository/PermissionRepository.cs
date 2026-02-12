using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="Permission"/> entities.
    /// Provides data access logic for roles using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PermissionRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class PermissionRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<Permission>(context, userContext), IPermissionRepository
    {

        /// <summary>
        /// Retrieves the most recently created role based on the PermissionId.
        /// </summary>
        /// <returns>The <see cref="Permission"/> entity with the highest PermissionId, or null if no roles exist.</returns>
        public async Task<Permission?> GetLastPermission()
        {
            // Queries the Permissions DbSet from the database context
            // Orders the roles in descending order by PermissionId to get the highest ID first
            // Returns the first role from the ordered list (highest PermissionId) or null if no roles exist
            // The operation is executed asynchronously to avoid blocking the thread
            return await _context.Permissions
                .OrderByDescending(r => r.PermissionId)
                .FirstOrDefaultAsync();
        }
    }
}



