using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for category management operations.
    /// Provides methods for performing CRUD operations, import/export, and bulk actions on categories.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves all categories for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity to retrieve categories for.</param>
        /// <returns>A list of <see cref="CategoryListModel"/> objects containing the categories.</returns>
        List<CategoryListModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves a specific category by its identifier within a given entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the category to retrieve.</param>
        /// <returns>The <see cref="CategoryListModel"/> with the specified ID.</returns>
        CategoryListModel GetById(int tenantId, int id);

        /// <summary>
        /// Adds a new category.
        /// </summary>
        /// <param name="category">The <see cref="CategoryCreateUpdateModel"/> containing the category details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(CategoryCreateUpdateModel category);

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="category">The <see cref="CategoryUpdateModel"/> containing the updated category details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(CategoryUpdateModel category);

        /// <summary>
        /// Removes a category by its identifier within a given entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the category to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<string> Remove(int tenantId, int id);

        /// <summary>
        /// Removes multiple categories within a given entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="ids">A list of unique identifiers of the categories to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<string> RemoveMultiple(int tenantId, List<int> ids);

        /// <summary>
        /// Exports categories to a stream for the specified entity and selected category IDs.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="selectedCategoryIds">A list of category IDs to export.</param>
        /// <returns>A stream containing the exported category data.</returns>
        Task<Stream> ExportCategory(int tenantId, List<int> selectedCategoryIds);

        /// <summary>
        /// Imports categories from a file stream for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="fileStream">The stream containing the category data to import.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A string containing the result or status of the import operation.</returns>
        Task<string> ImportCategory(int tenantId, Stream fileStream, string createdBy);

        /// <summary>
        /// Downloads a template file for category import.
        /// </summary>
        /// <returns>A byte array containing the template file data.</returns>
        Task<byte[]> DownloadTemplate();
    }
}
