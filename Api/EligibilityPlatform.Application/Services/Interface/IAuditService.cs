using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for audit operations.
    /// Provides methods for managing audit records including pagination and bulk operations.
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Retrieves audit records with pagination support.
        /// </summary>
        /// <param name="pageIndex">The zero-based index of the page to retrieve (default is 0).</param>
        /// <param name="pageSize">The number of records per page (default is 10).</param>
        /// <returns>A <see cref="PaginationModel{AuditModel}"/> containing the paginated audit records.</returns>
        Task<PaginationModel<AuditModel>> GetAll(int tenantId,int pageIndex = 0, int pageSize = 10);

        /// <summary>
        /// Retrieves a specific audit record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the audit record to retrieve.</param>
        /// <returns>The <see cref="AuditModel"/> with the specified ID.</returns>
        AuditModel GetById(int id,int tenantId);

        /// <summary>
        /// Adds a new audit record.
        /// </summary>
        /// <param name="model">The <see cref="AuditModel"/> containing the audit details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(AuditCreateUpdateModel model);

        /// <summary>
        /// Updates an existing audit record.
        /// </summary>
        /// <param name="model">The <see cref="AuditModel"/> containing the updated audit details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(AuditCreateUpdateModel model);

        /// <summary>
        /// Deletes an audit record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the audit record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple audit records in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the audit records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultiPleDelete(List<int> ids);
    }
}
