using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for parameters map management operations.
    /// Provides methods for performing CRUD operations on parameters map records.
    /// </summary>
    public interface IParamtersMapService
    {
        /// <summary>
        /// Retrieves all parameters map records.
        /// </summary>
        /// <returns>A list of <see cref="ParamtersMapModel"/> objects containing all parameters map records.</returns>
        List<ParamtersMapModel> GetAll();

        /// <summary>
        /// Retrieves a specific parameters map record by its composite key identifiers.
        /// </summary>
        /// <param name="apiId">The unique identifier of the API.</param>
        /// <param name="nodeId">The unique identifier of the node.</param>
        /// <param name="parameterId">The unique identifier of the parameter.</param>
        /// <returns>The <see cref="ParamtersMapModel"/> with the specified composite key identifiers.</returns>
        ParamtersMapModel GetById(int apiId, int nodeId, int parameterId);

        /// <summary>
        /// Adds a new parameters map record.
        /// </summary>
        /// <param name="model">The <see cref="ParamtersMapModel"/> containing the parameters map details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ParamtersMapModel model);

        /// <summary>
        /// Updates an existing parameters map record.
        /// </summary>
        /// <param name="model">The <see cref="ParamtersMapModel"/> containing the updated parameters map details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ParamtersMapModel model);

        /// <summary>
        /// Deletes a parameters map record by its composite key identifiers.
        /// </summary>
        /// <param name="apiId">The unique identifier of the API.</param>
        /// <param name="nodeId">The unique identifier of the node.</param>
        /// <param name="parameterId">The unique identifier of the parameter.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int apiId, int nodeId, int parameterId);
    }
}
