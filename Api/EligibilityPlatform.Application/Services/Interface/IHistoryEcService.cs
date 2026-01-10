using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for history EC management operations.
    /// Provides methods for performing CRUD operations and bulk actions on history EC records.
    /// </summary>
    public interface IHistoryEcService
    {
        /// <summary>
        /// Retrieves all history EC records.
        /// </summary>
        /// <returns>A list of <see cref="HistoryEcModel"/> objects containing all history EC records.</returns>
        List<HistoryEcModel> GetAll();

        /// <summary>
        /// Retrieves a specific history EC record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history EC record to retrieve.</param>
        /// <returns>The <see cref="HistoryEcModel"/> with the specified ID.</returns>
        HistoryEcModel GetById(int id);

        /// <summary>
        /// Adds a new history EC record.
        /// </summary>
        /// <param name="model">The <see cref="HistoryEcModel"/> containing the history EC details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(HistoryEcModel model);

        /// <summary>
        /// Updates an existing history EC record.
        /// </summary>
        /// <param name="model">The <see cref="HistoryEcModel"/> containing the updated history EC details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(HistoryEcModel model);

        /// <summary>
        /// Deletes a history EC record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history EC record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple history EC records in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the history EC records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
