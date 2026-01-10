using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for history PC management operations.
    /// Provides methods for performing CRUD operations and bulk actions on history PC records.
    /// </summary>
    public interface IHistoryPcService
    {
        /// <summary>
        /// Retrieves all history PC records for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity for which to retrieve history PC records.</param>
        /// <returns>A list of <see cref="HistoryPcModel"/> objects containing all history PC records for the specified entity.</returns>
        List<HistoryPcModel> GetAll(int entityId);

        /// <summary>
        /// Retrieves a specific history PC record by its identifier and entity identifier.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity associated with the history PC record.</param>
        /// <param name="id">The unique identifier of the history PC record to retrieve.</param>
        /// <returns>The <see cref="HistoryPcModel"/> with the specified ID and entity ID.</returns>
        HistoryPcModel GetById(int entityId, int id);

        /// <summary>
        /// Adds a new history PC record.
        /// </summary>
        /// <param name="model">The <see cref="HistoryPcModel"/> containing the history PC details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(HistoryPcModel model);

        /// <summary>
        /// Updates an existing history PC record.
        /// </summary>
        /// <param name="model">The <see cref="HistoryPcModel"/> containing the updated history PC details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(HistoryPcModel model);

        /// <summary>
        /// Deletes a history PC record by its identifier and entity identifier.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity associated with the history PC record.</param>
        /// <param name="id">The unique identifier of the history PC record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int entityId, int id);

        /// <summary>
        /// Deletes multiple history PC records in a single operation for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity associated with the history PC records.</param>
        /// <param name="ids">A list of unique identifiers of the history PC records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(int entityId, List<int> ids);
    }
}
