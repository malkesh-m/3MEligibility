using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for managing API detail operations.
    /// Provides methods for CRUD operations and bulk deletion of API details.
    /// </summary>
    public interface IApiDetailService
    {
        /// <summary>
        /// Retrieves all API detail records.
        /// </summary>
        /// <returns>A list of <see cref="ApiDetailListModel"/> objects.</returns>
        List<ApiDetailListModel> GetAll();

        /// <summary>
        /// Retrieves a specific API detail record by its ID.
        /// </summary>
        /// <param name="id">The ID of the API detail record to retrieve.</param>
        /// <returns>The <see cref="ApiDetailListModel"/> with the specified ID.</returns>
        ApiDetailListModel GetById(int id);
        Task<List<ApiListModel>> GetAllApiDetailsWithNode();

        /// <summary>
        /// Adds a new API detail record.
        /// </summary>
        /// <param name="model">The <see cref="ApiDetailCreateotUpdateModel"/> containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ApiDetailCreateotUpdateModel model);

        /// <summary>
        /// Updates an existing API detail record.
        /// </summary>
        /// <param name="model">The <see cref="ApiDetailCreateotUpdateModel"/> with updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ApiDetailCreateotUpdateModel model);

        /// <summary>
        /// Deletes an API detail record by its ID.
        /// </summary>
        /// <param name="id">The ID of the API detail record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple API detail records by their IDs.
        /// </summary>
        /// <param name="ids">The list of IDs of the API detail records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteMultiple(List<int> ids);
    }
}
