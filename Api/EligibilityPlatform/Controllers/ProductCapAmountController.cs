using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Constants;
using EligibilityPlatform.Application.Services;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing product cap amount operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductCapAmountController"/> class.
    /// </remarks>
    /// <param name="productCapAmountService">The product cap amount service.</param>
    [Route("api/productcapamount")]
    [ApiController]
    public class ProductCapAmountController(IProductCapAmountService productCapAmountService) : ControllerBase
    {
        private readonly IProductCapAmountService _productCapAmountService = productCapAmountService;

        /// <summary>
        /// Adds a new product cap amount record.
        /// </summary>
        /// <param name="model">The <see cref="ProductCapAmountAddModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCapAmount.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(ProductCapAmountAddModel model)
        {
            // Adds a new product cap amount record
            await _productCapAmountService.Add(model);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Retrieves all product cap amount records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of product cap amount records.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCapAmount.View)]

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            // Retrieves all product cap amount records
            var result = _productCapAmountService.GetAll();
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result });
        }

        /// <summary>
        /// Updates an existing product cap amount record.
        /// </summary>
        /// <param name="model">The <see cref="ProductCapAmountUpdateModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCapAmount.Edit)]

        [HttpPut]
        public async Task<IActionResult> Update(ProductCapAmountUpdateModel model)
        {
            // Updates an existing product cap amount record
            await _productCapAmountService.Update(model);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a product cap amount record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product cap amount record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCapAmount.Delete)]

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes a product cap amount record by ID
            await _productCapAmountService.Delete(id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Retrieves a product cap amount record by product ID.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the product cap amount record if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.ProductCapAmount.View)]

        [HttpGet("getbyproductid/{id}")]
        public async Task<IActionResult> GetByProductId(int id)
        {
            // Retrieves a product cap amount record by product ID
            var result = await _productCapAmountService.GetByProductId(id);
            // Checks if the record was found
            if (result != null)
            {
                // Returns success response with the retrieved data
                return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when record doesn't exist
                return NotFound(new ResponseModel { IsSuccess = false, Message = GlobalcConstants.NotFound });
            }
        }
    }
}
