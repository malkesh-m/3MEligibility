using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing managed list operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ManagedListController"/> class.
    /// </remarks>
    /// <param name="managedListService">The managed list service.</param>
    [Route("api/managedlist")]
    [ApiController]
    public class ManagedListController(IManagedListService managedListService) : ControllerBase
    {
        private readonly IManagedListService _managedListService = managedListService;

        /// <summary>
        /// Retrieves all managed list records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of managed list records.</returns>
        /// 
        [Authorize(Policy = Permissions.ManagedList.View)]

        [HttpGet("getall")]
        public async Task<IActionResult> Get()
        {
            // Retrieves all managed list records for the current entity
            List<ManagedListGetModel> result = await _managedListService.GetAll(User.GetTenantId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a managed list record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the managed list record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the record if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.ManagedList.View)]

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            // Retrieves a managed list record by ID for the current entity
            var result = _managedListService.GetById(User.GetTenantId(), id);
            // Checks if the record was found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new managed list record.
        /// </summary>
        /// <param name="managedList">The managed list model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ManagedList.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(ManagedListAddUpdateModel managedList)
        {
            // Sets entity ID and user information from the current user
            managedList.TenantId = User.GetTenantId();
            string? UserName = User.GetUserName();
            managedList.CreatedBy = UserName;
            managedList.UpdatedBy = UserName;

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new managed list record
            await _managedListService.Add(managedList);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing managed list record.
        /// </summary>
        /// <param name="managedList">The managed list model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ManagedList.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(ManagedListUpdateModel managedList)
        {
            // Sets user information and entity ID from the current user
            string? UserName = User.GetUserName();
            managedList.UpdatedBy = UserName;
            managedList.TenantId = User.GetTenantId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing managed list record
            await _managedListService.Update(managedList);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a managed list record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the managed list record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ManagedList.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the managed list record by ID for the current entity
            await _managedListService.Delete(User.GetTenantId(), id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple managed list records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the managed list records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ManagedList.Delete)]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple managed list records
            await _managedListService.MultipleDelete(User.GetTenantId(), ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Exports selected managed lists for the current entity.
        /// </summary>
        /// <param name="selectedListIds">The list of selected managed list IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [Authorize(Policy = Permissions.ManagedList.Export)]

        [HttpPost("export")]
        public async Task<IActionResult> ExportLists([FromBody] ExportRequestModel request)
        {
            // Exports managed lists based on standardized logic
            var stream = await _managedListService.ExportLists(User.GetTenantId(), request);
            // Returns the exported file as a downloadable Excel file
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Factor.xlsx");
        }

        /// <summary>
        /// Imports managed lists from a file for the current entity.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ManagedList.Import)]

        [HttpPost("import")]
        public async Task<IActionResult> ImportList(IFormFile file)
        {
            var userName = User.GetUserName();
            // Validates if file is provided and not empty
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });

            try
            {
                // Imports managed lists from the provided file
                string resultMessage = await _managedListService.ImportList(User.GetTenantId(), file.OpenReadStream(), userName ?? "");
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
        /// Downloads the managed list import template.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<ActionResult> DownloadTemplate()
        {
            // Retrieves the import template as byte array
            var excelBytes = await _managedListService.DownloadTemplate(User.GetTenantId());
            // Returns the template file as a downloadable Excel file
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ManagedList-Template.xlsx");
        }
    }
}
