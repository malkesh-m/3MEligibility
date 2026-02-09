using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for screen management operations.
    /// Provides methods for performing CRUD operations and bulk actions on screen records.
    /// </summary>
    public interface IScreenService
    {
        /// <summary>
        /// Retrieves all screen records.
        /// </summary>
        /// <returns>A list of <see cref="ScreenModel"/> objects containing all screen records.</returns>
        List<ScreenModel> GetAll();

        /// <summary>
        /// Retrieves a specific screen record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the screen record to retrieve.</param>
        /// <returns>The <see cref="ScreenModel"/> with the specified ID.</returns>
        ScreenModel GetById(int id);

        /// <summary>
        /// Adds a new screen record.
        /// </summary>
        /// <param name="screenModel">The <see cref="ScreenModel"/> containing the screen details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ScreenModel screenModel);

        /// <summary>
        /// Updates an existing screen record.
        /// </summary>
        /// <param name="screenModel">The <see cref="ScreenModel"/> containing the updated screen details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ScreenModel screenModel);

        /// <summary>
        /// Removes a screen record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the screen record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);

        /// <summary>
        /// Deletes multiple screen records in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the screen records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
