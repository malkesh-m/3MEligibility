using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing screen entities.
    /// Extends the base repository interface with additional screen-specific operations.
    /// </summary>
    public interface IScreenRepository : IRepository<Screen>
    {
        /// <summary>
        /// Retrieves screen identifiers associated with a specific controller.
        /// </summary>
        /// <param name="controllerName">The name of the controller to retrieve screens for.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of screen identifiers.</returns>
        Task<List<int>> GetScreen(string controllerName);
    }
}
