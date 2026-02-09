using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing product cap operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductCapController"/> class.
    /// </remarks>
    /// <param name="productCapService">The product cap service.</param>
    [Route("api/productcap")]
    [ApiController]
    public class ProductCapController(IProductCapService productCapService) : ControllerBase
    {
        private readonly IProductCapService _productCapService = productCapService;

        /// <summary>
        /// Retrieves all product cap records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing all product cap records.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCap.View)]

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var tenantId = User.GetTenantId();
            // Retrieves all product cap records
            var result = _productCapService.GetAll(tenantId);
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result });
        }

        /// <summary>
        /// Retrieves a product cap record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product cap record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the record if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCap.View)]

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var tenantId = User.GetTenantId();
            // Retrieves a product cap record by ID
            var result = _productCapService.GetById(id,tenantId);
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
        /// Retrieves product cap records by product ID.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the product cap records if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCap.View)]

        [HttpGet("getbyproductid/{id}")]
        public async Task<IActionResult> GetByProductId(int id)
        {
            var tenantId = User.GetTenantId();

            // Retrieves product cap records by product ID
            var result = await _productCapService.GetByProductId(id,tenantId);
            // Checks if records were found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when no records exist for the product
                return NotFound(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new product cap record.
        /// </summary>
        /// <param name="model">The product cap model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCap.Create)]

        [HttpPost]
        public async Task<IActionResult> Add(ProductCapModel model)
        {
            var tenantId = User.GetTenantId();
            model.TenantId = tenantId;
            // Adds a new product cap record
            await _productCapService.Add(model);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing product cap record.
        /// </summary>
        /// <param name="model">The product cap model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCap.Edit)]

        [HttpPut]
        public async Task<IActionResult> Update(ProductCapModel model)
        {
            var tenantId = User.GetTenantId();
            model.TenantId = tenantId;
            // Updates an existing product cap record
            await _productCapService.Update(model);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a product cap record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product cap record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.ProductCap.Delete)]

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes a product cap record by ID
            await _productCapService.Delete(id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
