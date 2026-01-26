using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing history ER operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HistoryErController"/> class.
    /// </remarks>
    /// <param name="historyErService">The history ER service.</param>
    [Route("api/historyer")]
    [ApiController]
    public class HistoryErController(IHistoryErService historyErService) : ControllerBase
    {
        /// <summary>
        /// The history ER service instance.
        /// </summary>
        private readonly IHistoryErService _historyErService = historyErService;

        /// <summary>
        /// Retrieves all history ER records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="HistoryErModel"/> objects.</returns>
        [Authorize(Policy = Permissions.HistoryEr.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all history ER records
            List<HistoryErModel> result = _historyErService.GetAll();
            // Returns success response with the retrieved history ER list
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a history ER record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history ER record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="HistoryErModel"/> if found; otherwise, a not found result.</returns>
        [Authorize(Policy = Permissions.HistoryEr.View)]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a history ER record by ID
            var result = _historyErService.GetById(id);
            // Checks if the history ER record was found
            if (result != null)
            {
                // Returns success response with the retrieved history ER data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when history ER record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new history ER record.
        /// </summary>
        /// <param name="history">The <see cref="HistoryErModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryEr.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(HistoryErModel history)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new history ER record
            await _historyErService.Add(history);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing history ER record.
        /// </summary>
        /// <param name="history">The <see cref="HistoryErModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryEr.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(HistoryErModel history)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing history ER record
            await _historyErService.Update(history);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a history ER record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history ER record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryEr.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the history ER record by ID
            await _historyErService.Delete(id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple history ER records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the history ER records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryEr.Delete)]
        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple history ER records
            await _historyErService.MultipleDelete(ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
