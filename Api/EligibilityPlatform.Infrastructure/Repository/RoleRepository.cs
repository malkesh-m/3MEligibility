using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="Role"/> entities.
    /// Provides data access logic for roles using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RoleRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class RoleRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<Role>(context, httpContext), IRoleRepository
    {

        /// <summary>
        /// Retrieves the most recently created role based on the RoleId.
        /// </summary>
        /// <returns>The <see cref="Role"/> entity with the highest RoleId, or null if no roles exist.</returns>
        public async Task<Role?> GetLastRole()
        {
            // Queries the Roles DbSet from the database context
            // Orders the roles in descending order by RoleId to get the highest ID first
            // Returns the first role from the ordered list (highest RoleId) or null if no roles exist
            // The operation is executed asynchronously to avoid blocking the thread
            return await _context.Roles
                .OrderByDescending(r => r.RoleId)
                .FirstOrDefaultAsync();
        }
    }
}
