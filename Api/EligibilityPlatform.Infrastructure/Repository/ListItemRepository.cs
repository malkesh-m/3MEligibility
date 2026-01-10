using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="ListItem"/> entities.
    /// Provides data access logic for list items using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ListItemRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class ListItemRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<ListItem>(context, httpContext), IListItemRepository
    {
        public bool ExistsInMemory(int listId, string nameValue)
        {
            return _context.ChangeTracker
                .Entries<ListItem>()
                .Any(e =>
                    e.Entity.ListId == listId &&
                    e.State == EntityState.Added &&
                    string.Equals(
                        e.Entity.ItemName?.Trim(),
                        nameValue?.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }
    }
}
