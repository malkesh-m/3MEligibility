using Azure.Identity;
using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing erule operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EruleController"/> class.
    /// </remarks>
    /// <param name="eruleService">The erule service.</param>
    /// <param name="eruleMasterService">The erule master service.</param>
    [Route("api/erule")]
    [ApiController]
    public class EruleController(IEruleService eruleService, IEruleMasterService eruleMasterService) : ControllerBase
    {
        private readonly IEruleService _eruleService = eruleService;
        private readonly IEruleMasterService _eruleMasterService = eruleMasterService;

        /// <summary>
        /// Retrieves all erule records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="EruleListModel"/> objects.</returns>
        /// 
        [RequireRole("View Rules Screen")]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            /// <summary>
            /// Retrieves all erules for the current entity and returns a success response.
            /// </summary>
            List<EruleListModel> result = _eruleService.GetAll(User.GetTenantId());
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves an erule record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the erule.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="EruleListModel"/> if found; otherwise, not found.</returns>
        [RequireRole("View Rules Screen")]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /// <summary>
            /// Retrieves a specific erule by ID for the current entity and returns appropriate response.
            /// </summary>
            var result = _eruleService.GetById(User.GetTenantId(), id);
            if (result != null)
            {
                /// <summary>
                /// Returns success response with the found erule data.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when erule does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new erule record.
        /// </summary>
        /// <param name="model">The <see cref="EruleCreateModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Add Rule")]

        [HttpPost]
        public async Task<IActionResult> Post(EruleCreateModel model)
        {
            /// <summary>
            /// Sets the entity ID from the current user context.
            /// </summary>
            /// 
            var userName = User.GetUserName();
            model.CreatedBy = userName;
            model.UpdatedBy = userName;
            model.TenantId = User.GetTenantId();
            /// <summary>
            /// Validates the model state before proceeding with erule creation.
            /// </summary>
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            /// <summary>
            /// Creates the new erule and returns a success response.
            /// </summary>
            await _eruleService.Create(model);
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Adds a new erule master record.
        /// </summary>
        /// <param name="model">The <see cref="EruleMasterCreateUpodateModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Add Rule")]

        [HttpPost("adderulemaster")]
        public async Task<IActionResult> AddEruleMaster(EruleMasterCreateUpodateModel model)
        {
            /// <summary>
            /// Sets the entity ID from the current user context.
            /// </summary>
            model.TenantId = User.GetTenantId();
            /// <summary>
            /// Validates the model state before proceeding with erule master creation.
            /// </summary>
            /// 
            var userName = User.GetUserName();
            model.CreatedBy = userName;
            model.UpdatedBy = userName;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            /// <summary>
            /// Adds the new erule master and returns a success response.
            /// </summary>
            await _eruleMasterService.Add(model, User.GetTenantId());
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing erule record.
        /// </summary>
        /// <param name="erule">The <see cref="EruleUpdateModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Rule")]

        [HttpPut]
        public async Task<IActionResult> Put(EruleUpdateModel erule)
        {
            /// <summary>
            /// Sets the entity ID from the current user context.
            /// </summary>
            erule.TenantId = User.GetTenantId();

            var userName = User.GetUserName();
            erule.UpdatedBy = userName;
            /// <summary>
            /// Validates the model state before proceeding with erule update.
            /// </summary>
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            /// <summary>
            /// Updates the erule and returns a success response.
            /// </summary>
            await _eruleService.Update(erule);
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Updates an existing erule master record.
        /// </summary>
        /// <param name="erule">The <see cref="EruleMasterCreateUpodateModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Rule")]

        [HttpPut("editerulemaster")]
        public async Task<IActionResult> EditEruleMaster(EruleMasterCreateUpodateModel erule)
        {
            /// <summary>
            /// Retrieves the entity ID from the current user context.
            /// </summary>
            var EntityId = User.GetTenantId();
            /// <summary>
            /// Validates the model state before proceeding with erule master update.
            /// </summary>
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            /// <summary>
            /// Edits the erule master and returns a success response.
            /// </summary>
            await _eruleMasterService.Edit(erule, EntityId);
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Retrieves all erule records by erule master ID for the current entity.
        /// </summary>
        /// <param name="EruleMasterId">The erule master ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the erule records.</returns>
        /// 
        [RequireRole("View Rules Screen")]

        [HttpGet("getbyerulemasterid")]
        public async Task<IActionResult> GetByEruleMasterId(int EruleMasterId)
        {
            /// <summary>
            /// Retrieves all erules by master ID for the current entity and returns a success response.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = await _eruleService.GetAllByEruleMasterId(EruleMasterId, User.GetTenantId()), Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Publishes a draft erule for the current entity.
        /// </summary>
        /// <param name="draftEruleId">The draft erule ID.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPut("publish-draft")]
        public async Task<IActionResult> UpdateDraft(int draftEruleId)
        {
            /// <summary>
            /// Retrieves the entity ID from the current user context.
            /// </summary>
            int entityId = User.GetTenantId();
            /// <summary>
            /// Publishes the draft erule and returns a success response.
            /// </summary>
            await _eruleService.PublishDraftAsync(draftEruleId, entityId);
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Updates the status of an erule for the current entity.
        /// </summary>
        /// <param name="eruleId">The erule ID.</param>
        /// <param name="isActive">The new status value.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Rule")]

        [HttpPut("updatestatus")]
        public async Task<IActionResult> UpdateStatus(int eruleId, bool isActive)
        {
            /// <summary>
            /// Retrieves the entity ID from the current user context.
            /// </summary>
            int entityId = User.GetTenantId();
            /// <summary>
            /// Updates the erule status and returns a success response.
            /// </summary>
            await _eruleService.UpdateStatusAsync(eruleId, entityId, isActive);
            return Ok(new ResponseModel { IsSuccess = true, Message = "Status updated successfully." });
        }

        /// <summary>
        /// Deletes an erule record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the erule to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Rule")]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                /// <summary>
                /// Deletes the specified erule and returns a success response with result message.
                /// </summary>
                string resultMessage = await _eruleService.Delete(User.GetTenantId(), id);
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            catch (Exception ex)
            {
                /// <summary>
                /// Handles exceptions during erule deletion and returns an error response.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes multiple erule records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the erules to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Rule")]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            /// <summary>
            /// Validates that IDs were provided before proceeding with multiple deletion.
            /// </summary>
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
            }
            try
            {
                /// <summary>
                /// Deletes multiple erules and returns a success response with result message.
                /// </summary>
                string resultMessage = await _eruleService.RemoveMultiple(User.GetTenantId(), ids);
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            catch (Exception ex)
            {
                /// <summary>
                /// Handles exceptions during multiple erule deletion and returns an error response.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Imports erules from a file for the current entity.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [RequireRole("Import Rule")]

        [HttpPost("import")]
        public async Task<IActionResult> ImportErule(IFormFile file, string createdBy)
        {
            /// <summary>
            /// Validates that a file was uploaded before proceeding with import.
            /// </summary>
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });
            try
            {
                /// <summary>
                /// Processes the erule file import and returns the result message.
                /// </summary>
                string resultMessage = await _eruleService.ImportErule(User.GetTenantId(), file.OpenReadStream(), createdBy);
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            catch (Exception ex)
            {
                /// <summary>
                /// Handles exceptions during erule import and returns an error response.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }
        [RequireRole("Import Rule")]

        [HttpPost("importerulemaster")]
        public async Task<IActionResult> ImportEruleMaster(IFormFile file)
        {

            var userName = User.GetUserName();
            /// <summary>
            /// Validates that a file was uploaded before proceeding with import.
            /// </summary>
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });
            try
            {
                /// <summary>
                /// Processes the erule file import and returns the result message.
                /// </summary>
                string resultMessage = await _eruleService.ImportEruleMaster(User.GetTenantId(), file.OpenReadStream(), userName ?? "");
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            catch (Exception ex)
            {
                /// <summary>
                /// Handles exceptions during erule import and returns an error response.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Downloads the erule import template for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            /// <summary>
            /// Generates and returns the erule import template as an Excel file download.
            /// </summary>
            var excelBytes = await _eruleService.DownloadTemplate(User.GetTenantId());
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Parameter-Template.xlsm");
        }
        [HttpGet("download-templateerulemaster")]
        public async Task<IActionResult> DownloadTemplateEruleMaster()
        {
            /// <summary>
            /// Generates and returns the erule import template as an Excel file download.
            /// </summary>
            var excelBytes = await _eruleService.DownloadTemplateEruleMaster(User.GetTenantId());
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Parameter-Template.xlsm");
        }

        /// <summary>
        /// Exports selected erules for the current entity.
        /// </summary>
        /// <param name="selectedEruleIds">The list of selected erule IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [RequireRole("Export Rule")]

        [HttpPost("export")]
        public async Task<IActionResult> ExportERule([FromBody] List<int> selectedEruleIds)
        {
            /// <summary>
            /// Exports the selected erules and returns the Excel file as a download.
            /// </summary>
            var stream = await _eruleService.ExportErule(User.GetTenantId(), selectedEruleIds);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "rules.xlsx");
        }

        [RequireRole("Delete Rule")]

        [HttpDelete("deleteerulemaster")]
        public async Task<IActionResult> DeleteRule(int id)
        {
            var message = await _eruleMasterService.Delete(id);

            return Ok(new ResponseModel
            {
                IsSuccess = message != "Erule not found.",
                Message = message
            });
        }

        [RequireRole("Delete Rule")]

        [HttpDelete("multipledeleteerulemaster")]
        public async Task<IActionResult> DeleteMultipleMaster([FromBody] List<int> ids)
        {
            /// <summary>
            /// Validates that IDs were provided before proceeding with multiple deletion.
            /// </summary>
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
            }
            try
            {
                /// <summary>
                /// Deletes multiple erules and returns a success response with result message.
                /// </summary>
                string resultMessage = await _eruleMasterService.RemoveMultiple(User.GetTenantId(), ids);
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            catch (Exception ex)
            {
                /// <summary>
                /// Handles exceptions during multiple erule deletion and returns an error response.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }
    }
}
