using EligibilityPlatform.Application.Constants;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing screen operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ScreenController"/> class.
    /// </remarks>
    /// <param name="screenService">The screen service.</param>
    [Route("api/screen")]
    [ApiController]
    public class ScreenController(IScreenService screenService) : ControllerBase
    {
        private readonly IScreenService _screenService = screenService;

        /// <summary>
        /// Retrieves all screens.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of screens.</returns>
        [Authorize(Policy = Permissions.Screen.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all screens from the screen service
            List<ScreenModel> result = _screenService.GetAll();
            // Returns success response with the list of screens
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a screen by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the screen.</param>
        /// <returns>An <see cref="IActionResult"/> containing the screen if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.Screen.View)]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a specific screen by its ID
            var result = _screenService.GetById(id);
            // Checks if the screen was found
            if (result != null)
            {
                // Returns success response with the screen data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when screen doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new screen.
        /// </summary>
        /// <param name="screenModel">The screen model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Screen.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(ScreenModel screenModel)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new screen
            await _screenService.Add(screenModel);
            // Returns success response after creation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing screen.
        /// </summary>
        /// <param name="screenModel">The screen model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Screen.Edit)]
        [HttpPut]
        public async Task<IActionResult> Put(ScreenModel screenModel)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing screen
            await _screenService.Update(screenModel);
            // Returns success response after update
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a screen by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the screen to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Screen.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the screen by its ID
            await _screenService.Remove(id);
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple screens by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the screens to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Screen.Delete)]
        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates that IDs were provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple screens by their IDs
            await _screenService.MultipleDelete(ids);
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
