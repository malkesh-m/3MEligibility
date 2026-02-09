using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing history PC operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="HistoryPcController"/> class.
    /// </remarks>
    /// <param name="historyPcService">The history PC service.</param>
    [Route("api/historypc")]
    [ApiController]
    public class HistoryPcController(IHistoryPcService historyPcService) : ControllerBase
    {
        /// <summary>
        /// The history PC service instance.
        /// </summary>
        private readonly IHistoryPcService _historyPcService = historyPcService;

        /// <summary>
        /// Retrieves all history PC records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="HistoryPcModel"/> objects.</returns>
        [Authorize(Policy = Permissions.HistoryPc.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all history PC records for the current entity
            List<HistoryPcModel> result = _historyPcService.GetAll(User.GetTenantId());
            // Returns success response with the retrieved history PC list
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a history PC record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the history PC record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="HistoryPcModel"/> if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.HistoryPc.View)]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a history PC record by ID for the current entity
            var result = _historyPcService.GetById(User.GetTenantId(), id);
            // Checks if the history PC record was found
            if (result != null)
            {
                // Returns success response with the retrieved history PC data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when history PC record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new history PC record.
        /// </summary>
        /// <param name="historyPc">The <see cref="HistoryPcModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryPc.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(HistoryPcModel historyPc)
        {
            // Sets the entity ID from current user context
            historyPc.TenantId = User.GetTenantId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new history PC record
            await _historyPcService.Add(historyPc);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing history PC record.
        /// </summary>
        /// <param name="historyPc">The <see cref="HistoryPcModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryPc.Edit)]
        [HttpPut]
        public async Task<IActionResult> Put(HistoryPcModel historyPc)
        {
            // Sets the entity ID from current user context
            historyPc.TenantId = User.GetTenantId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing history PC record
            await _historyPcService.Update(historyPc);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a history PC record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the history PC record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryPc.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the history PC record by ID for current entity
            await _historyPcService.Delete(User.GetTenantId(), id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple history PC records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the history PC records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.HistoryPc.Delete)]
        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple history PC records for current entity
            await _historyPcService.MultipleDelete(User.GetTenantId(), ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
