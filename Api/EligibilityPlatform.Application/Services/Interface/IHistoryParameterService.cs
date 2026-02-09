using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for history parameter management operations.
    /// Provides methods for performing CRUD operations and bulk actions on history parameter records.
    /// </summary>
    public interface IHistoryParameterService
    {
        /// <summary>
        /// Retrieves all history parameter records.
        /// </summary>
        /// <returns>A list of <see cref="HistoryParameterModel"/> objects containing all history parameter records.</returns>
        List<HistoryParameterModel> GetAll();

        /// <summary>
        /// Retrieves a specific history parameter record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history parameter record to retrieve.</param>
        /// <returns>The <see cref="HistoryParameterModel"/> with the specified ID.</returns>
        HistoryParameterModel GetById(int id);

        /// <summary>
        /// Adds a new history parameter record.
        /// </summary>
        /// <param name="model">The <see cref="HistoryParameterModel"/> containing the history parameter details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(HistoryParameterModel model);

        /// <summary>
        /// Updates an existing history parameter record.
        /// </summary>
        /// <param name="model">The <see cref="HistoryParameterModel"/> containing the updated history parameter details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(HistoryParameterModel model);

        /// <summary>
        /// Deletes a history parameter record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history parameter record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple history parameter records in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the history parameter records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
