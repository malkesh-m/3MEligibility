using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
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
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class UserStatusRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<UserStatus>(context, userContext), IUserStatusRepository
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


