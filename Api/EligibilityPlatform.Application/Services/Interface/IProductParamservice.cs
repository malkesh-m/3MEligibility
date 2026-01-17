using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for product parameter management operations.
    /// Provides methods for performing CRUD operations, import/export, and bulk actions on product parameter records.
    /// </summary>
    public interface IProductParamservice
    {
        /// <summary>
        /// Retrieves all product parameter records for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="ProductParamListModel"/> objects.</returns>
        Task<List<ProductParamListModel>> GetAll(int tenantId);

        /// <summary>
        /// Adds a new product parameter record.
        /// </summary>
        /// <param name="model">The <see cref="ProductParamAddUpdateModel"/> containing the product parameter details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ProductParamAddUpdateModel model);

        /// <summary>
        /// Retrieves a specific product parameter record by its composite key identifiers.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="parameterId">The unique identifier of the parameter.</param>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>The <see cref="ProductParamListModel"/> with the specified composite key identifiers.</returns>
        ProductParamListModel GetById(int productId, int parameterId, int tenantId);

        /// <summary>
        /// Retrieves product parameter records associated with a specific product identifier within an entity.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>The <see cref="ProductParamListModel"/> associated with the specified product and entity.</returns>
        ProductParamListModel GetByProductId(int productId, int tenantId);

        /// <summary>
        /// Updates an existing product parameter record.
        /// </summary>
        /// <param name="model">The <see cref="ProductParamAddUpdateModel"/> containing the updated product parameter details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ProductParamAddUpdateModel model);

        /// <summary>
        /// Deletes a product parameter record by its composite key identifiers.
        /// </summary>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <param name="parameterId">The unique identifier of the parameter.</param>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int productId, int parameterId, int tenantId);

        /// <summary>
        /// Deletes multiple product parameter records in a single operation.
        /// </summary>
        /// <param name="productids">A list of product identifiers of the records to delete.</param>
        /// <param name="parameterids">A list of parameter identifiers of the records to delete.</param>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> productids, List<int> parameterids, int tenantId);

        /// <summary>
        /// Downloads a template file for product parameter operations for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation, containing the template file as a byte array.</returns>
        Task<byte[]> DownloadTemplate(int tenantId);

        /// <summary>
        /// Exports product parameter details to a stream for the specified entity and selected products.
        /// </summary>
        /// <param name="selectedProductIds">A list of product identifiers to include in the export.</param>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation, containing the export stream.</returns>
        Task<Stream> ExportDetails(List<int> selectedProductIds, int tenantId);

        /// <summary>
        /// Imports product parameter details from a stream for the specified entity.
        /// </summary>
        /// <param name="fileStream">The stream containing the import data.</param>
        /// <param name="createdBy">The user who created the import.</param>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation, containing the import result message.</returns>
        Task<string> ImportDetails(Stream fileStream, string createdBy, int tenantId);
    }
}
