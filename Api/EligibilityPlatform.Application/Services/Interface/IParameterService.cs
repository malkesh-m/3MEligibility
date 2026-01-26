using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for parameter management operations.
    /// Provides methods for performing CRUD operations, import/export, and validation of parameter records.
    /// </summary>
    public interface IParameterService
    {
        /// <summary>
        /// Retrieves all parameter records for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="ParameterListModel"/> objects containing all parameter records for the specified entity.</returns>
        List<ParameterListModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves parameter records by entity identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="ParameterListModel"/> objects associated with the specified entity.</returns>
        List<ParameterListModel> GetByEntityId(int tenantId);

        /// <summary>
        /// Retrieves a specific parameter record by its identifier within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the parameter record to retrieve.</param>
        /// <returns>The <see cref="ParameterListModel"/> with the specified ID within the given entity.</returns>
        ParameterListModel GetById(int tenantId, int id);

        /// <summary>
        /// Validates and checks the computed value of a parameter.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="parameterId">The unique identifier of the parameter.</param>
        /// <param name="parameterValue">The parameter value to validate.</param>
        /// <returns>A task that represents the asynchronous operation, containing the validation result message or null if valid.</returns>
        Task<string?> CheckParameterComputedValue(int tenantId, int parameterId, string parameterValue);

        /// <summary>
        /// Adds a new parameter record.
        /// </summary>
        /// <param name="model">The <see cref="ParameterAddUpdateModel"/> containing the parameter details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(ParameterAddUpdateModel model);

        /// <summary>
        /// Updates an existing parameter record.
        /// </summary>
        /// <param name="model">The <see cref="ParameterAddUpdateModel"/> containing the updated parameter details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(ParameterAddUpdateModel model);

        /// <summary>
        /// Deletes a parameter record by its identifier within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the parameter record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int tenantId, int id);

        /// <summary>
        /// Removes multiple parameter records within a specific entity in a single operation.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="ids">A list of unique identifiers of the parameter records to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveMultiple(int tenantId, List<int> ids);

        /// <summary>
        /// Exports parameter data to a stream for the specified entity and identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="Identifier">The identifier for the export operation.</param>
        /// <param name="selectedParameterIds">A list of parameter identifiers to include in the export.</param>
        /// <returns>A task that represents the asynchronous operation, containing the export stream.</returns>
        Task<Stream> ExportParameter(int tenantId, int Identifier, List<int> selectedParameterIds);

        /// <summary>
        /// Imports entities from a stream for the specified entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="fileStream">The stream containing the import data.</param>
        /// <param name="Identifier">The identifier for the import operation.</param>
        /// <param name="createdBy">The user who created the import.</param>
        /// <returns>A task that represents the asynchronous operation, containing the import result message.</returns>
        Task<string> ImportEntities(int tenantId, Stream fileStream, int Identifier, string createdBy);

        /// <summary>
        /// Retrieves parameter records associated with specific products within an entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="productId">The unique identifier of the product.</param>
        /// <returns>A list of <see cref="ParameterModel"/> objects associated with the specified product.</returns>
        List<ParameterModel>? GetParameterByProducts(int tenantId, int productId);

        /// <summary>
        /// Downloads a template file for parameter operations.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the template file as a byte array.</returns>
        Task<byte[]> DownloadTemplate();
    }
}
