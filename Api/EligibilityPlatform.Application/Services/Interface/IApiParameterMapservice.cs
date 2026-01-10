using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for API parameter mapping operations.
    /// Provides methods for managing API parameter mappings including CRUD operations.
    /// </summary>
    public interface IApiParameterMapservice
    {
        /// <summary>
        /// Retrieves all API parameter mappings.
        /// </summary>
        /// <returns>A list of <see cref="ApiParameterListMapModel"/> objects containing all parameter mappings.</returns>
        List<ApiParameterListMapModel> GetAll();

        /// <summary>
        /// Retrieves a specific API parameter mapping by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the parameter mapping to retrieve.</param>
        /// <returns>The <see cref="ApiParameterListMapModel"/> with the specified ID.</returns>
        ApiParameterListMapModel GetById(int id);

        /// <summary>
        /// Adds a new API parameter mapping.
        /// </summary>
        /// <param name="model">The <see cref="ApiParameterCreateUpdateMapModel"/> containing the mapping details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ApiParameterCreateUpdateMapModel model);

        /// <summary>
        /// Updates an existing API parameter mapping.
        /// </summary>
        /// <param name="model">The <see cref="ApiParameterCreateUpdateMapModel"/> containing the updated mapping details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ApiParameterCreateUpdateMapModel model);

        /// <summary>
        /// Removes an API parameter mapping by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the parameter mapping to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);
        public List<ApiParameterMapName> GetMappingsByApiId(int apiId);

    }
}
