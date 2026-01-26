using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for product management operations.
    /// Provides methods for performing CRUD operations, import/export, and bulk actions on product records.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Retrieves all product records for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="ProductListModel"/> objects containing all product records for the specified entity.</returns>
        List<ProductListModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves product records by category within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <returns>A list of <see cref="ProductModel"/> objects associated with the specified category.</returns>
        List<ProductModel> GetProductsByCategory(int tenantId, int categoryId);

        /// <summary>
        /// Retrieves a specific product record by its identifier within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the product record to retrieve.</param>
        /// <returns>The <see cref="ProductListModel"/> with the specified ID within the given entity.</returns>
        ProductListModel GetById(int tenantId, int id);

        /// <summary>
        /// Adds a new product record.
        /// </summary>
        /// <param name="model">The <see cref="ProductAddUpdateModel"/> containing the product details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ProductAddUpdateModel model);

        /// <summary>
        /// Updates an existing product record.
        /// </summary>
        /// <param name="model">The <see cref="ProductAddUpdateModel"/> containing the updated product details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ProductAddUpdateModel model);

        /// <summary>
        /// Deletes a product record by its identifier within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the product record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int tenantId, int id);

        /// <summary>
        /// Removes multiple product records within a specific entity in a single operation.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="ids">A list of unique identifiers of the product records to remove.</param>
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
        /// Imports product information from a stream for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="fileStream">The stream containing the import data.</param>
        /// <param name="createdBy">The user who created the import.</param>
        /// <returns>A task that represents the asynchronous operation, containing the import result message.</returns>
        Task<string> ImportInfo(int tenantId, Stream fileStream, string createdBy);

        /// <summary>
        /// Exports product information to a stream for the specified entity and selected products.
        /// </summary>
        /// <param name="enttiyId">The unique identifier of the entity.</param>
        /// <param name="selectedProductIds">A list of product identifiers to include in the export.</param>
        /// <returns>A task that represents the asynchronous operation, containing the export stream.</returns>
        Task<Stream> ExportInfo(int enttiyId, List<int> selectedProductIds);

        /// <summary>
        /// Downloads a template file for product operations for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation, containing the template file as a byte array.</returns>
        Task<byte[]> DownloadTemplate(int tenantId);

        /// <summary>
        /// Retrieves product ID and name pairs for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="ProductIdAndNameModel"/> objects containing product identifiers and names.</returns>
        List<ProductIdAndNameModel> GetProductIAndName(int tenantId);

        /// <summary>
        /// Retrieves eligible product names for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="ProductEligibleModel"/> objects containing eligible product names.</returns>
        List<ProductEligibleModel> GetProductName(int tenantId);
    }
}
