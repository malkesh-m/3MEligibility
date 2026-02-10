using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing city operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CityController"/> class.
    /// </remarks>
    /// <param name="cityService">The city service.</param>
    [Route("api/city")]
    [ApiController]
    public class CityController(ICityService cityService) : ControllerBase
    {
        /// <summary>
        /// The city service instance for city operations.
        /// </summary>
        private readonly ICityService _cityService = cityService;

        /// <summary>
        /// Retrieves all city records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="CityModel"/> objects.</returns>
        [Authorize(Policy = Permissions.City.View)]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            /// <summary>
            /// Retrieves all city records from the service.
            /// </summary>
            List<CityModel> result = _cityService.GetAll();

            /// <summary>
            /// Returns successful response with the retrieved city records.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a city record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the city.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="CityModel"/> if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.City.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /// <summary>
            /// Retrieves a specific city record by ID from the service.
            /// </summary>
            var result = _cityService.GetById(id);

            /// <summary>
            /// Checks if the city record was found.
            /// </summary>
            if (result != null)
            {
                /// <summary>
                /// Returns successful response with the retrieved city record.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when the city record does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new city record.
        /// </summary>
        /// <param name="city">The <see cref="CityModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.City.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(CityModel city)
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
            /// Calls the service to add a new city record.
            /// </summary>
            await _cityService.Add(city);

            /// <summary>
            /// Returns successful response indicating the city record was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing city record.
        /// </summary>
        /// <param name="city">The <see cref="CityModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.City.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(CityModel city)
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
            /// Calls the service to update an existing city record.
            /// </summary>
            await _cityService.Update(city);

            /// <summary>
            /// Returns successful response indicating the city record was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a city record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the city to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.City.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete a city record.
            /// </summary>
            await _cityService.Delete(id);

            /// <summary>
            /// Returns successful response indicating the city record was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple city records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the cities to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.City.Delete)]

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
            /// Calls the service to delete multiple city records.
            /// </summary>
            await _cityService.DeleteMultiple(ids);

            /// <summary>
            /// Returns successful response indicating the city records were deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        [AllowAnonymous]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("City Controller is working!");
        }

        [AllowAnonymous]
        [HttpGet("testexception")]
        public IActionResult TestException()
        {
            throw new Exception("This is a test exception from City Controller.");
        }

    }
}
