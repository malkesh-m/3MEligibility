using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="UserStatus"/> entities.
    /// Provides data access logic for user statuses using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserStatusRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class UserStatusRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<UserStatus>(context, httpContext), IUserStatusRepository
    {

        /// <summary>
        /// Retrieves the most recently created user status based on the StatusId.
        /// </summary>
        /// <returns>The <see cref="UserStatus"/> entity with the highest StatusId, or null if no user statuses exist.</returns>
        public async Task<UserStatus?> GetLastUserStatus()
        {// Retrieves the most recent UserStatus by ordering StatusId in descending order and returning the first result
            return await _context.UserStatuses
                .OrderByDescending(r => r.StatusId)
                .FirstOrDefaultAsync();
        }
    }
}
