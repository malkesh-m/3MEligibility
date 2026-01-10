using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="UserGroup"/> entities.
    /// Provides data access logic for user groups using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserGroupRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class UserGroupRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<UserGroup>(context, httpContext), IUserGroupRepository
    {
        /// <summary>
        /// The database context instance for data operations.
        /// </summary>
        //private readonly EligibilityDbContext _context = context;

        /// <summary>
        /// The HTTP context accessor instance for accessing current HTTP context.
        /// </summary>
        private readonly IHttpContextAccessor _httpContext = httpContext;

        /// <summary>
        /// Checks if any group roles exist for the specified group ID.
        /// </summary>
        /// <param name="groupId">The ID of the group to check.</param>
        /// <returns>True if any group roles exist for the specified group ID; otherwise, false.</returns>
        public async Task<bool> GetByUserGroupId(int groupId)
        {
            // Executes a query to check if any group roles exist for the specified group ID
            return await _context.GroupRoles.AnyAsync(gr => gr.GroupId == groupId);
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
        public List<UserInfo> GetUserByGroupId(int groupId)
        {
            // Creates a LINQ query to join user groups with users and entities
            var query = (from ug in _context.UserGroups
                         join u in _context.Users on ug.UserId equals u.UserId
                         join e in _context.Entities on u.EntityId equals e.EntityId into entityGroup
                         from e in entityGroup.DefaultIfEmpty()
                         where ug.GroupId == groupId
                         select new UserInfo
                         {
                             UserId = u.UserId,
                             GroupId = groupId,
                             UserName = string.IsNullOrEmpty(u.UserName) ? "" : u.UserName,
                             LoginId = string.IsNullOrEmpty(u.LoginId) ? "" : u.LoginId,
                             Email = string.IsNullOrEmpty(u.Email) ? "" : u.Email,
                             EntityName = e != null && !string.IsNullOrEmpty(e.EntityName) ? e.EntityName : ""
                         }).ToList();

            // Returns the list of UserInfo objects
            return query;
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