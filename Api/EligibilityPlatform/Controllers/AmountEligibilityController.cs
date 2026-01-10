using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing amount eligibility operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AmountEligibilityController"/> class.
    /// </remarks>
    /// <param name="amountEligibilityService">The amount eligibility service.</param>
    [Route("api/amounteligibility")]
    [ApiController]
    public class AmountEligibilityController(IAmountEligibilityService amountEligibilityService) : ControllerBase
    {
        /// <summary>
        /// The amount eligibility service instance for business logic operations.
        /// </summary>
        private readonly IAmountEligibilityService _amountEligibilityService = amountEligibilityService;

        /// <summary>
        /// Retrieves all amount eligibility records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="AmountEligibilityModel"/> objects.</returns>
        [HttpGet("getall")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AmountEligibilityModel))]
        public IActionResult GetAll()
        {
            /// <summary>
            /// Retrieves all amount eligibility records from the service.
            /// </summary>
            var result = _amountEligibilityService.GetAll();

            /// <summary>
            /// Returns successful response with the retrieved data.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = result });
        }

        /// <summary>
        /// Retrieves an amount eligibility record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the amount eligibility record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="AmountEligibilityModel"/> if found; otherwise, a not found result.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AmountEligibilityModel))]
        public IActionResult GetById(int id)
        {
            /// <summary>
            /// Retrieves a specific amount eligibility record by ID from the service.
            /// </summary>
            var result = _amountEligibilityService.GetById(id);

            /// <summary>
            /// Checks if the record was found.
            /// </summary>
            if (result != null)
            {
                /// <summary>
                /// Returns successful response with the retrieved data.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when the record does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new amount eligibility record.
        /// </summary>
        /// <param name="model">The <see cref="AmountEligibilityModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Add(AmountEligibilityModel model)
        {
            /// <summary>
            /// Calls the service to add a new amount eligibility record.
            /// </summary>
            await _amountEligibilityService.Add(model);

            /// <summary>
            /// Returns successful response indicating the record was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing amount eligibility record.
        /// </summary>
        /// <param name="model">The <see cref="AmountEligibilityModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPut]
        public async Task<IActionResult> Update(AmountEligibilityModel model)
        {
            /// <summary>
            /// Calls the service to update an existing amount eligibility record.
            /// </summary>
            await _amountEligibilityService.Update(model);

            /// <summary>
            /// Returns successful response indicating the record was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes an amount eligibility record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the amount eligibility record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete an amount eligibility record.
            /// </summary>
            await _amountEligibilityService.Delete(id);

            /// <summary>
            /// Returns successful response indicating the record was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Calculates the eligible amount based on the provided pre-amount and pcard ID.
        /// </summary>
        /// <param name="preAmount">The pre-amount value as a string.</param>
        /// <param name="pcardId">The pcard identifier.</param>
        /// <returns>An <see cref="IActionResult"/> containing the calculated amount or a bad request if input is invalid.</returns>
        [HttpGet]
        public IActionResult CalculateAmount(string preAmount, int pcardId)
        {
            /// <summary>
            /// Validates the input parameters for the calculation.
            /// </summary>
            if (string.IsNullOrEmpty(preAmount) || pcardId <= 0)
            {
                /// <summary>
                /// Returns bad request response for invalid input parameters.
                /// </summary>
                return BadRequest("Invalid Input");
            }

            /// <summary>
            /// Calls the service to calculate the eligible amount.
            /// </summary>
            string result = _amountEligibilityService.AmountCalculate(User.GetEntityId(), preAmount, pcardId);

            /// <summary>
            /// Returns successful response with the calculated amount.
            /// </summary>
            return Ok(new { Amount = result });
        }
    }
}
