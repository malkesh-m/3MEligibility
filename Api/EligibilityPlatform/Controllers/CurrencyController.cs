using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing currency operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CurrencyController"/> class.
    /// </remarks>
    /// <param name="currencyService">The currency service.</param>
    [Route("api/currency")]
    [ApiController]
    public class CurrencyController(ICurrencyService currencyService) : ControllerBase
    {
        /// <summary>
        /// The currency service instance for currency operations.
        /// </summary>
        private readonly ICurrencyService _currencyService = currencyService;

        /// <summary>
        /// Retrieves all currency records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="CurrencyModel"/> objects.</returns>
        [HttpGet("getall")]
        public IActionResult Get()
        {
            /// <summary>
            /// Retrieves all currency records from the service.
            /// </summary>
            List<CurrencyModel> result = _currencyService.GetAll();

            /// <summary>
            /// Returns successful response with the retrieved currency records.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a currency record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the currency.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="CurrencyModel"/> if found; otherwise, not found.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /// <summary>
            /// Retrieves a specific currency record by ID from the service.
            /// </summary>
            var result = _currencyService.GetById(id);

            /// <summary>
            /// Checks if the currency record was found.
            /// </summary>
            if (result != null)
            {
                /// <summary>
                /// Returns successful response with the retrieved currency record.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when the currency record does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new currency record.
        /// </summary>
        /// <param name="currency">The <see cref="CurrencyModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Post(CurrencyModel currency)
        {
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
            /// Calls the service to add a new currency record.
            /// </summary>
            await _currencyService.Add(currency);

            /// <summary>
            /// Returns successful response indicating the currency record was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing currency record.
        /// </summary>
        /// <param name="currency">The <see cref="CurrencyModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPut]
        public async Task<IActionResult> Put(CurrencyModel currency)
        {
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
            /// Calls the service to update an existing currency record.
            /// </summary>
            await _currencyService.Update(currency);

            /// <summary>
            /// Returns successful response indicating the currency record was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a currency record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the currency to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete a currency record.
            /// </summary>
            await _currencyService.Delete(id);

            /// <summary>
            /// Returns successful response indicating the currency record was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple currency records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the currencies to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            /// <summary>
            /// Validates that IDs were provided in the request body.
            /// </summary>
            if (ids.Count == 0 || ids == null)
            {
                /// <summary>
                /// Returns bad request response when no IDs are provided.
                /// </summary>
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }

            /// <summary>
            /// Calls the service to delete multiple currency records.
            /// </summary>
            await _currencyService.MultipleDelete(ids);

            /// <summary>
            /// Returns successful response indicating the currency records were deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
