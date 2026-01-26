using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing import document entities.
    /// Extends the base repository interface with additional import document specific operations.
    /// </summary>
    public interface IImportDocumentRepository : IRepository<ImportDocument>
    {
        /// <summary>
        /// Adds a new import document record asynchronously.
        /// </summary>
        /// <param name="importDocument">The <see cref="ImportDocument"/> entity to add.</param>
        /// <returns>A task that represents the asynchronous operation, containing the added <see cref="ImportDocument"/> entity.</returns>
        Task<ImportDocument> AddAsync(ImportDocument importDocument);

        /// <summary>
        /// Updates an existing import document record asynchronously.
        /// </summary>
        /// <param name="importDocument">The <see cref="ImportDocument"/> entity with updated information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateAsync(ImportDocument importDocument);

        /// <summary>
        /// Retrieves all import history records.
        /// </summary>
        /// <returns>A list of <see cref="ImportDocument"/> entities representing the complete import history.</returns>
        List<ImportDocument> GetAllImportHistory();
    }
}
