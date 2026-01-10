using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for Erule management operations.
    /// Provides methods for performing CRUD operations, import/export, publishing, and status management on Erules.
    /// </summary>
    public interface IEruleService
    {
        /// <summary>
        /// Retrieves all Erules for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity for which to retrieve Erules.</param>
        /// <returns>A list of <see cref="EruleListModel"/> objects containing all Erules for the specified entity.</returns>
        List<EruleListModel> GetAll(int entityId);

        /// <summary>
        /// Retrieves a specific Erule by its identifier and entity identifier.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity associated with the Erule.</param>
        /// <param name="id">The unique identifier of the Erule to retrieve.</param>
        /// <returns>The <see cref="EruleListModel"/> with the specified ID and entity ID.</returns>
        EruleListModel GetById(int entityId, int id);

        /// <summary>
        /// Updates an existing Erule.
        /// </summary>
        /// <param name="model">The <see cref="EruleUpdateModel"/> containing the updated Erule details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(EruleUpdateModel model);

        /// <summary>
        /// Creates a new Erule.
        /// </summary>
        /// <param name="model">The <see cref="EruleCreateModel"/> containing the Erule details to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Create(EruleCreateModel model);

        /// <summary>
        /// Deletes an Erule by its identifier and entity identifier.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity associated with the Erule.</param>
        /// <param name="id">The unique identifier of the Erule to delete.</param>
        /// <returns>A task that represents the asynchronous operation, containing a status message string.</returns>
        Task<string> Delete(int entityId, int id);

        /// <summary>
        /// Deletes multiple Erules in a single operation for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity associated with the Erules.</param>
        /// <param name="ids">A list of unique identifiers of the Erules to delete.</param>
        /// <returns>A task that represents the asynchronous operation, containing a status message string.</returns>
        Task<string> RemoveMultiple(int entityId, List<int> ids);

        /// <summary>
        /// Downloads a template file for Erule import.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity for which to download the template.</param>
        /// <returns>A task that represents the asynchronous operation, containing a byte array with the template file data.</returns>
        Task<byte[]> DownloadTemplate(int entityId);
        Task<byte[]> DownloadTemplateEruleMaster(int entityId);

        /// <summary>
        /// Imports Erules from a file stream for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity for which to import Erules.</param>
        /// <param name="fileStream">The stream containing the Erule data to import.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, containing a status message string.</returns>
        Task<string> ImportErule(int entityId, Stream fileStream, string createdBy);
        Task<string> ImportEruleMaster(int entityId, Stream fileStream, string createdBy);

        /// <summary>
        /// Exports Erules to a stream for the selected Erule IDs and specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity for which to export Erules.</param>
        /// <param name="selectedEruleIds">A list of Erule IDs to export.</param>
        /// <returns>A task that represents the asynchronous operation, containing a stream with the exported Erule data.</returns>
        Task<Stream> ExportErule(int entityId, List<int> selectedEruleIds);

        /// <summary>
        /// Publishes a draft Erule to make it active.
        /// </summary>
        /// <param name="draftEruleId">The unique identifier of the draft Erule to publish.</param>
        /// <param name="entityId">The unique identifier of the entity associated with the Erule.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PublishDraftAsync(int draftEruleId, int entityId);

        /// <summary>
        /// Updates the active status of an Erule.
        /// </summary>
        /// <param name="eruleId">The unique identifier of the Erule to update.</param>
        /// <param name="entityId">The unique identifier of the entity associated with the Erule.</param>
        /// <param name="isActive">The new active status value (true for active, false for inactive).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateStatusAsync(int eruleId, int entityId, bool isActive);

        /// <summary>
        /// Retrieves all Erules associated with a specific Erule master and entity.
        /// </summary>
        /// <param name="eruleMasterId">The unique identifier of the Erule master for which to retrieve Erules.</param>
        /// <param name="entityId">The unique identifier of the entity for which to retrieve Erules.</param>
        /// <returns>A list of <see cref="EruleListModel"/> objects containing Erules for the specified Erule master and entity.</returns>
        Task<List<EruleListModel>> GetAllByEruleMasterId(int eruleMasterId, int entityId);

        /// <summary>
        /// Updates an existing Erule using a combined create/update model.
        /// </summary>
        /// <param name="model">The <see cref="EruleCreateOrUpdateModel"/> containing the updated Erule details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateErule(EruleCreateOrUpdateModel model);
    }
}
