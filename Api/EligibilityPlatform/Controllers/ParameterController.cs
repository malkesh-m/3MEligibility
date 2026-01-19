using System.Net;
using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Constants;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{/// <summary>
 /// API controller for managing parameter operations.
 /// </summary>
 /// <remarks>
 /// Initializes a new instance of the <see cref="ParameterController"/> class.
 /// </remarks>
 /// <param name="parameterService">The parameter service.</param>
    [Route("api/parameter")]
    [ApiController]
    [Authorize]
    public class ParameterController(IParameterService parameterService) : ControllerBase
    {
        private readonly IParameterService _parameterService = parameterService;

        /// <summary>
        /// Retrieves all parameter records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ParameterListModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.Parameter.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all parameter records for the current entity
            List<ParameterListModel> result = _parameterService.GetAll(User.GetTenantId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves all parameter records by entity ID.
        /// </summary>
        /// <param name="entityid">The entity ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ParameterListModel"/> objects.</returns>
        /// 

        [HttpGet("getallbyentityid"), AllowAnonymous]
        public IActionResult GetByEntityId(int entityid)
        {
            // Retrieves all parameter records by entity ID
            List<ParameterListModel> result = _parameterService.GetByEntityId(entityid);
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a parameter record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the parameter record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="ParameterListModel"/> if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.Parameter.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a parameter record by ID for the current entity
            var result = _parameterService.GetById(User.GetTenantId(), id);
            // Checks if the record was found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new parameter record for the current entity.
        /// </summary>
        /// <param name="parameter">The parameter model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Parameter.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(ParameterAddUpdateModel parameter)
        {
            // Sets user information and entity ID from the current user
            string? UserName = User.GetUserName();
            parameter.CreatedBy = UserName;
            parameter.UpdatedBy = UserName;
            parameter.TenantId = User.GetTenantId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new parameter record
            await _parameterService.Add(parameter);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing parameter record for the current entity.
        /// </summary>
        /// <param name="parameter">The parameter model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Parameter.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(ParameterAddUpdateModel parameter)
        {
            // Sets user information and entity ID from the current user
            string? UserName = User.GetUserName();
            parameter.UpdatedBy = UserName;
            parameter.TenantId = User.GetTenantId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing parameter record
            await _parameterService.Update(parameter);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a parameter record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the parameter record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        //[HttpDelete]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {
        //        await _parameterService.Delete(User.GetTenantId(), id);
        //        return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        // Handle FK constraint violation
        //        return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new ResponseModel
        //        {
        //            IsSuccess = false,
        //            Message = "An unexpected error occurred: " + ex.Message
        //        });
        //    }
        //}

        [Authorize(Policy = Permissions.Parameter.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _parameterService.Delete(User.GetTenantId(), id);
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Exports selected parameters for the current entity.
        /// </summary>
        /// <param name="Identifier">The identifier for export context.</param>
        /// <param name="selectedParameterIds">The list of selected parameter IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [Authorize(Policy = Permissions.Parameter.Export)]

        [HttpPost("export")]
        public async Task<IActionResult> ExportParameter(int Identifier, [FromBody] List<int> selectedParameterIds)
        {
            // Exports selected parameters to a stream
            var stream = await _parameterService.ExportParameter(User.GetTenantId(), Identifier, selectedParameterIds);
            // Returns the exported file as a downloadable Excel file
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Parameter.xlsx");
        }

        /// <summary>
        /// Imports customer parameters from a file for the current entity.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Parameter.Import)]

        [HttpPost("importcustomer")]
        public async Task<IActionResult> ImportParameterCustomer(IFormFile file, string createdBy)
        {
            // Validates if file is provided and not empty
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });
            try
            {
                // Imports customer parameters from the provided file
                string resultMessage = await _parameterService.ImportEntities(User.GetTenantId(), file.OpenReadStream(), 1, createdBy);
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
        /// Imports product parameters from a file for the current entity.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Parameter.Import)]

        [HttpPost("importproduct")]
        public async Task<IActionResult> ImportParameterProduct(IFormFile file)
        {
            var userName = User.GetUserName();
            // Validates if file is provided and not empty
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });
            try
            {
                // Imports product parameters from the provided file
                string resultMessage = await _parameterService.ImportEntities(User.GetTenantId(), file.OpenReadStream(), 2, userName ?? "");
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
        /// Deletes multiple parameter records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the parameter records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Parameter.Delete)]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            // Validates if IDs are provided
            if (ids == null || ids.Count == 0)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
            }
            // Deletes multiple parameter records
            await _parameterService.RemoveMultiple(User.GetTenantId(), ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Retrieves parameters by product ID for the current entity.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the parameters if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.Parameter.View)]
        [HttpGet("getparametersbyproductid")]
        public IActionResult GetParametrsByProductId(int productId)
        {
            // Retrieves parameters by product ID for the current entity
            var result = _parameterService.GetParameterByProducts(User.GetTenantId(), productId);
            // Checks if parameters were found
            if (result == null)
            {
                // Returns bad request if parameters are not found
                return BadRequest(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Downloads the parameter import template.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Retrieves the import template as byte array
            var excelBytes = await _parameterService.DownloadTemplate();
            // Returns the template file as a downloadable Excel file
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Parameter-Template.xlsx");
        }

        /// <summary>
        /// Checks the computed value of a parameter for the current entity.
        /// </summary>
        /// <param name="parameterId">The parameter ID.</param>
        /// <param name="parameterValue">The parameter value to check.</param>
        /// <returns>An <see cref="IActionResult"/> containing the computed value if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.Parameter.CheckComputedValue)]

        [HttpGet("check-parameter-computed-value/{parameterId}/{parameterValue}")]
        public async Task<IActionResult> CheckParameterComputedValue(int parameterId, string parameterValue)
        {
            // Checks the computed value of a parameter
            var result = await _parameterService.CheckParameterComputedValue(User.GetTenantId(), parameterId, parameterValue);
            // Checks if the computed value was found
            if (result != null)
            {
                // Returns success response with the computed value
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when computed value doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }
    }
}
