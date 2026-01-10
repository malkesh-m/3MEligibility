using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for API parameters management operations.
    /// Provides methods for performing CRUD operations on API parameters.
    /// </summary>
    public interface IApiParametersService
    {
        /// <summary>
        /// Retrieves all API parameters.
        /// </summary>
        /// <returns>A list of <see cref="ApiParametersListModel"/> objects containing all API parameters.</returns>
        List<ApiParametersListModel> GetAll();

        /// <summary>
        /// Retrieves a specific API parameter by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter to retrieve.</param>
        /// <returns>The <see cref="ApiParametersListModel"/> with the specified ID.</returns>
        ApiParametersListModel GetById(int id);
        Task<List<ApiParametersListModel>> GetByApiId(int id);

        /// <summary>
        /// Adds a new API parameter.
        /// </summary>
        /// <param name="model">The <see cref="ApiParametersCreateUpdateModel"/> containing the parameter details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<ApiParameter> Add(ApiParametersCreateUpdateModel model);

        /// <summary>
        /// Updates an existing API parameter.
        /// </summary>
        /// <param name="model">The <see cref="ApiParametersCreateUpdateModel"/> containing the updated parameter details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ApiParametersCreateUpdateModel model);

        /// <summary>
        /// Removes an API parameter by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API parameter to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);

        /// <summary>
        /// Adds multiple API parameters in a single operation.
        /// </summary>
        /// <param name="models">A list of <see cref="ApiParametersCreateUpdateModel"/> objects containing the parameters to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddRange(List<ApiParametersCreateUpdateModel> models);

        /// <summary>
        /// Deletes all API parameters associated with a specific API identifier.
        /// </summary>
        /// <param name="apiId">The unique identifier of the API whose parameters should be deleted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteByApiIdAsync(int apiId);
        Task RemoveMultiple(List<int> ids);

    }
}
