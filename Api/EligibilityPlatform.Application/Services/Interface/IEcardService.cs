using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for e-card management operations.
    /// Provides methods for performing CRUD operations, import/export, and bulk actions on e-cards.
    /// </summary>
    public interface IEcardService
    {
        /// <summary>
        /// Retrieves all e-cards for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity to retrieve e-cards for.</param>
        /// <returns>A list of <see cref="EcardListModel"/> objects containing all e-cards.</returns>
        List<EcardListModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves a specific e-card by its identifier within a given entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the e-card to retrieve.</param>
        /// <returns>The <see cref="EcardListModel"/> with the specified ID.</returns>
        EcardListModel GetById(int tenantId, int id);

        /// <summary>
        /// Adds a new e-card to the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="model">The <see cref="EcardAddUpdateModel"/> containing the e-card details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(int tenantId, EcardAddUpdateModel model);

        /// <summary>
        /// Updates an existing e-card.
        /// </summary>
        /// <param name="model">The <see cref="EcardUpdateModel"/> containing the updated e-card details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(EcardUpdateModel model);

        /// <summary>
        /// Deletes an e-card by its identifier within a given entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the e-card to delete.</param>
        /// <returns>A string containing the result or status of the deletion operation.</returns>
        Task<string> Delete(int tenantId, int id);

        /// <summary>
        /// Deletes multiple e-cards within a given entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="ids">A list of unique identifiers of the e-cards to delete.</param>
        /// <returns>A string containing the result or status of the bulk deletion operation.</returns>
        Task<string> RemoveMultiple(int tenantId, List<int> ids);

        /// <summary>
        /// Downloads a template file for e-card import for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A byte array containing the template file data.</returns>
        Task<byte[]> DownloadTemplate(int tenantId);

        /// <summary>
        /// Imports e-cards from a file stream for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="fileStream">The stream containing the e-card data to import.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A string containing the result or status of the import operation.</returns>
        Task<string> ImportECard(int tenantId, Stream fileStream, string createdBy);

        /// <summary>
        /// Exports e-cards to a stream for the specified entity and selected e-card IDs.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="ids">A list of e-card IDs to export.</param>
        /// <returns>A stream containing the exported e-card data.</returns>
        Task<Stream> ExportECard(int tenantId, List<int> ids);
    }
}
