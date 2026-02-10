using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing category operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CategoryController"/> class.
    /// </remarks>
    /// <param name="categoryService">The category service.</param>
    /// 

    [Route("api/category")]
    [ApiController]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        /// <summary>
        /// The category service instance for category operations.
        /// </summary>
        private readonly ICategoryService _categoryService = categoryService;

        /// <summary>
        /// Retrieves all categories for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="CategoryListModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.Category.View)]

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            /// <summary>
            /// Retrieves all categories for the current user's entity from the service.
            /// </summary>
            List<CategoryListModel> result = _categoryService.GetAll(User.GetTenantId());

            /// <summary>
            /// Returns successful response with the retrieved categories.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a category by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="CategoryListModel"/> if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.Category.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /// <summary>
            /// Retrieves a specific category by ID for the current user's entity from the service.
            /// </summary>
            var result = _categoryService.GetById(User.GetTenantId(), id);

            /// <summary>
            /// Checks if the category was found.
            /// </summary>
            if (result != null)
            {
                /// <summary>
                /// Returns successful response with the retrieved category.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when the category does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new category.
        /// </summary>
        /// <param name="category">The <see cref="CategoryCreateUpdateModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Category.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(CategoryCreateUpdateModel category)
        {
            /// <summary>
            /// Sets the entity ID from the current user's context.
            /// </summary>
            /// 
            category.TenantId = User.GetTenantId();

            /// <summary>
            /// Gets the current user's name for audit tracking.
            /// </summary>
            string? UserName = User.GetUserName();

            /// <summary>
            /// Sets the created and updated by fields to the current user.
            /// </summary>
            category.CreatedBy = UserName;
            category.UpdatedBy = UserName;

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
            /// Calls the service to add a new category.
            /// </summary>
            await _categoryService.Add(category);

            /// <summary>
            /// Returns successful response indicating the category was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="category">The <see cref="CategoryUpdateModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Category.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(CategoryUpdateModel category)
        {
            /// <summary>
            /// Gets the current user's name for audit tracking.
            /// </summary>
            string UserName = User.GetUserName() ;

            /// <summary>
            /// Sets the updated by field to the current user.
            /// </summary>
            category.UpdatedBy = UserName;

            /// <summary>
            /// Sets the entity ID from the current user's context.
            /// </summary>
            category.TenantId = User.GetTenantId();

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
            /// Calls the service to update an existing category.
            /// </summary>
            await _categoryService.Update(category);

            /// <summary>
            /// Returns successful response indicating the category was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a category by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the category to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Category.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete a category for the current user's entity.
            /// </summary>
            var message = await _categoryService.Remove(User.GetTenantId(), id);

            /// <summary>
            /// Returns successful response indicating the category was deleted.
            /// </summary>
            if (message == "Deleted Successfully")
            {

                return Ok(new ResponseModel { IsSuccess = true, Message = message });
            }
            else
            { return Ok(new ResponseModel { IsSuccess = false, Message = message }); }


        }



        /// <summary>
        /// Deletes multiple categories by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the categories to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Category.Delete)]
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
            /// Calls the service to delete multiple categories for the current user's entity.
            /// </summary>
            var message = await _categoryService.RemoveMultiple(User.GetTenantId(), ids);

            /// <summary>
            /// Returns successful response indicating the categories were deleted.
            /// </summary>

            return Ok(new ResponseModel { IsSuccess = true, Message = message });
        }

        /// <summary>
        /// Imports categories from a file for the specified entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Category.Import)]

        [HttpPost("import")]
        public async Task<IActionResult> ImportCategory(IFormFile file)
        {
            /// <summary>
            /// Validates that a file was uploaded and has content.
            /// </summary>
            /// 
            var userName = User.GetUserName();
            var tenantId = User.GetTenantId();

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
                /// Calls the service to import categories from the file.
                /// </summary>
                string resultMessage = await _categoryService.ImportCategory(tenantId, file.OpenReadStream(), userName ?? "");

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
        /// Exports selected categories for the current entity.
        /// </summary>
        /// <param name="selectedCategoryIds">The list of selected category IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [Authorize(Policy = Permissions.Category.Export)]

        [HttpPost("export")]
        public async Task<IActionResult> ExportCategory([FromBody] List<int> selectedCategoryIds)
        {
            /// <summary>
            /// Calls the service to export categories for the current user's entity.
            /// </summary>
            var stream = await _categoryService.ExportCategory(User.GetTenantId(), selectedCategoryIds);

            /// <summary>
            /// Returns the exported file as a downloadable Excel document.
            /// </summary>
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "entities.xlsx");
        }

        /// <summary>
        /// Downloads the category import template.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            /// <summary>
            /// Calls the service to download the category import template.
            /// </summary>
            byte[] excelBytes = await _categoryService.DownloadTemplate();

            /// <summary>
            /// Returns the template file as a downloadable Excel document.
            /// </summary>
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template.xlsx");
        }
    }
}
