using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for condition management operations.
    /// Provides methods for performing CRUD operations and bulk actions on conditions.
    /// </summary>
    public interface IConditionService
    {
        /// <summary>
        /// Retrieves all conditions.
        /// </summary>
        /// <returns>A list of <see cref="ConditionModel"/> objects containing all conditions.</returns>
        List<ConditionModel> GetAll();

        /// <summary>
        /// Retrieves a specific condition by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the condition to retrieve.</param>
        /// <returns>The <see cref="ConditionModel"/> with the specified ID.</returns>
        ConditionModel GetById(int id);

        /// <summary>
        /// Adds a new condition.
        /// </summary>
        /// <param name="model">The <see cref="ConditionModel"/> containing the condition details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ConditionModel model);

        /// <summary>
        /// Updates an existing condition.
        /// </summary>
        /// <param name="model">The <see cref="ConditionModel"/> containing the updated condition details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ConditionModel model);

        /// <summary>
        /// Deletes a condition by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the condition to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple conditions in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the conditions to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
