using MEligibilityPlatform.Application.Attributes;
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
    /// Initializes a new instance of the <see cref="SettingController"/> class.
    /// </remarks>
    /// <param name="settingService">The setting service for managing application settings.</param>
    [Route("api/setting")]
    [ApiController]
    public class SettingController(ISettingService settingService) : Controller
    {
        private readonly ISettingService _settingService = settingService;

        /// <summary>
        /// Retrieves a specific setting by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the setting to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing the setting data.</returns>
        /// <response code="200">Returns the setting data successfully.</response>
        /// 
        [Authorize(Policy = Permissions.MakerCheckerConfig.View)]

        [HttpGet("getbyid")]
        public IActionResult Get(int id)
        {
            // Retrieves a specific setting by ID for the current entity
            SettingModel result = _settingService.GetById(User.GetTenantId(), id);
            // Returns success response with the setting data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves all settings for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the settings for the entity.</returns>
        /// <response code="200">Returns the entity settings successfully.</response>
        /// 
        [Authorize(Policy = Permissions.MakerCheckerConfig.View)]

        [HttpGet("getbyentityid")]
        public async Task<IActionResult> GetbyEntityId()
        {
            // Retrieves all settings for the current entity
            SettingModel result = await _settingService.GetbyEntityId(User.GetTenantId());
            // Returns success response with the entity settings
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Updates the settings for the current entity.
        /// </summary>
        /// <param name="setting">The setting model containing the updated data.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
        /// <response code="200">Returns when the settings are updated successfully.</response>
        /// <response code="400">Returned when the model state is invalid or validation fails.</response>
        /// 
        [Authorize(Policy = Permissions.MakerCheckerConfig.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(SettingModel setting)
        {
            // Sets the entity ID from the current user context
            setting.EntityId = User.GetTenantId();

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }

            // Updates the entity settings
            await _settingService.Update(setting);
            // Returns success response after update
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }
    }
}
