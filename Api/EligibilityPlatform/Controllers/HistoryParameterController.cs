using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing history parameter operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HistoryParameterController"/> class.
    /// </remarks>
    /// <param name="historyParameterService">The history parameter service.</param>
    [Route("api/historyparameter")]
    [ApiController]
    public class HistoryParameterController(IHistoryParameterService historyParameterService) : ControllerBase
    {
        /// <summary>
        /// The history parameter service instance.
        /// </summary>
        private readonly IHistoryParameterService _historyParameterService = historyParameterService;

        /// <summary>
        /// Retrieves all history parameter records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="HistoryParameterModel"/> objects.</returns>
        [Authorize(Policy = Permissions.HistoryParameter.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all history parameter records
            List<HistoryParameterModel> result = _historyParameterService.GetAll();
            // Returns success response with the retrieved history parameter list
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a history parameter record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history parameter record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="HistoryParameterModel"/> if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.HistoryParameter.View)]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a history parameter record by ID
            var result = _historyParameterService.GetById(id);
            // Checks if the history parameter record was found
            if (result != null)
            {
                // Returns success response with the retrieved history parameter data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when history parameter record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new history parameter record.
        /// </summary>
        /// <param name="historyParameterModel">The <see cref="HistoryParameterModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryParameter.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(HistoryParameterModel historyParameterModel)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new history parameter record
            await _historyParameterService.Add(historyParameterModel);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing history parameter record.
        /// </summary>
        /// <param name="historyParameterModel">The <see cref="HistoryParameterModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryParameter.Edit)]
        [HttpPut]
        public async Task<IActionResult> Put(HistoryParameterModel historyParameterModel)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing history parameter record
            await _historyParameterService.Update(historyParameterModel);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a history parameter record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history parameter record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryParameter.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the history parameter record by ID
            await _historyParameterService.Delete(id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple history parameter records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the history parameter records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryParameter.Delete)]
        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple history parameter records
            await _historyParameterService.MultipleDelete(ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
