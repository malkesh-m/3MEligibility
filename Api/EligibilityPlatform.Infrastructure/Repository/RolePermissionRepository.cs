using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="RolePermission"/> entities.
    /// Provides data access logic for role permissions using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RolePermissionRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class RolePermissionRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<RolePermission>(context, userContext), IRolePermissionRepository
    {

        /// <summary>
        /// Retrieves a specific role permission by role ID and permission ID.
        /// </summary>
        /// <param name="roleId">The ID of the security role.</param>
        /// <param name="permissionId">The ID of the permission.</param>
        /// <returns>The <see cref="RolePermission"/> entity matching the specified role and permission IDs, or null if not found.</returns>
        public async Task<RolePermission?> GetRolePermission(int roleId, int permissionId)
        {
            // Queries the RolePermissions DbSet from the database context
            return await _context.RolePermissions
                        .FirstOrDefaultAsync(gr => gr.RoleId == roleId && gr.PermissionId == permissionId);
        }

        /// <summary>
        /// Checks if any role permissions exist for the specified security role ID.
        /// </summary>
        /// <param name="roleId">The ID of the security role to check.</param>
        /// <returns>True if any role permissions exist for the specified role ID; otherwise, false.</returns>
        public async Task<bool> GetBySecurityRoleId(int roleId)
        {
            // Queries the RolePermissions DbSet from the database context
            return await _context.RolePermissions
                        .AnyAsync(gr => gr.RoleId == roleId);
        }

        /// <summary>
        /// Retrieves all permission IDs associated with a specific security role.
        /// </summary>
        /// <param name="roleId">The ID of the security role.</param>
        /// <returns>A list of permission IDs associated with the specified role.</returns>
        public async Task<List<int>> GetRolePermissions(int roleId)
        {
            // Queries the RolePermissions DbSet from the database context
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the permission with the highest PermissionId from the database.
        /// </summary>
        /// <returns>
        /// A Task that represents the asynchronous operation. The task result contains 
        /// the Permission entity with the highest PermissionId, or null if no permissions exist.
        /// </returns>
        public async Task<Permission?> GetLastPermission()
        {
            // Queries the Permissions DbSet from the database context
            return await _context.Permissions
                .OrderByDescending(r => r.PermissionId)
                .FirstOrDefaultAsync();
        }
    }
}
