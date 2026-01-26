using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace MEligibilityPlatform.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for managing <see cref="ImportDocument"/> entities.
    /// Provides data access logic for import documents using the base <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ImportDocumentRepository"/> class.
    /// </remarks>
    /// <param name="context">The database context used for data operations.</param>
    /// <param name="httpContext">Provides access to the current HTTP context for user-related data.</param>
    public class ImportDocumentRepository(
        EligibilityDbContext context,
        IHttpContextAccessor httpContext) : Repository<ImportDocument>(context, httpContext), IImportDocumentRepository
    {

        /// <summary>
        /// Adds a new import document asynchronously.
        /// </summary>
        /// <param name="importDocument">The import document entity to add.</param>
        /// <returns>The added <see cref="ImportDocument"/> entity.</returns>
        public async Task<ImportDocument> AddAsync(ImportDocument importDocument)
        {
            await _context.ImportDocuments.AddAsync(importDocument);
            await _context.SaveChangesAsync();
            return importDocument;
        }

        /// <summary>
        /// Retrieves an import document by its unique identifier.
        /// </summary>
        /// <param name="documentId">The ID of the import document to retrieve.</param>
        /// <returns>The <see cref="ImportDocument"/> entity with the specified ID, or null if not found.</returns>
        public ImportDocument? ImportById(int documentId)
        {
            return _context.ImportDocuments.Where(x => x.Id == documentId).FirstOrDefault();
        }

        /// <summary>
        /// Updates an existing import document asynchronously.
        /// </summary>
        /// <param name="importDocument">The import document entity to update.</param>
        public async Task UpdateAsync(ImportDocument importDocument)
        {
            _context.ImportDocuments.Update(importDocument);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Error saving changes: " + e.Message, e);
            }
        }

        /// <summary>
        /// Retrieves all import document history records.
        /// </summary>
        /// <returns>A list of all <see cref="ImportDocument"/> entities.</returns>
        public List<ImportDocument> GetAllImportHistory()
        {
            return [.. _context.ImportDocuments];
        }
    }
}
