using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing e-card operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ECardController"/> class.
    /// </remarks>
    /// <param name="ecardService">The e-card service.</param>
    [Route("api/ecard")]
    [ApiController]
    public class
        ECardController(IEcardService ecardService) : ControllerBase
    {
        /// <summary>
        /// The e-card service instance for e-card operations.
        /// </summary>
        private readonly IEcardService _ecardService = ecardService;

        /// <summary>
        /// Retrieves all e-card records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="EcardListModel"/> objects.</returns>
        /// 
        [RequireRole("View Cards Screen")]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            /// <summary>
            /// Retrieves all e-card records for the current user's entity from the service.
            /// </summary>
            List<EcardListModel> result = _ecardService.GetAll(User.GetTenantId());

            /// <summary>
            /// Returns successful response with the retrieved e-card records.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves an e-card record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the e-card.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="EcardListModel"/> if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Cards Screen")]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /// <summary>
            /// Retrieves a specific e-card record by ID for the current user's entity from the service.
            /// </summary>
            var result = _ecardService.GetById(User.GetTenantId(), id);

            /// <summary>
            /// Checks if the e-card record was found.
            /// </summary>
            if (result != null)
            {
                /// <summary>
                /// Returns successful response with the retrieved e-card record.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when the e-card record does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new e-card record.
        /// </summary>
        /// <param name="ecard">The <see cref="EcardAddUpdateModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Add Card")]

        [HttpPost]
        public async Task<IActionResult> Post(EcardAddUpdateModel ecard)
        {
            /// <summary>
            /// Validates the model state before processing.
            /// </summary>
            /// 
            var userName = User.GetUserName();
            ecard.CreatedBy = userName;
            ecard.UpdatedBy = userName;
            if (!ModelState.IsValid)
            {
                /// <summary>
                /// Returns bad request response for invalid model state.
                /// </summary>
                return BadRequest(ModelState);
            }

            /// <summary>
            /// Gets the current user's entity ID.
            /// </summary>
            var entityId = User.GetTenantId();

            /// <summary>
            /// Calls the service to add a new e-card record for the entity.
            /// </summary>
            await _ecardService.Add(entityId, ecard);

            /// <summary>
            /// Returns successful response indicating the e-card record was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing e-card record.
        /// </summary>
        /// <param name="ecard">The <see cref="EcardUpdateModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [RequireRole("Edit Card")]

        [HttpPut]
        public async Task<IActionResult> Put(EcardUpdateModel ecard)
        {
            /// <summary>
            /// Sets the entity ID from the current user's context.
            /// </summary>
            /// 
            var userName = User.GetUserName();
            ecard.UpdatedBy = userName;
            ecard.TenantId = User.GetTenantId();

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
            /// Calls the service to update an existing e-card record.
            /// </summary>
            await _ecardService.Update(ecard);

            /// <summary>
            /// Returns successful response indicating the e-card record was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes an e-card record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the e-card to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Card")]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Wraps the delete operation in a try-catch block for error handling.
            /// </summary>
            try
            {
                /// <summary>
                /// Calls the service to delete an e-card record for the current user's entity.
                /// </summary>
                string resultMessage = await _ecardService.Delete(User.GetTenantId(), id);

                /// <summary>
                /// Returns successful response with the delete result message.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            /// <summary>
            /// Catches any exceptions that occur during the delete process.
            /// </summary>
            catch (Exception ex)
            {
                /// <summary>
                /// Returns bad request response with the exception message.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes multiple e-card records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the e-cards to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Card")]

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
            /// Wraps the multiple delete operation in a try-catch block for error handling.
            /// </summary>
            try
            {
                /// <summary>
                /// Calls the service to delete multiple e-card records for the current user's entity.
                /// </summary>
                string resultMessage = await _ecardService.RemoveMultiple(User.GetTenantId(), ids);

                /// <summary>
                /// Returns successful response with the delete result message.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            /// <summary>
            /// Catches any exceptions that occur during the multiple delete process.
            /// </summary>
            catch (Exception ex)
            {
                /// <summary>
                /// Returns bad request response with the exception message.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Imports e-cards from a file for the current entity.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [RequireRole("Import Card")]

        [HttpPost("import")]
        public async Task<IActionResult> ImportECard(IFormFile file)
        {
            /// <summary>
            /// Validates that a file was uploaded and has content.
            /// </summary>
            /// 
            var userName = User.GetUserName();
            if (file == null || file.Length == 0)
                /// <summary>
                /// Returns bad request response for no file uploaded.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });

            /// <summary>
            /// Wraps the import operation in a try-catch block for error handling.
            /// </summary>
            try
            {
                /// <summary>
                /// Calls the service to import e-cards from the file for the current user's entity.
                /// </summary>
                string resultMessage = await _ecardService.ImportECard(User.GetTenantId(), file.OpenReadStream(), userName ?? "");

                /// <summary>
                /// Returns successful response with the import result message.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            /// <summary>
            /// Catches any exceptions that occur during the import process.
            /// </summary>
            catch (Exception ex)
            {
                /// <summary>
                /// Returns bad request response with the exception message.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Downloads the e-card import template for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        /// 

        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            /// <summary>
            /// Calls the service to download the e-card import template for the current user's entity.
            /// </summary>
            var excelBytes = await _ecardService.DownloadTemplate(User.GetTenantId());

            /// <summary>
            /// Returns the template file as a downloadable Excel document.
            /// </summary>
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Parameter-Template.xlsm");
        }

        /// <summary>
        /// Exports selected e-cards for the current entity.
        /// </summary>
        /// <param name="selectedEntityIds">The list of selected e-card IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [RequireRole("Export Card")]

        [HttpPost("export")]
        public async Task<IActionResult> ExportECard([FromBody] List<int> selectedEntityIds)
        {
            /// <summary>
            /// Calls the service to export e-cards for the current user's entity.
            /// </summary>
            var stream = await _ecardService.ExportECard(User.GetTenantId(), selectedEntityIds);

            /// <summary>
            /// Returns the exported file as a downloadable Excel document.
            /// </summary>
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Ecards.xlsx");
        }
    }
}
