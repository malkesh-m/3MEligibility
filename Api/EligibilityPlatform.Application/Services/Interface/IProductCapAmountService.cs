using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for product cap amount management operations.
    /// Provides methods for performing CRUD operations on product cap amount records.
    /// </summary>
    public interface IProductCapAmountService
    {
        /// <summary>
        /// Adds a new product cap amount record.
        /// </summary>
        /// <param name="model">The <see cref="ProductCapAmountAddModel"/> containing the product cap amount details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ProductCapAmountAddModel model);

        /// <summary>
        /// Retrieves all product cap amount records.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="ProductCapAmountModel"/> objects.</returns>
        List<ProductCapAmountModel> GetAll();

        /// <summary>
        /// Updates an existing product cap amount record.
        /// </summary>
        /// <param name="model">The <see cref="ProductCapAmountUpdateModel"/> containing the updated product cap amount details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ProductCapAmountUpdateModel model);

        /// <summary>
        /// Deletes a product cap amount record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product cap amount record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Retrieves product cap amount records associated with a specific product identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="ProductCapAmountModel"/> objects associated with the specified product.</returns>
        Task<List<ProductCapAmountModel>> GetByProductId(int id);
    }
}
