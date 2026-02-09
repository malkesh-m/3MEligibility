using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for managed list operations.
    /// Provides methods for performing CRUD operations, import/export, and bulk actions on managed lists.
    /// </summary>
    public interface IManagedListService
    {
        /// <summary>
        /// Retrieves all managed lists for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to retrieve managed lists.</param>
        /// <returns>A list of <see cref="ManagedListGetModel"/> objects containing all managed lists for the specified entity.</returns>
        List<ManagedListGetModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves a specific managed list by its identifier and entity identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the managed list.</param>
        /// <param name="id">The unique identifier of the managed list to retrieve.</param>
        /// <returns>The <see cref="ManagedListGetModel"/> with the specified ID and entity ID.</returns>
        ManagedListGetModel GetById(int tenantId, int id);

        /// <summary>
        /// Adds a new managed list.
        /// </summary>
        /// <param name="model">The <see cref="ManagedListAddUpdateModel"/> containing the managed list details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ManagedListAddUpdateModel model);

        /// <summary>
        /// Updates an existing managed list.
        /// </summary>
        /// <param name="model">The <see cref="ManagedListUpdateModel"/> containing the updated managed list details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ManagedListUpdateModel model);

        /// <summary>
        /// Deletes a managed list by its identifier and entity identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the managed list.</param>
        /// <param name="id">The unique identifier of the managed list to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int tenantId, int id);

        /// <summary>
        /// Deletes multiple managed lists in a single operation for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the managed lists.</param>
        /// <param name="ids">A list of unique identifiers of the managed lists to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(int tenantId, List<int> ids);

        /// <summary>
        /// Imports managed lists from a file stream for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to import managed lists.</param>
        /// <param name="fileStream">The stream containing the managed list data to import.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, containing a status message string.</returns>
        Task<string> ImportList(int tenantId, Stream fileStream, string createdBy);

        /// <summary>
        /// Downloads a template file for managed list import.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a byte array with the template file data.</returns>
        Task<byte[]> DownloadTemplate();

        /// <summary>
        /// Exports managed lists to a stream for the selected list IDs and specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to export managed lists.</param>
        /// <param name="selectedListIds">A list of managed list IDs to export.</param>
        /// <returns>A task that represents the asynchronous operation, containing a stream with the exported managed list data.</returns>
        Task<Stream> ExportLists(int tenantId, List<int> selectedListIds);
    }
}
