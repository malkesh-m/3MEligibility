using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for factor management operations.
    /// Provides methods for performing CRUD operations, import/export, and specialized factor retrieval operations.
    /// </summary>
    public interface IFactorService
    {
        /// <summary>
        /// Retrieves all factors for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to retrieve factors.</param>
        /// <returns>A list of <see cref="FactorListModel"/> objects containing all factors for the specified entity.</returns>
        List<FactorListModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves a specific factor by its identifier and entity identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the factor.</param>
        /// <param name="id">The unique identifier of the factor to retrieve.</param>
        /// <returns>The <see cref="FactorListModel"/> with the specified ID and entity ID.</returns>
        FactorListModel GetById(int tenantId, int id);

        /// <summary>
        /// Adds a new factor.
        /// </summary>
        /// <param name="model">The <see cref="FactorAddUpdateModel"/> containing the factor details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(FactorAddUpdateModel model);

        /// <summary>
        /// Updates an existing factor.
        /// </summary>
        /// <param name="model">The <see cref="FactorAddUpdateModel"/> containing the updated factor details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(FactorAddUpdateModel model);

        /// <summary>
        /// Deletes a factor by its identifier and entity identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the factor.</param>
        /// <param name="id">The unique identifier of the factor to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int tenantId, int id);

        /// <summary>
        /// Deletes multiple factors in a single operation for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the factors.</param>
        /// <param name="ids">A list of unique identifiers of the factors to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveMultiple(int tenantId, List<int> ids);

        /// <summary>
        /// Imports factors from a file stream for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to import factors.</param>
        /// <param name="fileStream">The stream containing the factor data to import.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ImportEntities(int tenantId, Stream fileStream);

        /// <summary>
        /// Exports factors to a stream for the selected factor IDs and specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to export factors.</param>
        /// <param name="selectedFactorIds">A list of factor IDs to export.</param>
        /// <returns>A task that represents the asynchronous operation, containing a stream with the exported factor data.</returns>
        Task<Stream> ExportFactors(int tenantId, List<int> selectedFactorIds);

        /// <summary>
        /// Retrieves factor values based on entity and parameter identifiers.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="parameterId">The unique identifier of the parameter.</param>
        /// <returns>A list of strings containing the factor values for the specified entity and parameter.</returns>
        List<string> GetValueByParams(int tenantId, int parameterId);

        /// <summary>
        /// Retrieves factors associated with a specific condition for an entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="conditionId">The unique identifier of the condition.</param>
        /// <returns>A list of <see cref="FactorModel"/> objects containing factors for the specified condition and entity.</returns>
        List<FactorModel> GetFactorByCondition(int tenantId, int conditionId);

        /// <summary>
        /// Retrieves factors associated with a specific parameter for an entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="parameterId">The unique identifier of the parameter.</param>
        /// <returns>A list of <see cref="FactorModel"/> objects containing factors for the specified parameter and entity.</returns>
        List<FactorModel> GetFactorByparameter(int tenantId, int parameterId);

        /// <summary>
        /// Downloads a template file for factor import.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to download the template.</param>
        /// <returns>A task that represents the asynchronous operation, containing a byte array with the template file data.</returns>
        Task<byte[]> DownloadTemplate(int tenantId);

        /// <summary>
        /// Imports factors from a file stream for a specific entity with user tracking.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to import factors.</param>
        /// <param name="fileStream">The stream containing the factor data to import.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, containing a status message string.</returns>
        Task<string> ImportFactor(int tenantId, Stream fileStream, string createdBy);
    }
}
