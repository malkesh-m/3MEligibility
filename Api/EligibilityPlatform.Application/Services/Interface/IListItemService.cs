using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for list item management operations.
    /// Provides methods for performing CRUD operations, import/export, and bulk actions on list items.
    /// </summary>
    public interface IListItemService
    {
        /// <summary>
        /// Retrieves all list items.
        /// </summary>
        /// <returns>A list of <see cref="ListItemModel"/> objects containing all list items.</returns>
        List<ListItemModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves a specific list item by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the list item to retrieve.</param>
        /// <returns>The <see cref="ListItemModel"/> with the specified ID.</returns>
        ListItemModel GetById(int id,int tenantId);

        /// <summary>
        /// Adds a new list item.
        /// </summary>
        /// <param name="model">The <see cref="ListItemModel"/> containing the list item details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ListItemCreateUpdateModel model);

        /// <summary>
        /// Updates an existing list item.
        /// </summary>
        /// <param name="model">The <see cref="ListItemModel"/> containing the updated list item details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ListItemCreateUpdateModel model);

        /// <summary>
        /// Deletes a list item by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the list item to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple list items in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the list items to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);

        /// <summary>
        /// Exports list items to a stream for the selected list item IDs.
        /// </summary>
        /// <param name="selectedListItemIds">A list of list item IDs to export.</param>
        /// <returns>A task that represents the asynchronous operation, containing a stream with the exported list item data.</returns>
        Task<Stream> ExportListIteam(List<int> selectedListItemIds);

        /// <summary>
        /// Imports list items from a file stream.
        /// </summary>
        /// <param name="fileStream">The stream containing the list item data to import.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, containing a status message string.</returns>
        Task<string> ImportListIteams(Stream fileStream, string createdBy);

        /// <summary>
        /// Downloads a template file for list item import.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to download the template.</param>
        /// <returns>A task that represents the asynchronous operation, containing a byte array with the template file data.</returns>
        Task<byte[]> DownloadTemplate(int tenantId);
    }
}
