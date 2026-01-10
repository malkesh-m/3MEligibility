using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="GroupRole"/> entities.
    /// Provides data access logic for group roles using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GroupRoleRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class GroupRoleRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<GroupRole>(context, httpContext), IGroupRoleRepository
    {

        /// <summary>
        /// Retrieves a specific group role by group ID and role ID.
        /// </summary>
        /// <param name="groupId">The ID of the security group.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>The <see cref="GroupRole"/> entity matching the specified group and role IDs, or null if not found.</returns>
        public async Task<GroupRole?> GetGroupRole(int groupId, int roleId)
        {
            // Queries the GroupRoles DbSet from the database context
            // Uses FirstOrDefaultAsync to find the first GroupRole entity that matches both groupId and roleId criteria
            // Returns null if no matching entity is found in the database
            // The operation is executed asynchronously to avoid blocking the thread
            return await _context.GroupRoles
                        .FirstOrDefaultAsync(gr => gr.GroupId == groupId && gr.RoleId == roleId);
        }

        /// <summary>
        /// Checks if any group roles exist for the specified security group ID.
        /// </summary>
        /// <param name="groupId">The ID of the security group to check.</param>
        /// <returns>True if any group roles exist for the specified group ID; otherwise, false.</returns>
        public async Task<bool> GetBySecurityGroupId(int groupId)
        {
            // Queries the GroupRoles DbSet from the database context
            // Uses AnyAsync to check if any GroupRole entities exist with the specified groupId
            // Returns a boolean value indicating presence (true) or absence (false) of matching records
            // The operation is executed asynchronously to avoid blocking the thread
            return await _context.GroupRoles
                        .AnyAsync(gr => gr.GroupId == groupId);
        }

        /// <summary>
        /// Retrieves all role IDs associated with a specific security group.
        /// </summary>
        /// <param name="groupId">The ID of the security group.</param>
        /// <returns>A list of role IDs associated with the specified group.</returns>
        public async Task<List<int>> GetGroupRoles(int groupId)
        {
            // Queries the GroupRoles DbSet from the database context
            // Filters the GroupRole entities to only those matching the specified groupId
            // Projects the result to select only the RoleId property from each matching entity
            // Converts the result to a List<int> containing all role IDs for the group
            // The operation is executed asynchronously to avoid blocking the thread
            return await _context.GroupRoles
                .Where(rp => rp.GroupId == groupId)
                .Select(rp => rp.RoleId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the role with the highest RoleId from the database.
        /// </summary>
        /// <returns>
        /// A Task that represents the asynchronous operation. The task result contains 
        /// the Role entity with the highest RoleId, or null if no roles exist.
        /// </returns>
        /// <remarks>
        /// This method is typically used to get the most recently created role
        /// when RoleId is an auto-incrementing primary key.
        /// </remarks>
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

