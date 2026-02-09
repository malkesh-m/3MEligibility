using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for bulk import operations.
    /// Provides methods for downloading templates, importing data, and managing import history.
    /// </summary>
    public interface IBulkImportService
    {
        /// <summary>
        /// Downloads a template file for bulk import based on the specified entity and list.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to download the template.</param>
        /// <param name="selectedList">The specific list or type of template to download.</param>
        /// <returns>A byte array containing the template file data.</returns>
        Task<byte[]> DownloadTemplate(int tenantId, string selectedList);

        /// <summary>
        /// Downloads a previously imported file by its document identifier.
        /// </summary>
        /// <param name="documentId">The unique identifier of the imported document to download.</param>
        /// <returns>A byte array containing the imported file data.</returns>
        Task<byte[]> DownloadImportedFile(int documentId);

        /// <summary>
        /// Performs bulk import of data from the provided file stream.
        /// </summary>
        /// <param name="fileStream">The stream containing the file data to import.</param>
        /// <param name="fileName">The name of the file being imported.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import operation.</param>
        /// <returns>A string containing the result or status of the import operation.</returns>
        Task<string> BulkImport(Stream fileStream, string fileName, string createdBy, int tenantId);

        /// <summary>
        /// Retrieves all import history records.
        /// </summary>
        /// <returns>A list of <see cref="ImportDocument"/> objects representing the import history.</returns>
        List<ImportDocument> GetAllImportHistory();
    }
}
