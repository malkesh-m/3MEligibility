using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing pcard operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PcardController"/> class.
    /// </remarks>
    /// <param name="pcardService">The pcard service.</param>
    [Route("api/pcard")]
    [ApiController]
    public class PcardController(IPcardService pcardService) : ControllerBase
    {
        private readonly IPcardService _pcardService = pcardService;

        /// <summary>
        /// Retrieves all pcard records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="PcardListModel"/> objects.</returns>
        /// 
        [RequireRole("View Product Cards Screen")]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all pcard records for the current entity
            List<PcardListModel> result = _pcardService.GetAll(User.GetTenantId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a pcard record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the pcard record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="PcardListModel"/> if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Product Cards Screen")]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a pcard record by ID for the current entity
            var result = _pcardService.GetById(User.GetTenantId(), id);
            // Checks if the pcard record was found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when pcard record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new pcard record for the current entity.
        /// </summary>
        /// <param name="pcard">The pcard model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Add Product Card")]

        [HttpPost]
        public async Task<IActionResult> Post(PcardAddUpdateModel pcard)
        {
            // Sets entity ID and user information from the current user context
            pcard.TenantId = User.GetTenantId();
            string? UserName = User.GetUserName();
            pcard.CreatedBy = UserName;
            pcard.UpdatedBy = UserName;

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }

            try
            {
                // Adds the new pcard record
                string resultMessage = await _pcardService.Add(pcard);
                // Returns success response for created operation
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            catch (Exception ex)
            {
                // Returns bad request if an exception occurs during the operation
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing pcard record for the current entity.
        /// </summary>
        /// <param name="pcard">The pcard model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Product Card")]

        [HttpPut]
        public async Task<IActionResult> Put(PcardUpdateModel pcard)
        {
            var userName = User.GetUserName();
            pcard.UpdatedBy = userName;
            // Sets entity ID from the current user context
            pcard.TenantId = User.GetTenantId();

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }

            // Updates the existing pcard record
            await _pcardService.Update(pcard);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a pcard record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the pcard record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Product Card")]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the pcard record by ID for the current entity
            await _pcardService.Delete(User.GetTenantId(), id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Exports selected pcards for the current entity.
        /// </summary>
        /// <param name="selectedPcardIds">The list of selected pcard IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [RequireRole("Export Product Card")]

        [HttpPost("export")]
        public async Task<IActionResult> ExportPCards([FromBody] List<int> selectedPcardIds)
        {
            // Exports selected pcard records for the current entity
            var stream = await _pcardService.ExportPCards(User.GetTenantId(), selectedPcardIds);
            // Returns the exported file as a downloadable Excel document
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "entities.xlsx");
        }

        /// <summary>
        /// Deletes multiple pcard records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the pcard records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Product Card")]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            // Validates that IDs are provided
            if (ids == null || ids.Count == 0)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
            }

            // Deletes multiple pcard records for the current entity
            await _pcardService.RemoveMultiple(User.GetTenantId(), ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Imports pcards from a file for the current entity.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [RequireRole("Import Product Card")]

        [HttpPost("import")]
        public async Task<IActionResult> ImportPCards(IFormFile file)
        {
            var userName = User.GetUserName();
            // Validates that a file is provided
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });

            try
            {
                // Imports pcard records from the provided file
                string resultMessage = await _pcardService.ImportPCards(User.GetTenantId(), file.OpenReadStream(), userName ?? "");
                // Returns success response for import operation
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            catch (Exception ex)
            {
                // Returns bad request if an exception occurs during import
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Downloads the pcard import template for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Downloads the pcard import template for the current entity
            var excelBytes = await _pcardService.DownloadTemplate(User.GetTenantId());
            // Returns the template file as a downloadable Excel document
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Parameter-Template.xlsm");
        }
    }
}
