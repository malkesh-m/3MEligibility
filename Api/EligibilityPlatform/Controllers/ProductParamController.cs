using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing product parameter operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductParamController"/> class.
    /// </remarks>
    /// <param name="productParamservice">The product parameter service.</param>
    /// <param name="productservice">The product service.</param>
    /// <param name="parameterservice">The parameter service.</param>
    [Route("api/productparam")]
    [ApiController]
    public class ProductParamController(IProductParamservice productParamservice, IProductService productservice, IParameterService parameterservice) : ControllerBase
    {
        private readonly IProductParamservice _productParamservice = productParamservice;
        private readonly IProductService _userProductService = productservice;
        private readonly IParameterService _ParameterService = parameterservice;

        /// <summary>
        /// Retrieves all product parameters for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of product parameters.</returns>
        [HttpGet("getall")]
        public async Task<IActionResult> Get()
        {
            // Retrieves all product parameters for the current entity
            return Ok(new ResponseModel { IsSuccess = true, Data = await _productParamservice.GetAll(User.GetTenantId()), Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Adds a new product parameter for the current entity.
        /// </summary>
        /// <param name="product">The product parameter model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Post(ProductParamAddUpdateModel product)
        {
            // Sets the entity ID from the current user context
            product.TenantId = User.GetTenantId();
            // Gets the username from the current user identity
            string? UserName = User.GetUserName();
            // Sets the created by field with the current username
            product.CreatedBy = UserName;
            // Sets the updated by field with the current username
            product.UpdatedBy = UserName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new product parameter
            await _productParamservice.Add(product);
            // Returns success response after creation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Retrieves a product parameter by product and parameter ID for the current entity.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="parameterId">The parameter ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the product parameter if found; otherwise, not found.</returns>
        [HttpGet("getbyid")]
        public IActionResult Get(int productId, int parameterId)
        {
            // Retrieves product parameter by product ID and parameter ID for the current entity
            var result = _productParamservice.GetById(productId, parameterId, User.GetTenantId());
            // Checks if the product parameter was found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when product parameter doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Retrieves a product parameter by product ID for the current entity.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the product parameter if found; otherwise, not found.</returns>
        [HttpGet("getbyproductid")]
        public IActionResult Get(int productId)
        {
            // Retrieves product parameters by product ID for the current entity
            var result = _productParamservice.GetByProductId(productId, User.GetTenantId());
            // Checks if any product parameters were found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when no product parameters exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Updates an existing product parameter for the current entity.
        /// </summary>
        /// <param name="product">The product parameter model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPut]
        public async Task<IActionResult> Put(ProductParamAddUpdateModel product)
        {
            // Sets the entity ID from the current user context
            product.TenantId = User.GetTenantId();
            // Gets the username from the current user identity
            string? UserName = User.GetUserName();
            // Sets the created by field with the current username
            product.CreatedBy = UserName;
            // Sets the updated by field with the current username
            product.UpdatedBy = UserName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing product parameter
            await _productParamservice.Update(product);
            // Returns success response after update
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a product parameter by product and parameter ID for the current entity.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="parameterId">The parameter ID.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int productId, int parameterId)
        {
            // Deletes the product parameter by product ID and parameter ID
            await _productParamservice.Delete(productId, parameterId, User.GetTenantId());
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple product parameters by their product and parameter IDs for the current entity.
        /// </summary>
        /// <param name="productIds">The list of product IDs.</param>
        /// <param name="parameterIds">The list of parameter IDs.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete([FromQuery] List<int> productIds, [FromQuery] List<int> parameterIds)
        {
            //TODO:add required validations

            // Deletes multiple product parameters by their IDs
            await _productParamservice.MultipleDelete(productIds, parameterIds, User.GetTenantId());
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Exports selected product parameters for the current entity.
        /// </summary>
        /// <param name="selectedProductIds">The list of selected product IDs to export.</param>
        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
        [HttpPost("export")]
        public async Task<IActionResult> ExportDetails([FromBody] List<int> selectedProductIds)
        {
            // Exports product parameters to an Excel stream
            var stream = await _productParamservice.ExportDetails(selectedProductIds, User.GetTenantId());
            // Returns the Excel file as a downloadable response
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Product.xlsx");
        }

        /// <summary>
        /// Downloads the product parameter import template for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            // Downloads the Excel template for product parameter import
            byte[] excelBytes = await _productParamservice.DownloadTemplate(User.GetTenantId());
            // Returns the Excel template as a downloadable response
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template.xlsx");
        }

        /// <summary>
        /// Imports product parameters from a file for the current entity.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <param name="createdBy">The creator of the import.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
        [HttpPost("import")]
        public async Task<IActionResult> ImportDetails(IFormFile file, string createdBy)
        {
            // Validates that a file was uploaded
            if (file == null || file.Length == 0)
                // Returns bad request if no file is uploaded
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });

            try
            {
                // Imports product parameters from the uploaded file
                string resultMessage = await _productParamservice.ImportDetails(file.OpenReadStream(), createdBy, User.GetTenantId());
                // Returns success response with import result message
                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
            }
            catch (Exception ex)
            {
                // Returns bad request if import fails with exception message
                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
            }
        }
    }
}
