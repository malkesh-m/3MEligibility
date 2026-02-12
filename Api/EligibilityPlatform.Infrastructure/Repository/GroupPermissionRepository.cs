using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="GroupPermission"/> entities.
    /// Provides data access logic for group roles using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GroupPermissionRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class GroupPermissionRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<GroupPermission>(context, userContext), IGroupPermissionRepository
    {

        /// <summary>
        /// Retrieves a specific group role by group ID and role ID.
        /// </summary>
        /// <param name="groupId">The ID of the security group.</param>
        /// <param name="permissionId">The ID of the permission.</param>
        /// <returns>The <see cref="GroupPermission"/> entity matching the specified group and role IDs, or null if not found.</returns>
        public async Task<GroupPermission?> GetGroupPermission(int groupId, int permissionId)
        {
            // Queries the GroupPermissions DbSet from the database context
            // Uses FirstOrDefaultAsync to find the first GroupPermission entity that matches both groupId and permissionId criteria
            // Returns null if no matching entity is found in the database
            // The operation is executed asynchronously to avoid blocking the thread
            return await _context.GroupPermissions
                        .FirstOrDefaultAsync(gr => gr.GroupId == groupId && gr.PermissionId == permissionId);
        }

        /// <summary>
        /// Checks if any group roles exist for the specified security group ID.
        /// </summary>
        /// <param name="groupId">The ID of the security group to check.</param>
        /// <returns>True if any group roles exist for the specified group ID; otherwise, false.</returns>
        public async Task<bool> GetBySecurityGroupId(int groupId)
        {
            // Queries the GroupPermissions DbSet from the database context
            // Uses AnyAsync to check if any GroupPermission entities exist with the specified groupId
            // Returns a boolean value indicating presence (true) or absence (false) of matching records
            // The operation is executed asynchronously to avoid blocking the thread
            return await _context.GroupPermissions
                        .AnyAsync(gr => gr.GroupId == groupId);
        }

        /// <summary>
        /// Retrieves all role IDs associated with a specific security group.
        /// </summary>
        /// <param name="groupId">The ID of the security group.</param>
        /// <returns>A list of role IDs associated with the specified group.</returns>
        public async Task<List<int>> GetGroupPermissions(int groupId)
        {
            // Queries the GroupPermissions DbSet from the database context
            // Filters the GroupPermission entities to only those matching the specified groupId
            // Projects the result to select only the PermissionId property from each matching entity
            // Converts the result to a List<int> containing all role IDs for the group
            // The operation is executed asynchronously to avoid blocking the thread
            return await _context.GroupPermissions
                .Where(rp => rp.GroupId == groupId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the role with the highest PermissionId from the database.
        /// </summary>
        /// <returns>
        /// A Task that represents the asynchronous operation. The task result contains 
        /// the Permission entity with the highest PermissionId, or null if no roles exist.
        /// </returns>
        /// <remarks>
        /// This method is typically used to get the most recently created role
        /// when PermissionId is an auto-incrementing primary key.
        /// </remarks>
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




