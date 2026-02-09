using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for data type management operations.
    /// Provides methods for performing CRUD operations and bulk actions on data types.
    /// </summary>
    public interface IDataTypeService
    {
        /// <summary>
        /// Retrieves all data types.
        /// </summary>
        /// <returns>A list of <see cref="DataTypeModel"/> objects containing all data types.</returns>
        List<DataTypeModel> GetAll();

        /// <summary>
        /// Retrieves a specific data type by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the data type to retrieve.</param>
        /// <returns>The <see cref="DataTypeModel"/> with the specified ID.</returns>
        DataTypeModel GetById(int id);

        /// <summary>
        /// Adds a new data type.
        /// </summary>
        /// <param name="model">The <see cref="DataTypeModel"/> containing the data type details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(DataTypeModel model);

        /// <summary>
        /// Updates an existing data type.
        /// </summary>
        /// <param name="model">The <see cref="DataTypeModel"/> containing the updated data type details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(DataTypeModel model);

        /// <summary>
        /// Deletes a data type by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the data type to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int id);

        /// <summary>
        /// Deletes multiple data types in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the data types to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
