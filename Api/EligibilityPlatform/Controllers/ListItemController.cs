using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing list item operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ListItemController"/> class.
    /// </remarks>
    /// <param name="listItemService">The list item service.</param>
    [Route("api/listitem")]
    [ApiController]
    public class ListItemController(IListItemService listItemService) : ControllerBase
    {
        /// <summary>
        /// The list item service instance.
        /// </summary>
        private readonly IListItemService _listItemService = listItemService;

        /// <summary>
        /// Retrieves all list item records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ListItemModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.ListItem.View)]

        [HttpGet("getall")]
        public async Task<IActionResult> Get()
        {
            var tenantId = User.GetTenantId();
            // Retrieves all list item records
            List<ListItemModel> result = await _listItemService.GetAll(tenantId);
            // Returns success response with the retrieved list item list
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a list item record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the list item record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="ListItemModel"/> if found; otherwise, not found.</returns>

        [Authorize(Policy = Permissions.ListItem.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var tenantId = User.GetTenantId();
            // Retrieves a list item record by ID
            var result = _listItemService.GetById(id,tenantId);
            // Checks if the list item record was found
            if (result != null)
            {
                // Returns success response with the retrieved list item data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when list item record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new list item record.
        /// </summary>
        /// <param name="listItem">The <see cref="ListItemModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.ListItem.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(ListItemCreateUpdateModel listItem)
        {
            var tenantId = User.GetTenantId();
            var userName = User.GetUserName();
            listItem.CreatedBy = userName;
            listItem.UpdatedBy = userName;
            listItem.TenantId = tenantId;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new list item record
            await _listItemService.Add(listItem);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing list item record.
        /// </summary>
        /// <param name="listItem">The <see cref="ListItemModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ListItem.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(ListItemCreateUpdateModel listItem)
        {
            var tenantId = User.GetTenantId();
            listItem.TenantId = tenantId;

            var userName = User.GetUserName();
            listItem.UpdatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing list item record
            await _listItemService.Update(listItem);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a list item record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the list item record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ListItem.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the list item record by ID
            await _listItemService.Delete(id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple list item records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the list item records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ListItem.Delete)]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple list item records
            await _listItemService.MultipleDelete(ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Exports selected list items.
        /// </summary>
        /// <param name="selectedListItemIds">The list of selected list item IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [Authorize(Policy = Permissions.ListItem.Export)]

        [HttpPost("export")]
        public async Task<IActionResult> ExportListIteam([FromBody] ExportRequestModel request)
        {
            // Exports list items based on standardized logic
            var stream = await _listItemService.ExportListIteam(User.GetTenantId(), request);
            // Returns the exported file as Excel document
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Factor.xlsx");
        }

        /// <summary>
        /// Imports list items from a file.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ListItem.Import)]

        [HttpPost("import")]
        public async Task<IActionResult> ImportListIteams(IFormFile file)
        {
            var createdBy = User.GetUserName();
            var tenantId = User.GetTenantId();  
            // Validates if file exists and has content
            if (file == null || file.Length == 0)
                // Returns bad request if no file is uploaded
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });
            try
            {
                // Imports list items from the uploaded file stream
                string resultMessage = await _listItemService.ImportListIteams(file.OpenReadStream(), createdBy ?? "", tenantId);
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
        /// Downloads the list item import template for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Downloads the list item import template
            var excelBytes = await _listItemService.DownloadTemplate(User.GetTenantId());
            // Returns the template file as Excel document
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Parameter-Template.xlsx");
        }
    }
}
