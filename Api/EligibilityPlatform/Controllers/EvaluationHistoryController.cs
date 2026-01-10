using EligibilityPlatform.Application.Services;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing evaluation history operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EvaluationHistoryController"/> class.
    /// </remarks>
    /// <param name="evaluationHistoryService">The evaluation history service.</param>
    [Route("api/evalutionhistory")]
    [ApiController]
    public class EvaluationHistoryController(IEvaluationHistoryService evaluationHistoryService) : ControllerBase
    {
        private readonly IEvaluationHistoryService _evaluationHistoryService = evaluationHistoryService;

        /// <summary>
        /// Retrieves all evaluation history records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of evaluation history records.</returns>
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            // Retrieves all evaluation history records for the current entity
            var result = await _evaluationHistoryService.GetAll(User.GetEntityId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result });
        }

        /// <summary>
        /// Retrieves an evaluation history record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the evaluation history record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the record if found; otherwise, not found.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Retrieves specific evaluation history record by ID for the current entity
            var result = await _evaluationHistoryService.GetById(id, User.GetEntityId());
            if (result != null)
            {
                // Returns success response with the found record data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when record does not exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new evaluation history record.
        /// </summary>
        /// <param name="model">The evaluation history model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Add(EvaluationHistoryModel model)
        {
            // Sets the current user ID in the model
            model.NationalId = User.GetUserId().ToString();
            // Adds the new evaluation history record
            await _evaluationHistoryService.Add(model, User.GetEntityId());
            // Returns success response indicating record was created
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing evaluation history record.
        /// </summary>
        /// <param name="model">The evaluation history model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPut]
        public async Task<IActionResult> Update(EvaluationHistoryModel model)
        {
            // Updates the existing evaluation history record
            await _evaluationHistoryService.Update(model);
            // Returns success response indicating record was updated
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes an evaluation history record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the evaluation history record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the evaluation history record by ID for the current entity
            await _evaluationHistoryService.Delete(id, User.GetEntityId());
            // Returns success response indicating record was deleted
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
