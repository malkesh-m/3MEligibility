using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for history ER management operations.
    /// Provides methods for performing CRUD operations and bulk actions on history ER records.
    /// </summary>
    public interface IHistoryErService
    {
        /// <summary>
        /// Retrieves all history ER records.
        /// </summary>
        /// <returns>A list of <see cref="HistoryErModel"/> objects containing all history ER records.</returns>
        List<HistoryErModel> GetAll();

        /// <summary>
        /// Retrieves a specific history ER record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history ER record to retrieve.</param>
        /// <returns>The <see cref="HistoryErModel"/> with the specified ID.</returns>
        HistoryErModel GetById(int id);

        /// <summary>
        /// Adds a new history ER record.
        /// </summary>
        /// <param name="model">The <see cref="HistoryErModel"/> containing the history ER details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(HistoryErModel model);

        /// <summary>
        /// Updates an existing history ER record.
        /// </summary>
        /// <param name="model">The <see cref="HistoryErModel"/> containing the updated history ER details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(HistoryErModel model);

        /// <summary>
        /// Deletes a history ER record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history ER record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple history ER records in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the history ER records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
