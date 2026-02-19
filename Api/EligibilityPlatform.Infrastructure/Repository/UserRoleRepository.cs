using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="UserRole"/> entities.
    /// Provides data access logic for user roles using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserRoleRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class UserRoleRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<UserRole>(context, userContext), IUserRoleRepository
    {
        /// <summary>
        /// Checks if any role roles exist for the specified role ID.
        /// </summary>
        /// <param name="roleId">The ID of the role to check.</param>
        /// <returns>True if any role roles exist for the specified role ID; otherwise, false.</returns>
        public async Task<bool> GetByUserRoleId(int roleId)
        {
            // Executes a query to check if any users exist for the specified role ID
            return await _context.UserRoles.AnyAsync(gr => gr.RoleId == roleId);
        }

        /// <summary>
        /// Checks if any user roles exist for the specified role ID.
        /// </summary>
        /// <param name="roleId">The ID of the role to check.</param>
        /// <returns>True if any user roles exist for the specified role ID; otherwise, false.</returns>
        public async Task<bool> GetByUserRolesId(int roleId)
        {
            // Executes a query to check if any user roles exist for the specified role ID
            return await _context.UserRoles.AnyAsync(gr => gr.RoleId == roleId);
        }

        /// <summary>
        /// Retrieves user information for all users in a specific role.
        /// </summary>
        /// <param name="roleId">The ID of the role to retrieve users from.</param>
        /// <returns>A list of <see cref="UserInfo"/> objects containing user details for the specified role.</returns>
        public List<UserInfo> GetUserByRoleId(int roleId, ApiResponse<List<UserGetModel>> users)
        {
            // Creates a LINQ query to join user roles with users and entities
            var userRoles = _context.UserRoles
             .Where(x => x.RoleId == roleId)
             .ToList();
                    var query =
                from ur in userRoles
                join u in users.Data
                    on ur.UserId equals u.Id
        select new UserInfo
        {
            UserId = u.Id,
            RoleId = roleId,
            UserName = u.DisplayName ?? string.Empty,
            Email = u.Email ?? string.Empty,
            MobileNo=u.MobileNo
        };
            // Returns the list of UserInfo objects
            return [.. query];
        }

        /// <summary>
        /// Retrieves a user role entity for deletion based on user ID and role ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>The <see cref="UserRole"/> entity matching the specified user and role IDs, or null if not found.</returns>
        public async Task<UserRole?> DeleteUserRoleAsync(int userId, int roleId)
        {
            // Executes a query to find a user role entity matching the specified user and role IDs
            return await _context.UserRoles.FirstOrDefaultAsync(g => g.RoleId == roleId && g.UserId == userId);
        }
    }
}
