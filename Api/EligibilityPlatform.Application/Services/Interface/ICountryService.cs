using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for country management operations.
    /// Provides methods for performing CRUD operations and bulk actions on countries.
    /// </summary>
    public interface ICountryService
    {
        /// <summary>
        /// Retrieves all countries.
        /// </summary>
        /// <returns>A list of <see cref="CountryModel"/> objects containing all countries.</returns>
        List<CountryModel> GetAll();

        /// <summary>
        /// Retrieves a specific country by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the country to retrieve.</param>
        /// <returns>The <see cref="CountryModel"/> with the specified ID.</returns>
        CountryModel GetById(int id);

        /// <summary>
        /// Adds a new country.
        /// </summary>
        /// <param name="model">The <see cref="CountryModel"/> containing the country details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(CountryModel model);

        /// <summary>
        /// Updates an existing country.
        /// </summary>
        /// <param name="model">The <see cref="CountryModel"/> containing the updated country details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(CountryModel model);

        /// <summary>
        /// Deletes a country by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the country to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple countries in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the countries to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
