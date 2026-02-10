using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing condition operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ConditionController"/> class.
    /// </remarks>
    /// <param name="conditionService">The condition service.</param>
    [Route("api/condition")]
    [ApiController]
    public class ConditionController(IConditionService conditionService) : ControllerBase
    {
        /// <summary>
        /// The condition service instance for condition operations.
        /// </summary>
        private readonly IConditionService _conditionService = conditionService;

        /// <summary>
        /// Retrieves all condition records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ConditionModel"/> objects.</returns>
        [Authorize(Policy = Permissions.Condition.View)]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            /// <summary>
            /// Retrieves all condition records from the service.
            /// </summary>
            List<ConditionModel> result = _conditionService.GetAll();

            /// <summary>
            /// Returns successful response with the retrieved condition records.
            /// </summary>
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a condition record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the condition.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="ConditionModel"/> if found.</returns>
        [Authorize(Policy = Permissions.Condition.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /// <summary>
            /// Retrieves a specific condition record by ID from the service.
            /// </summary>
            var result = _conditionService.GetById(id);

            /// <summary>
            /// Returns successful response with the retrieved condition record.
            /// </summary>
            return Ok(result);
        }

        /// <summary>
        /// Adds a new condition record.
        /// </summary>
        /// <param name="condition">The <see cref="ConditionModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Condition.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(ConditionModel condition)
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
            /// Calls the service to add a new condition record.
            /// </summary>
            await _conditionService.Add(condition);

            /// <summary>
            /// Returns successful response indicating the condition record was created.
            /// </summary>
            return Ok();
        }

        /// <summary>
        /// Updates an existing condition record.
        /// </summary>
        /// <param name="condition">The <see cref="ConditionModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Condition.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(ConditionModel condition)
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
            /// Calls the service to update an existing condition record.
            /// </summary>
            await _conditionService.Update(condition);

            /// <summary>
            /// Returns successful response indicating the condition record was updated.
            /// </summary>
            return Ok();
        }

        /// <summary>
        /// Deletes a condition record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the condition to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Condition.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete a condition record.
            /// </summary>
            await _conditionService.Delete(id);

            /// <summary>
            /// Returns successful response indicating the condition record was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple condition records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the conditions to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Condition.Delete)]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            /// <summary>
            /// Validates that IDs were provided in the request body.
            /// </summary>
            if (ids.Count == 0 || ids == null)
            {
                /// <summary>
                /// Returns bad request response when no IDs are provided.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "no Id's provided" });
            }

            /// <summary>
            /// Calls the service to delete multiple condition records.
            /// </summary>
            await _conditionService.MultipleDelete(ids);

            /// <summary>
            /// Returns successful response indicating the condition records were deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
