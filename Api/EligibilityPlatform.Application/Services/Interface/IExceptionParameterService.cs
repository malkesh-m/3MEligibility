using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for exception parameter management operations.
    /// Provides methods for performing CRUD operations on exception parameter records.
    /// </summary>
    public interface IExceptionParameterService
    {
        /// <summary>
        /// Retrieves all exception parameter records.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="ExceptionParameterModel"/> objects.</returns>
        Task<List<ExceptionParameterModel>> GetAll();

        /// <summary>
        /// Retrieves a specific exception parameter record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the exception parameter record to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="ExceptionParameterModel"/> with the specified ID.</returns>
        Task<ExceptionParameterModel> GetById(int id);

        /// <summary>
        /// Adds a new exception parameter record.
        /// </summary>
        /// <param name="model">The <see cref="ExceptionParameterModel"/> containing the exception parameter details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ExceptionParameterModel model);

        /// <summary>
        /// Updates an existing exception parameter record.
        /// </summary>
        /// <param name="model">The <see cref="ExceptionParameterModel"/> containing the updated exception parameter details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ExceptionParameterModel model);

        /// <summary>
        /// Deletes an exception parameter record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the exception parameter record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);
    }
}
