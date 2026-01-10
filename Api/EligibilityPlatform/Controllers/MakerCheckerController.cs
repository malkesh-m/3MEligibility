using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Enums;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing maker checker operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MakerCheckerController"/> class.
    /// </remarks>
    /// <param name="makerChecker">The maker checker service.</param>
    [Route("api/makerchecker")]
    [ApiController]
    [Authorize]
    public class MakerCheckerController(IMakerCheckerService makerChecker) : ControllerBase
    {
        private readonly IMakerCheckerService _makerChecker = makerChecker;

        /// <summary>
        /// Retrieves all maker checker records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing all maker checker records.</returns>
        /// 
        [RequireRole("View Checker Screen")]

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            // Retrieves all maker checker records
            var data = _makerChecker.GetAll();
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = data, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a maker checker record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the maker checker record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the record if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Checker Screen")]

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            // Retrieves a maker checker record by ID
            var result = _makerChecker.GetById(id);
            // Checks if the record was found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new maker checker record.
        /// </summary>
        /// <param name="model">The maker checker model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Post(MakerCheckerAddUpdateModel model)
        {

            var userName = User.Identity!.Name;
            model.MakerName = userName;
            var userId = User.GetUserId();
            model.MakerId = userId;
            ;            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }

            // Adds the new maker checker record
            await _makerChecker.Add(model);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing maker checker record.
        /// </summary>
        /// <param name="model">The maker checker model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Maker-Checker")]

        [HttpPut]
        public async Task<IActionResult> Put(MakerCheckerModel model)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }

            // Updates the existing maker checker record
            await _makerChecker.Update(User.GetEntityId(), model);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a maker checker record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the maker checker record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the maker checker record by ID
            await _makerChecker.Remove(id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Updates the status of a maker checker record.
        /// </summary>
        /// <param name="id">The unique identifier of the maker checker record.</param>
        /// <param name="statusName">The status name to update to.</param>
        /// <param name="Comment">An optional comment for the status update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Maker-Checker")]

        [HttpPut("statusupdate")]
        public async Task<IActionResult> StatusUpdate(int id, string statusName, string? Comment)
        {
            // Validates if the status name is valid
            if (!Enum.TryParse(typeof(MakerCheckerStatusEnum), statusName, out var status))
            {
                // Returns bad request for invalid status name
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "Invalid status name" });
            }

            // Retrieves the maker checker record by ID
            var result = _makerChecker.GetById(id);
            // Updates the status of the record
            result!.Status = (MakerCheckerStatusEnum)status;
            // Sets the checker ID from user claims
            result.CheckerId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            // Sets the current date as checker date
            result.CheckerDate = DateTime.Now;
            // Sets the optional comment
            result.Comment = Comment;
            // Updates the maker checker record
            await _makerChecker.Update(User.GetEntityId(), result);

            // Returns success response for status update
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves all maker checker statuses.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing all maker checker statuses.</returns>
        [HttpGet("getmakercheckerstatuses")]
        public IActionResult GetMakerCheckerStatuses()
        {
            // Retrieves all enum values for maker checker statuses
            var statuses = Enum.GetValues<MakerCheckerStatusEnum>()
                .Cast<MakerCheckerStatusEnum>()
                // Projects each enum value to an object with ID and Name
                .Select(e => new { Id = (int)e, Name = e.ToString() })
                .ToList();

            // Returns the list of statuses
            return Ok(statuses);
        }
    }
}
