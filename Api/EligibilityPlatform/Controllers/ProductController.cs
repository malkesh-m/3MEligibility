using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing product operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductController"/> class.
    /// </remarks>
    /// <param name="productService">The product service.</param>
    [Route("api/product")]
    [ApiController]
    public class ProductController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;

        /// <summary>
        /// Retrieves all product records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ProductListModel"/> objects.</returns>
        /// 
        [RequireRole("View Products Screen")]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves the entity ID from the current user context
            int entityId = Convert.ToInt32(User.GetEntityId());
            // Retrieves all product records for the current entity
            List<ProductListModel> result = _productService.GetAll(entityId);
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves all product IDs and names for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ProductIdAndNameModel"/> objects.</returns>
        /// 
        [RequireRole("View Products Screen")]

        [HttpGet("getallidandhname")]
        public IActionResult GetAllIdAndName()
        {
            // Retrieves all product IDs and names for the current entity
            List<ProductIdAndNameModel> result = _productService.GetProductIAndName(User.GetEntityId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves all product names for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ProductEligibleModel"/> objects.</returns>
        /// 
        [RequireRole("View Products Screen")]

        [HttpGet("getallname")]
        public IActionResult GetAllName()
        {
            // Retrieves all product names for the current entity
            List<ProductEligibleModel> result = _productService.GetProductName(User.GetEntityId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves products by category for the current entity.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the products if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Products Screen")]

        [HttpGet("getproductsbycategory/{id}")]
        public IActionResult GetProductsByCategory(int id)
        {
            // Retrieves products by category for the current entity
            var result = _productService.GetProductsByCategory(User.GetEntityId(), id);
            // Checks if any products were found for the category
            if (result.Count != 0)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when no products exist for the category
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Retrieves a product record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the product record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="ProductListModel"/> if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Products Screen")]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a product record by ID for the current entity
            var result = _productService.GetById(User.GetEntityId(), id);
            // Checks if the product record was found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when product record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new product record for the current entity.
        /// </summary>
        /// <param name="product">The product model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Add new Info")]

        [HttpPost]
        public async Task<IActionResult> Post(ProductAddUpdateModel product)
        {
            // Sets user information and entity ID from the current user context
            String? UserName = User.Identity?.Name;
            product.CreatedBy = UserName;
            product.UpdatedBy = UserName;
            product.EntityId = User.GetEntityId();

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }

            // Adds the new product record
            await _productService.Add(product);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing product record for the current entity.
        /// </summary>
        /// <param name="product">The product model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Info")]

        [HttpPut]
        public async Task<IActionResult> Put(ProductAddUpdateModel product)
        {
            // Sets user information and entity ID from the current user context
            String? UserName = User.Identity?.Name;
            product.UpdatedBy = UserName;
            product.EntityId = User.GetEntityId();

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }

            // Updates the existing product record
            await _productService.Update(product);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a product record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the product record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Info")]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes a product record by ID for the current entity
            await _productService.Delete(User.GetEntityId(), id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Exports selected products for the current entity.
        /// </summary>
        /// <param name="selectedProductIds">The list of selected product IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [RequireRole("Export Info")]

        [HttpPost("export")]
        public async Task<IActionResult> ExportInfo([FromBody] List<int> selectedProductIds)
        {
            // Exports selected product records for the current entity
            var stream = await _productService.ExportInfo(User.GetEntityId(), selectedProductIds);
            // Returns the exported file as a downloadable Excel document
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Product.xlsx");
        }

        /// <summary>
        /// Imports products from a file for the current entity.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        /// 
        [RequireRole("Import Info")]

        [HttpPost("import")]
        public async Task<IActionResult> ImportInfo(IFormFile file)
        {
            var userName = User.Identity!.Name;

            // Validates that a file is provided
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });

            try
            {
                // Imports product records from the provided file
                string resultMessage = await _productService.ImportInfo(User.GetEntityId(), file.OpenReadStream(), userName ?? "");
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
        /// Deletes multiple product records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the product records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Info")]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            // Validates that IDs are provided
            if (ids == null || ids.Count == 0)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
            }

            // Deletes multiple product records for the current entity
            await _productService.RemoveMultiple(User.GetEntityId(), ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Downloads the product import template for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Downloads the product import template for the current entity
            byte[] excelBytes = await _productService.DownloadTemplate(User.GetEntityId());
            // Returns the template file as a downloadable Excel document
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template.xlsx");
        }
    }
}
