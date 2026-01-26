using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for PCard management operations.
    /// Provides methods for performing CRUD operations, import/export, and bulk actions on PCard records.
    /// </summary>
    public interface IPcardService
    {
        /// <summary>
        /// Retrieves all PCard records for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="PcardListModel"/> objects containing all PCard records for the specified entity.</returns>
        List<PcardListModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves a specific PCard record by its identifier within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the PCard record to retrieve.</param>
        /// <returns>The <see cref="PcardListModel"/> with the specified ID within the given entity.</returns>
        PcardListModel GetById(int tenantId, int id);

        /// <summary>
        /// Adds a new PCard record.
        /// </summary>
        /// <param name="model">The <see cref="PcardAddUpdateModel"/> containing the PCard details to add.</param>
        /// <returns>A task that represents the asynchronous operation, containing a result message.</returns>
        Task<string> Add(PcardAddUpdateModel model);

        /// <summary>
        /// Updates an existing PCard record.
        /// </summary>
        /// <param name="model">The <see cref="PcardUpdateModel"/> containing the updated PCard details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(PcardUpdateModel model);

        /// <summary>
        /// Deletes a PCard record by its identifier within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the PCard record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int tenantId, int id);

        /// <summary>
        /// Removes multiple PCard records within a specific entity in a single operation.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="ids">A list of unique identifiers of the PCard records to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveMultiple(int tenantId, List<int> ids);

        /// <summary>
        /// Imports entities from a stream for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="fileStream">The stream containing the import data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ImportEntities(int tenantId, Stream fileStream);

        /// <summary>
        /// Exports PCard data to a stream for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="selectedPcardIds">A list of PCard identifiers to include in the export.</param>
        /// <returns>A task that represents the asynchronous operation, containing the export stream.</returns>
        Task<Stream> ExportPCards(int tenantId, List<int> selectedPcardIds);

        /// <summary>
        /// Downloads a template file for PCard operations for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation, containing the template file as a byte array.</returns>
        Task<byte[]> DownloadTemplate(int tenantId);

        /// <summary>
        /// Imports PCards from a stream for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="fileStream">The stream containing the import data.</param>
        /// <param name="createdBy">The user who created the import.</param>
        /// <returns>A task that represents the asynchronous operation, containing the import result message.</returns>
        Task<string> ImportPCards(int tenantId, Stream fileStream, string createdBy);
    }
}
