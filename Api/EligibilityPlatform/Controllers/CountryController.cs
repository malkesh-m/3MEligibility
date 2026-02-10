using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing country operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CountryController"/> class.
    /// </remarks>
    /// <param name="countryService">The country service.</param>
    [Route("api/country")]
    [ApiController]
    public class CountryController(ICountryService countryService) : ControllerBase
    {
        /// <summary>
        /// The country service instance for country operations.
        /// </summary>
        private readonly ICountryService _countryService = countryService;

        /// <summary>
        /// Retrieves all country records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="CountryModel"/> objects.</returns>
        [Authorize(Policy = Permissions.Country.View)]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            /// <summary>
            /// Retrieves all country records from the service.
            /// </summary>
            List<CountryModel> result = _countryService.GetAll();

            /// <summary>
            /// Returns successful response with the retrieved country records.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a country record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the country.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="CountryModel"/> if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.Country.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /// <summary>
            /// Retrieves a specific country record by ID from the service.
            /// </summary>
            var result = _countryService.GetById(id);

            /// <summary>
            /// Checks if the country record was found.
            /// </summary>
            if (result != null)
            {
                /// <summary>
                /// Returns successful response with the retrieved country record.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when the country record does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new country record.
        /// </summary>
        /// <param name="city">The <see cref="CountryModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Country.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(CountryModel city)
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
            /// Calls the service to add a new country record.
            /// </summary>
            await _countryService.Add(city);

            /// <summary>
            /// Returns successful response indicating the country record was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing country record.
        /// </summary>
        /// <param name="city">The <see cref="CountryModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Country.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(CountryModel city)
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
            /// Calls the service to update an existing country record.
            /// </summary>
            await _countryService.Update(city);

            /// <summary>
            /// Returns successful response indicating the country record was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a country record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the country to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Country.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete a country record.
            /// </summary>
            await _countryService.Delete(id);

            /// <summary>
            /// Returns successful response indicating the country record was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple country records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the countries to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.Country.Delete)]

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
            /// Calls the service to delete multiple country records.
            /// </summary>
            await _countryService.MultipleDelete(ids);

            /// <summary>
            /// Returns successful response indicating the country records were deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
