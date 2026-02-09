using MEligibilityPlatform.Application.Attributes;
using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing audit operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AuditController"/> class.
    /// </remarks>
    /// <param name="AuditService">The audit service.</param>
    [Route("api/audit")]
    [ApiController]
    public class AuditController(IAuditService AuditService) : ControllerBase
    {
        /// <summary>
        /// The audit service instance for audit operations.
        /// </summary>
        private readonly IAuditService _AuditService = AuditService;

        /// <summary>
        /// Retrieves all audit records with pagination support.
        /// </summary>
        /// <param name="pageIndex">The page index for pagination (default is 0).</param>
        /// <param name="pageSize">The page size for pagination (default is 10).</param>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="AuditModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.Audit.View)]

        [HttpGet("getall")]
        public async Task<IActionResult> Get(int pageIndex = 0, int pageSize = 10)
        {
            var tenantId = User.GetTenantId();
            /// <summary>
            /// Calls the service to retrieve all audit records with pagination.
            /// </summary>
            var result = await _AuditService.GetAll(tenantId,pageIndex, pageSize);

            /// <summary>
            /// Returns successful response with the retrieved audit records.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves an audit record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the audit record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="AuditModel"/> if found; otherwise, a not found result.</returns>
        [Authorize(Policy = Permissions.Audit.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var tenantId = User.GetTenantId();
            /// <summary>
            /// Retrieves a specific audit record by ID from the service.
            /// </summary>
            var result = _AuditService.GetById(id,tenantId);

            /// <summary>
            /// Checks if the audit record was found.
            /// </summary>
            if (result != null)
            {
                /// <summary>
                /// Returns successful response with the retrieved audit record.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when the audit record does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new audit record.
        /// </summary>
        /// <param name="audit">The <see cref="AuditModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Audit.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(AuditCreateUpdateModel audit)
        {
            /// <summary>
            /// Validates the model state before processing.
            /// </summary>
            /// 
            var userName = User.GetUserName();
            audit.UserName = userName;

            if (!ModelState.IsValid)
            {
                /// <summary>
                /// Returns bad request response for invalid model state.
                /// </summary>
                return BadRequest(ModelState);
            }

            /// <summary>
            /// Calls the service to add a new audit record.
            /// </summary>
            await _AuditService.Add(audit);

            /// <summary>
            /// Returns successful response indicating the audit record was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing audit record.
        /// </summary>
        /// <param name="audit">The <see cref="AuditModel"/> to update.</param>
        /// 
        [Authorize(Policy = Permissions.Audit.Edit)]

        /// <returns>An <see cref="I    ActionResult"/> indicating the result of the operation.</returns>
        [HttpPut]
        public async Task<IActionResult> Put(AuditCreateUpdateModel audit)
        {
            /// <summary>
            /// Validates the model state before processing.
            /// </summary>
            /// 
            var userName = User.GetUserName();
            audit.UserName = userName;

            if (!ModelState.IsValid)
            {
                /// <summary>
                /// Returns bad request response for invalid model state.
                /// </summary>
                return BadRequest(ModelState);
            }

            /// <summary>
            /// Calls the service to update an existing audit record.
            /// </summary>
            await _AuditService.Update(audit);

            /// <summary>
            /// Returns successful response indicating the audit record was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes an audit record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the audit record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Audit.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete an audit record.
            /// </summary>
            await _AuditService.Delete(id);

            /// <summary>
            /// Returns successful response indicating the audit record was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple audit records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the audit records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Audit.Delete)]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            /// <summary>
            /// Validates that IDs were provided in the request body.
            /// </summary>
            if (ids == null || ids.Count == 0)
            {
                /// <summary>
                /// Returns bad request response when no IDs are provided.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
            }

            /// <summary>
            /// Calls the service to delete multiple audit records.
            /// </summary>
            await _AuditService.MultiPleDelete(ids);

            /// <summary>
            /// Returns successful response indicating the audit records were deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
