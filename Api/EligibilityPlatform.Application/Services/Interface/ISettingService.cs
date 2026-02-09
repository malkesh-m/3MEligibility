using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for setting management operations.
    /// Provides methods for performing CRUD operations on setting records.
    /// </summary>
    public interface ISettingService
    {
        /// <summary>
        /// Updates an existing setting record.
        /// </summary>
        /// <param name="model">The <see cref="SettingModel"/> containing the updated setting details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(SettingModel model);

        /// <summary>
        /// Adds a new setting record.
        /// </summary>
        /// <param name="model">The <see cref="SettingModel"/> containing the setting details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(SettingModel model);

        /// <summary>
        /// Retrieves a specific setting record by its identifier within a specific entity.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <param name="id">The unique identifier of the setting record to retrieve.</param>
        /// <returns>The <see cref="SettingModel"/> with the specified ID within the given entity.</returns>
        SettingModel GetById(int tenantId, int id);

        /// <summary>
        /// Retrieves setting records associated with a specific entity identifier.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="SettingModel"/> associated with the specified entity.</returns>
        Task<SettingModel> GetbyEntityId(int tenantId);
    }
}
