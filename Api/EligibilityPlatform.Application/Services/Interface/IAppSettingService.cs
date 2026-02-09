using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for application settings operations.
    /// Provides methods for managing application configuration settings.
    /// </summary>
    public interface IAppSettingService
    {
        /// <summary>
        /// Retrieves a specific application setting by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the application setting to retrieve.</param>
        /// <returns>The <see cref="AppSettingModel"/> with the specified ID.</returns>
        AppSettingModel GetById(int id);

        /// <summary>
        /// Updates an existing application setting.
        /// </summary>
        /// <param name="appSetting">The <see cref="AppSettingModel"/> containing the updated setting details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(AppSettingModel appSetting);
    }
}
