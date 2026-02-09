using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="ListItem"/> entities.
    /// Provides data access logic for list items using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ListItemRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class ListItemRepository(
        EligibilityDbContext context,
        IUserContextService userContext) : Repository<ListItem>(context, userContext), IListItemRepository
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


