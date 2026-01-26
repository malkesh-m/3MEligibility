using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="Screen"/> entities.
    /// Provides data access logic for screens using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ScreenRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class ScreenRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<Screen>(context, httpContext), IScreenRepository
    {

        /// <summary>
        /// Retrieves screen IDs that match the specified controller name pattern.
        /// </summary>
        /// <param name="controllerName">The controller name to search for in screen names.</param>
        /// <returns>A list of screen IDs that match the controller name pattern.</returns>
        public async Task<List<int>> GetScreen(string controllerName)
        {
            // Constructs a LINQ query to select ScreenIds from the Screens DbSet
            var query = from rp in _context.Screens
                            // Filters screens where ScreenName contains the controllerName (case-insensitive search using EF.Functions.Like)
                        where EF.Functions.Like(rp.ScreenName, "%" + controllerName + "%")
                        // Projects the result to select only the ScreenId property
                        select rp.ScreenId;

            // Executes the query asynchronously and returns the result as a List<int>
            return await query.ToListAsync();
        }
    }
}
