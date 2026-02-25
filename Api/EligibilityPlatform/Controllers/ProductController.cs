using System.Net.Http.Headers;
using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
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
    public class ProductController(IProductService productService,IDriveService driveService) : ControllerBase
    {
        private readonly IProductService _productService = productService;
        private readonly IDriveService _driveService = driveService;
        /// <summary>
        /// Retrieves all product records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ProductListModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.Product.View)]

        [HttpGet("getall")]
        public async Task< IActionResult> Get()
        {
            // Retrieves the entity ID from the current user context
            int tenantId = Convert.ToInt32(User.GetTenantId());
            // Retrieves all product records for the current entity
            List<ProductListModel> result = await _productService.GetAll(tenantId);
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves all product IDs and names for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ProductIdAndNameModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.Product.View)]


        [HttpGet("getallidandhname")]
        public IActionResult GetAllIdAndName()
        {
            // Retrieves all product IDs and names for the current entity
            List<ProductIdAndNameModel> result = _productService.GetProductIAndName(User.GetTenantId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves all product names for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="ProductEligibleModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.Product.View)]
        [HttpGet("getallname")]
        public IActionResult GetAllName()
        {
            // Retrieves all product names for the current entity
            List<ProductEligibleModel> result = _productService.GetProductName(User.GetTenantId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves products by category for the current entity.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the products if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.Product.View)]

        [HttpGet("getproductsbycategory/{id}")]
        public IActionResult GetProductsByCategory(int id)
        {
            // Retrieves products by category for the current entity
            var result = _productService.GetProductsByCategory(User.GetTenantId(), id);
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
        [Authorize(Policy = Permissions.Product.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a product record by ID for the current entity
            var result = _productService.GetById(User.GetTenantId(), id);
            // Checks if the product record was found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
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
        [Authorize(Policy = Permissions.Product.Create)]

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] ProductAddUpdateModel product)
        {
            // Sets user information and entity ID from the current user context
            String? UserName = User.GetUserName();
            product.CreatedBy = UserName;
            product.UpdatedBy = UserName;
            product.TenantId = User.GetTenantId();

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }

            var token = Request.Headers.Authorization.ToString();

            //var token = AuthenticationHeaderValue
            //    .Parse(authHeader)
            //    .Parameter;
            // Adds the new product record
            await _productService.Add(product, token??"");
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing product record for the current entity.
        /// </summary>
        /// <param name="product">The product model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Product.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put([FromForm] ProductAddUpdateModel product)
        {
            // Sets user information and entity ID from the current user context
            String? UserName = User.GetUserName();
            product.UpdatedBy = UserName;
            product.TenantId = User.GetTenantId();

            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            var token = Request.Headers.Authorization.ToString();

            // Updates the existing product record
            await _productService.Update(product,token);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a product record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the product record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.Product.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var token = Request.Headers.Authorization.ToString();
            // Deletes a product record by ID for the current entity
            await _productService.Delete(User.GetTenantId(), id, token);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple product records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the product records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Product.Delete)]
        [HttpDelete("multipleDelete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            var token = Request.Headers.Authorization.ToString();
            // Validates that IDs are provided
            if (ids == null || ids.Count == 0)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
            }

            // Deletes multiple product records for the current entity
            await _productService.RemoveMultiple(User.GetTenantId(), ids, token);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Exports selected products for the current entity.
        /// </summary>
        /// <param name="selectedProductIds">The list of selected product IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        /// 
        [Authorize(Policy = Permissions.Product.Export)]
        [HttpPost("export")]
        public async Task<IActionResult> ExportInfo([FromBody] ExportRequestModel request)
        {
            // Exports product records for the current entity based on selection or filters
            var stream = await _productService.ExportInfo(User.GetTenantId(), request);
            
            if (stream == null || stream.Length == 0)
                return Ok(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NoRecordsToExport });

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
        [Authorize(Policy = Permissions.Product.Import)]

        [HttpPost("import")]
        public async Task<IActionResult> ImportInfo(IFormFile file)
        {
            var userName = User.GetUserName();

            // Validates that a file is provided
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });

            try
            {
                // Imports product records from the provided file
                string resultMessage = await _productService.ImportInfo(User.GetTenantId(), file.OpenReadStream(), userName ?? "");
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
        /// Downloads the product import template for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Downloads the product import template for the current entity
            byte[] excelBytes = await _productService.DownloadTemplate(User.GetTenantId());
            // Returns the template file as a downloadable Excel document
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template.xlsx");
        }

        /// <summary>
        /// Serves a product image file from the file system.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filename">The filename of the image.</param>
        /// <returns>An <see cref="IActionResult"/> containing the image file.</returns>
        [AllowAnonymous]
        [HttpGet("image/{tenantId}/{filename}")]
        public IActionResult GetImage(int tenantId, string filename)
        {
            // Constructs the relative file path
            var relativePath = $"files/{tenantId}/{filename}";
            
            // Gets the full file path
            var filePath = _productService.GetImagePath(relativePath);
            
            // Checks if file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new ResponseModel { IsSuccess = false, Message = "Image not found." });
            }
            
            // Determines content type based on file extension
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
            
            // Returns the image file
            return PhysicalFile(filePath, contentType);
        }

        [Authorize]
        [HttpGet("drive-image/{fileId}")]
        public async Task<IActionResult> GetDriveImage(int fileId)
        {
            var token = Request.Headers.Authorization.ToString();

            try
            {
                // result.Bytes will be used to stream the file if found.
                var (Bytes, ContentType) = await _productService.DownloadImageAsync(fileId, token);
                if (Bytes == null || Bytes.Length == 0)
                {
                    return NotFound(new ResponseModel { IsSuccess = false, Message = "Image not found in drive." });
                }

                return File(Bytes, ContentType); 
            }
            catch (Exception ex)
            {
                // Returning full exception message to help the user identify if MDrive is rejecting the token or URL
                return BadRequest(new ResponseModel { 
                    IsSuccess = false, 
                    Message = $"Error fetching image from drive: {ex.Message}" 
                });
            }
        }
        [Authorize]
        [HttpPost("upload-drive-image")]
        public async Task<IActionResult> UploadDriveImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });

            var token = Request.Headers.Authorization.ToString();
            try
            {
                var response = await _driveService.UploadAsync(file, token);
                return Ok(new ResponseModel { IsSuccess = true, Data = response.Data, Message = GlobalcConstants.Created });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { IsSuccess = false, Message = $"Upload failed: {ex.Message}" });
            }
        }
        [Authorize]
        [HttpDelete("drive-image/{fileId}")]
        public async Task<IActionResult> DeleteDriveImage(int fileId)
        {
            var token = Request.Headers.Authorization.ToString();
            try
            {
                 await _driveService.DeleteAsync(fileId, token);
                return Ok(new ResponseModel { IsSuccess = true, Message = "Image deleted from drive." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { IsSuccess = false, Message = $"Delete failed: {ex.Message}" });
            }
        }
    }
}
