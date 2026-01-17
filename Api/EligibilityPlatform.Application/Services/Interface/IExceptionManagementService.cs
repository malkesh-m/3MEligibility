using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for exception management operations.
    /// Provides methods for performing CRUD operations on exception management records.
    /// </summary>
    public interface IExceptionManagementService
    {
        /// <summary>
        /// Retrieves all exception management records for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to retrieve exception management records.</param>
        /// <returns>A list of <see cref="ExceptionManagementListModel"/> objects containing all exception management records for the specified entity.</returns>
        List<ExceptionManagementListModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves a specific exception management record by its identifier and entity identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the exception management record.</param>
        /// <param name="id">The unique identifier of the exception management record to retrieve.</param>
        /// <returns>The <see cref="ExceptionManagementGetModel"/> with the specified ID and entity ID.</returns>
        ExceptionManagementGetModel GetById(int tenantId, int id);

        /// <summary>
        /// Adds a new exception management record for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity for which to add the exception management record.</param>
        /// <param name="model">The <see cref="ExceptionManagementCreateOrUpdateModel"/> containing the exception management details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(int tenantId, ExceptionManagementCreateOrUpdateModel model);

        /// <summary>
        /// Updates an existing exception management record for a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the exception management record.</param>
        /// <param name="model">The <see cref="ExceptionManagementCreateOrUpdateModel"/> containing the updated exception management details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(int tenantId, ExceptionManagementCreateOrUpdateModel model);

        /// <summary>
        /// Deletes an exception management record by its identifier and entity identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity associated with the exception management record.</param>
        /// <param name="id">The unique identifier of the exception management record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Delete(int tenantId, int id);
    }
}
