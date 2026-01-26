using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for API responses management operations.
    /// Provides methods for performing CRUD operations on API responses.
    /// </summary>
    public interface IApiResponsesService
    {
        /// <summary>
        /// Retrieves all API responses.
        /// </summary>
        /// <returns>A list of <see cref="ApiResponsesListModel"/> objects containing all API responses.</returns>
        List<ApiResponsesListModel> GetAll();

        /// <summary>
        /// Retrieves a specific API response by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API response to retrieve.</param>
        /// <returns>The <see cref="ApiResponsesListModel"/> with the specified ID.</returns>
        ApiResponsesListModel GetById(int id);

        /// <summary>
        /// Adds a new API response.
        /// </summary>
        /// <param name="model">The <see cref="ApiResponsesCreateUpdateModel"/> containing the response details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ApiResponsesCreateUpdateModel model);

        /// <summary>
        /// Updates an existing API response.
        /// </summary>
        /// <param name="model">The <see cref="ApiResponsesCreateUpdateModel"/> containing the updated response details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ApiResponsesCreateUpdateModel model);

        /// <summary>
        /// Removes an API response by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the API response to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);

        /// <summary>
        /// Adds multiple API responses in a single operation.
        /// </summary>
        /// <param name="models">A list of <see cref="ApiResponsesCreateUpdateModel"/> objects containing the responses to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddRange(List<ApiResponsesCreateUpdateModel> models);
    }
}
