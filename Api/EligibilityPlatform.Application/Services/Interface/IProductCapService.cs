using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{

    /// <summary>
    /// Service interface for product cap management operations.
    /// Provides methods for performing CRUD operations on product cap records.
    /// </summary>
    public interface IProductCapService
    {
        /// <summary>
        /// Deletes a product cap record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product cap record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Updates an existing product cap record.
        /// </summary>
        /// <param name="model">The <see cref="ProductCapModel"/> containing the updated product cap details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ProductCapModel model);

        /// <summary>
        /// Adds a new product cap record.
        /// </summary>
        /// <param name="model">The <see cref="ProductCapModel"/> containing the product cap details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ProductCapModel model);

        /// <summary>
        /// Retrieves a specific product cap record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product cap record to retrieve.</param>
        /// <returns>The <see cref="ProductCapModel"/> with the specified ID.</returns>
        ProductCapModel GetById(int id);

        /// <summary>
        /// Retrieves product cap records associated with a specific product identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="ProductCapModel"/> objects associated with the specified product.</returns>
        Task<List<ProductCapModel>> GetByProductId(int id);

        /// <summary>
        /// Retrieves all product cap records.
        /// </summary>
        /// <returns>A list of <see cref="ProductCapModel"/> objects containing all product cap records.</returns>
        List<ProductCapModel> GetAll();
    }
}
