using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="UserGroup"/> entities.
    /// Provides data access logic for user groups using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserGroupRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class UserGroupRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<UserGroup>(context, userContext), IUserGroupRepository
    {
        /// <summary>
        /// The database context instance for data operations.
        /// </summary>
        //private readonly EligibilityDbContext _context = context;

        /// <summary>
        /// Checks if any group roles exist for the specified group ID.
        /// </summary>
        /// <param name="groupId">The ID of the group to check.</param>
        /// <returns>True if any group roles exist for the specified group ID; otherwise, false.</returns>
        public async Task<bool> GetByUserGroupId(int groupId)
        {
            // Executes a query to check if any group roles exist for the specified group ID
            return await _context.GroupPermissions.AnyAsync(gr => gr.GroupId == groupId);
        }

        /// <summary>
        /// Checks if any user groups exist for the specified group ID.
        /// </summary>
        /// <param name="groupId">The ID of the group to check.</param>
        /// <returns>True if any user groups exist for the specified group ID; otherwise, false.</returns>
        public async Task<bool> GetByUserGroupsId(int groupId)
        {
            // Executes a query to check if any user groups exist for the specified group ID
            return await _context.UserGroups.AnyAsync(gr => gr.GroupId == groupId);
        }

        /// <summary>
        /// Retrieves user information for all users in a specific group.
        /// </summary>
        /// <param name="groupId">The ID of the group to retrieve users from.</param>
        /// <returns>A list of <see cref="UserInfo"/> objects containing user details for the specified group.</returns>
        public List<UserInfo> GetUserByGroupId(int groupId, ApiResponse<List<UserGetModel>> users)
        {
            // Creates a LINQ query to join user groups with users and entities
            var userGroups = _context.UserGroups
             .Where(x => x.GroupId == groupId)
             .ToList();
                    var query =
                from ug in userGroups
                join u in users.Data
                    on ug.UserId equals u.Id
        select new UserInfo
        {
            UserId = u.Id,
            GroupId = groupId,
            UserName = u.DisplayName ?? string.Empty,
            Email = u.Email ?? string.Empty,
            MobileNo=u.MobileNo
        };
            // Returns the list of UserInfo objects
            return [.. query];
        }

        /// <summary>
        /// Retrieves a user group entity for deletion based on user ID and group ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="groupId">The ID of the group.</param>
        /// <returns>The <see cref="UserGroup"/> entity matching the specified user and group IDs, or null if not found.</returns>
        public async Task<UserGroup?> DeleteUserGroupAsync(int userId, int groupId)
        {
            // Executes a query to find a user group entity matching the specified user and group IDs
            return await _context.UserGroups.FirstOrDefaultAsync(g => g.GroupId == groupId && g.UserId == userId);
        }
    }
}

