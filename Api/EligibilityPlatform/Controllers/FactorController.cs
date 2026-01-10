using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing factor operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FactorController"/> class.
    /// </remarks>
    /// <param name="factorService">The factor service.</param>
    [Route("api/factors")]
    [ApiController]
    [Authorize]
    public class FactorController(IFactorService factorService) : ControllerBase
    {
        /// <summary>
        /// The factor service instance.
        /// </summary>
        private readonly IFactorService _factorService = factorService;

        /// <summary>
        /// Retrieves all factor records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="FactorListModel"/> objects.</returns>
        /// 
        [RequireRole("View Factors Screen")]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all factor records for the current entity
            List<FactorListModel> result = _factorService.GetAll(User.GetEntityId());
            // Returns success response with the retrieved factor list
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a factor record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the factor.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="FactorListModel"/> if found; otherwise, not found.</returns>
        [RequireRole("View Factors Screen")]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a factor record by ID for the current entity
            var result = _factorService.GetById(User.GetEntityId(), id);
            // Checks if the factor was found
            if (result != null)
            {
                // Returns success response with the retrieved factor data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when factor doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new factor record.
        /// </summary>
        /// <param name="factor">The <see cref="FactorAddUpdateModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Add new Factor")]
        [HttpPost]
        public async Task<IActionResult> Post(FactorAddUpdateModel factor)
        {
            // Gets the current user's name
            string? UserName = User.Identity?.Name;
            // Sets the created by field with current user
            factor.CreatedBy = UserName;
            // Sets the updated by field with current user
            factor.UpdatedBy = UserName;
            // Sets the entity ID from current user context
            factor.EntityId = User.GetEntityId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new factor record
            await _factorService.Add(factor);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing factor record.
        /// </summary>
        /// <param name="factor">The <see cref="FactorAddUpdateModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Factor")]
        [HttpPut]
        public async Task<IActionResult> Put(FactorAddUpdateModel factor)
        {
            // Sets the entity ID from current user context
            factor.EntityId = User.GetEntityId();
            var userName = User.Identity!.Name;
            factor.UpdatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing factor record
            await _factorService.Update(factor);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a factor record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the factor to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Factor")]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the factor record by ID for current entity
            await _factorService.Delete(User.GetEntityId(), id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Exports selected factors for the current entity.
        /// </summary>
        /// <param name="selectedFactorIds">The list of selected factor IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [RequireRole("Export Factors")]

        [HttpPost("export")]
        public async Task<IActionResult> ExportFactors([FromBody] List<int> selectedFactorIds)
        {
            // Exports selected factors and gets the file stream
            var stream = await _factorService.ExportFactors(User.GetEntityId(), selectedFactorIds);
            // Returns the exported file as Excel document
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Factor.xlsx");
        }

        /// <summary>
        /// Imports factors from a file for the current entity.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [RequireRole("Import factor")]

        [HttpPost("import")]
        public async Task<IActionResult> ImportFactor(IFormFile file)
        {
            var userName = User.Identity!.Name;

            // Validates if file exists and has content
            if (file == null || file.Length == 0)
                // Returns bad request if no file is uploaded
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });
            try
            {
                // Imports factors from the uploaded file stream
                string resultMessage = await _factorService.ImportFactor(User.GetEntityId(), file.OpenReadStream(), userName ?? "");
                // Returns success response with import result message
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            catch (Exception ex)
            {
                // Returns bad request if import operation fails
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes multiple factor records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the factors to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Factor")]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            // Validates if IDs list is provided and not empty
            if (ids == null || ids.Count == 0)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
            }
            // Deletes multiple factor records by IDs
            await _factorService.RemoveMultiple(User.GetEntityId(), ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Retrieves the value by parameter ID for the current entity.
        /// </summary>
        /// <param name="parameterId">The parameter ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the value if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Factors Screen")]

        [HttpGet("getvaluebyparameterid")]
        public IActionResult GetValueByParams(int parameterId)
        {
            // Retrieves value by parameter ID for current entity
            var result = _factorService.GetValueByParams(User.GetEntityId(), parameterId);
            // Checks if result is found
            if (result != null)
            {
                // Returns success response with retrieved value
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when value doesn't exist
                return NotFound(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Retrieves factors by condition ID for the current entity.
        /// </summary>
        /// <param name="conditionId">The condition ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the factors if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Factors Screen")]

        [HttpGet("getfactorsbyconditionid")]
        public IActionResult GetFactorsByConditionId(int conditionId)
        {
            // Retrieves factors by condition ID for current entity
            var result = _factorService.GetFactorByCondition(User.GetEntityId(), conditionId);
            // Checks if result is null
            if (result == null)
            {
                // Returns bad request if no factors found
                return BadRequest(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
            // Returns success response with retrieved factors
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves factors by parameter ID for the current entity.
        /// </summary>
        /// <param name="parameterid">The parameter ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the factors if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Factors Screen")]

        [HttpGet("getfactorsbyparameterid")]
        public IActionResult GetFactorsByParameterId(int parameterid)
        {
            // Retrieves factors by parameter ID for current entity
            var result = _factorService.GetFactorByparameter(User.GetEntityId(), parameterid);
            // Checks if result is null
            if (result == null)
            {
                // Returns bad request if no factors found
                return BadRequest(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
            // Returns success response with retrieved factors
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Downloads the factor import template for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Downloads the import template Excel bytes
            var excelBytes = await _factorService.DownloadTemplate(User.GetEntityId());
            // Returns the template file as Excel document
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Parameter-Template.xlsx");
        }
    }
}