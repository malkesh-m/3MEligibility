using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing application settings operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AppSettingController"/> class.
    /// </remarks>
    /// <param name="appSettingService">The app setting service.</param>
    [Route("api/appsetting")]
    [ApiController]
    public class AppSettingController(IAppSettingService appSettingService) : ControllerBase
    {
        /// <summary>
        /// The application setting service instance for setting operations.
        /// </summary>
        private readonly IAppSettingService _appSettingService = appSettingService;

        /// <summary>
        /// Updates the application settings.
        /// </summary>
        /// <param name="AppSetting">The <see cref="AppSettingModel"/> containing updated settings. Always pass 1 for AppSettingId.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
        [Authorize(Policy = Permissions.AppSetting.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(AppSettingModel AppSetting)
        {
            /// <summary>
            /// Validates the model state before processing.
            /// </summary>
            if (!ModelState.IsValid)
            {
                /// <summary>
                /// Returns bad request response for invalid model state.
                /// </summary>
                return BadRequest(ModelState);
            }

            /// <summary>
            /// Calls the service to update the application settings.
            /// </summary>
            await _appSettingService.Update(AppSetting);

            /// <summary>
            /// Returns successful response indicating the application settings were updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }
    }
}
