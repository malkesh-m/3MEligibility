using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
//using EligibilityPlatform.Infrastructure.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing bulk import operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BulkImportController"/> class.
    /// </remarks>
    /// <param name="bulkImportService">The bulk import service.</param>
    [Route("api/bulkimport")]
    [ApiController]
    [Authorize]
    public class BulkImportController(IBulkImportService bulkImportService) : Controller
    {
        /// <summary>
        /// The bulk import service instance for import operations.
        /// </summary>
        private readonly IBulkImportService _bulkImportService = bulkImportService;

        /// <summary>
        /// Retrieves all import document history records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ImportDocument"/> objects or not found if none exist.</returns>
        [RequireRole("View Bulk Import Screen")]

        [HttpGet("getAll")]
        public IActionResult Get()
        {
            /// <summary>
            /// Retrieves all import document history records from the service.
            /// </summary>
            List<ImportDocument> documents = _bulkImportService.GetAllImportHistory();

            /// <summary>
            /// Checks if any documents were found.
            /// </summary>
            if (documents == null || documents.Count == 0)
            {
                /// <summary>
                /// Returns not found response when no import history records exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = false, Message = "No records found." });
            }

            /// <summary>
            /// Returns successful response with the import document history.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = documents, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Downloads an imported file by document ID.
        /// </summary>
        /// <param name="id">The document ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the file or not found if it does not exist.</returns>
        /// 
        [RequireRole("Download Bulk Import")]

        [HttpPost("download")]
        public async Task<IActionResult> GetimportedFile([FromForm] int id)
        {
            /// <summary>
            /// Calls the service to download the imported file by document ID.
            /// </summary>
            byte[] excelBytes = await _bulkImportService.DownloadImportedFile(id);

            /// <summary>
            /// Checks if the file data was retrieved successfully.
            /// </summary>
            if (excelBytes == null || excelBytes.Length == 0)
            {
                /// <summary>
                /// Returns not found response when the file does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = false, Message = "No records found." });
            }

            /// <summary>
            /// Returns the file as a downloadable Excel document.
            /// </summary>
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ImportedFile.xlsm");
        }

        /// <summary>
        /// Downloads a template for bulk import based on the selected list.
        /// </summary>
        /// <param name="selectedList">The selected list for which to download the template.</param>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        /// 

        [HttpPost("download-template")]
        public async Task<IActionResult> DownloadTemplate([FromForm] string selectedList)
        {
            /// <summary>
            /// Calls the service to download the import template for the specified list.
            /// </summary>
            byte[] excelBytes = await _bulkImportService.DownloadTemplate(User.GetTenantId(), selectedList);

            /// <summary>
            /// Returns the template file as a downloadable Excel document.
            /// </summary>
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template.xlsm");
        }

        /// <summary>
        /// Performs a bulk import operation using the provided file and creator information.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [RequireRole("Import Bulk Import")]

        [HttpPost("import")]
        public async Task<IActionResult> BulkImport(IFormFile file)
        {
            /// <summary>
            /// Validates that a file was uploaded and has content.
            /// </summary>
            /// 
            var userName = User.GetUserName() ?? "";
            var createdBy = userName;
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
                var entityId = User.GetTenantId();
                /// <summary>
                /// Calls the service to perform the bulk import operation.
                /// </summary>
                string resultMessage = await _bulkImportService.BulkImport(file.OpenReadStream(), file.FileName, createdBy, entityId);

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
    }
}
