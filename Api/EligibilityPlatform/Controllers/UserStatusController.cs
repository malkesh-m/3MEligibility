using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing user status operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserStatusController"/> class.
    /// </remarks>
    /// <param name="userStatusService">The user status service.</param>
    [Route("api/userstatus")]
    [ApiController]
    public class UserStatusController(IUserStatusService userStatusService) : ControllerBase
    {
        private readonly IUserStatusService _userStatusService = userStatusService;

        /// <summary>
        /// Retrieves all user statuses.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of user statuses.</returns>
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all user statuses from the service
            List<UserStatusModel> result = _userStatusService.GetAll();
            // Returns success response with the list of user statuses
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a user status by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user status.</param>
        /// <returns>An <see cref="IActionResult"/> containing the user status if found; otherwise, not found.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a specific user status by its ID
            var result = _userStatusService.GetById(id);
            // Checks if the user status was found
            if (result != null)
            {
                // Returns success response with the user status data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when user status doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new user status.
        /// </summary>
        /// <param name="userStatusModel">The user status model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Post(UserStatusAddModel userStatusModel)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new user status
            await _userStatusService.Add(userStatusModel);
            // Returns success response after creation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing user status.
        /// </summary>
        /// <param name="userStatusModel">The user status model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPut]
        public async Task<IActionResult> Put(UserStatusAddModel userStatusModel)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the user status
            await _userStatusService.Update(userStatusModel);
            // Returns success response after update
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a user status by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user status to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the user status by ID
            await _userStatusService.Remove(id);
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple user statuses by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the user statuses to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple user statuses
            await _userStatusService.MultipleDelete(ids);
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
