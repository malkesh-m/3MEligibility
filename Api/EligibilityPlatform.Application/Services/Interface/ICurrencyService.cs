using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for currency management operations.
    /// Provides methods for performing CRUD operations and bulk actions on currencies.
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Retrieves all currencies.
        /// </summary>
        /// <returns>A list of <see cref="CurrencyModel"/> objects containing all currencies.</returns>
        List<CurrencyModel> GetAll();

        /// <summary>
        /// Retrieves a specific currency by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the currency to retrieve.</param>
        /// <returns>The <see cref="CurrencyModel"/> with the specified ID.</returns>
        CurrencyModel GetById(int id);

        /// <summary>
        /// Adds a new currency.
        /// </summary>
        /// <param name="model">The <see cref="CurrencyModel"/> containing the currency details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(CurrencyModel model);

        /// <summary>
        /// Updates an existing currency.
        /// </summary>
        /// <param name="model">The <see cref="CurrencyModel"/> containing the updated currency details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(CurrencyModel model);

        /// <summary>
        /// Deletes a currency by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the currency to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple currencies in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the currencies to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}

