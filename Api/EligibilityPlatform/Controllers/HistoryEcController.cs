using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing history EC operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HistoryEcController"/> class.
    /// </remarks>
    /// <param name="historyEcService">The history EC service.</param>
    [Route("api/historyec")]
    [ApiController]
    public class HistoryEcController(IHistoryEcService historyEcService) : ControllerBase
    {
        /// <summary>
        /// The history EC service instance.
        /// </summary>
        private readonly IHistoryEcService _historyEcService = historyEcService;

        /// <summary>
        /// Retrieves all history EC records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="HistoryEcModel"/> objects.</returns>
        [Authorize(Policy = Permissions.HistoryEc.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all history EC records
            List<HistoryEcModel> result = _historyEcService.GetAll();
            // Returns success response with the retrieved history EC list
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a history EC record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history EC record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="HistoryEcModel"/> if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.HistoryEc.View)]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a history EC record by ID
            var result = _historyEcService.GetById(id);
            // Checks if the history EC record was found
            if (result != null)
            {
                // Returns success response with the retrieved history EC data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when history EC record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new history EC record.
        /// </summary>
        /// <param name="history">The <see cref="HistoryEcModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryEc.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(HistoryEcModel history)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new history EC record
            await _historyEcService.Add(history);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing history EC record.
        /// </summary>
        /// <param name="history">The <see cref="HistoryEcModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryEc.Edit)]
        [HttpPut]
        public async Task<IActionResult> Put(HistoryEcModel history)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing history EC record
            await _historyEcService.Update(history);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a history EC record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the history EC record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryEc.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the history EC record by ID
            await _historyEcService.Delete(id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple history EC records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the history EC records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryEc.Delete)]
        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple history EC records
            await _historyEcService.MultipleDelete(ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
