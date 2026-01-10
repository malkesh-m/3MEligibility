using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
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
        [RequireRole("View Max Percentage")]

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            // Retrieves all product cap records
            var result = _productCapService.GetAll();
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result });
        }

        /// <summary>
        /// Retrieves a product cap record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product cap record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the record if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Max Percentage")]

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            // Retrieves a product cap record by ID
            var result = _productCapService.GetById(id);
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
        [RequireRole("View Max Percentage")]

        [HttpGet("getbyproductid/{id}")]
        public async Task<IActionResult> GetByProductId(int id)
        {
            // Retrieves product cap records by product ID
            var result = await _productCapService.GetByProductId(id);
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
        [RequireRole("Add Max Percentage")]

        [HttpPost]
        public async Task<IActionResult> Add(ProductCapModel model)
        {
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
        [RequireRole("Edit Max Percentage")]

        [HttpPut]
        public async Task<IActionResult> Update(ProductCapModel model)
        {
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
        [RequireRole("Delete Max Percentage")]

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
