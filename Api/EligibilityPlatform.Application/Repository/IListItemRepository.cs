using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing list item entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IListItemRepository : IRepository<ListItem>
    {
        bool ExistsInMemory(int listId, string nameValue);

    }
}

