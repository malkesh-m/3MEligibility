using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for city management operations.
    /// Provides methods for performing CRUD operations and bulk actions on cities.
    /// </summary>
    public interface ICityService
    {
        /// <summary>
        /// Retrieves all cities.
        /// </summary>
        /// <returns>A list of <see cref="CityModel"/> objects containing all cities.</returns>
        List<CityModel> GetAll();

        /// <summary>
        /// Retrieves a specific city by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the city to retrieve.</param>
        /// <returns>The <see cref="CityModel"/> with the specified ID.</returns>
        CityModel GetById(int id);

        /// <summary>
        /// Adds a new city.
        /// </summary>
        /// <param name="model">The <see cref="CityModel"/> containing the city details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(CityModel model);

        /// <summary>
        /// Updates an existing city.
        /// </summary>
        /// <param name="model">The <see cref="CityModel"/> containing the updated city details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(CityModel model);

        /// <summary>
        /// Deletes a city by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the city to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple cities in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the cities to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteMultiple(List<int> ids);
    }
}
