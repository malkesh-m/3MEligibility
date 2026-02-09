using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for mapping function management operations.
    /// Provides methods for performing CRUD operations and bulk actions on mapping function records.
    /// </summary>
    public interface IMappingfunctionService
    {
        /// <summary>
        /// Retrieves all mapping function records.
        /// </summary>
        /// <returns>A list of <see cref="MappingFunctionModel"/> objects containing all mapping function records.</returns>
        List<MappingFunctionModel> GetAll();

        /// <summary>
        /// Retrieves a specific mapping function record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the mapping function record to retrieve.</param>
        /// <returns>The <see cref="MappingFunctionModel"/> with the specified ID.</returns>
        MappingFunctionModel GetById(int id);

        /// <summary>
        /// Adds a new mapping function record.
        /// </summary>
        /// <param name="model">The <see cref="MappingFunctionModel"/> containing the mapping function details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(MappingFunctionModel model);

        /// <summary>
        /// Updates an existing mapping function record.
        /// </summary>
        /// <param name="model">The <see cref="MappingFunctionModel"/> containing the updated mapping function details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(MappingFunctionModel model);

        /// <summary>
        /// Deletes a mapping function record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the mapping function record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple mapping function records in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the mapping function records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
