using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing data type operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DataTypeController"/> class.
    /// </remarks>
    /// <param name="dataTypeService">The data type service.</param>
    [Route("api/datatype")]
    [ApiController]
    public class DataTypeController(IDataTypeService dataTypeService) : ControllerBase
    {
        /// <summary>
        /// The data type service instance for data type operations.
        /// </summary>
        private readonly IDataTypeService _dataTypeService = dataTypeService;

        /// <summary>
        /// Retrieves all data type records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="DataTypeModel"/> objects.</returns>
        [Authorize(Policy = Permissions.DataType.View)]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            /// <summary>
            /// Retrieves all data type records from the service.
            /// </summary>
            List<DataTypeModel> result = _dataTypeService.GetAll();

            /// <summary>
            /// Returns successful response with the retrieved data type records.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a data type record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the data type.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="DataTypeModel"/> if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.DataType.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /// <summary>
            /// Retrieves a specific data type record by ID from the service.
            /// </summary>
            var result = _dataTypeService.GetById(id);

            /// <summary>
            /// Checks if the data type record was found.
            /// </summary>
            if (result != null)
            {
                /// <summary>
                /// Returns successful response with the retrieved data type record.
                /// </summary>
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                /// <summary>
                /// Returns not found response when the data type record does not exist.
                /// </summary>
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new data type record.
        /// </summary>
        /// <param name="city">The <see cref="DataTypeModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.DataType.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(DataTypeModel city)
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
            /// Calls the service to add a new data type record.
            /// </summary>
            await _dataTypeService.Add(city);

            /// <summary>
            /// Returns successful response indicating the data type record was created.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing data type record.
        /// </summary>
        /// <param name="city">The <see cref="DataTypeModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.DataType.Edit)]

        [HttpPut]
        public async Task<IActionResult> Put(DataTypeModel city)
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
            /// Calls the service to update an existing data type record.
            /// </summary>
            await _dataTypeService.Update(city);

            /// <summary>
            /// Returns successful response indicating the data type record was updated.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a data type record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the data type to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.DataType.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /// <summary>
            /// Calls the service to delete a data type record.
            /// </summary>
            await _dataTypeService.Delete(id);

            /// <summary>
            /// Returns successful response indicating the data type record was deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple data type records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the data types to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.DataType.Delete)]

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
            /// Calls the service to delete multiple data type records.
            /// </summary>
            await _dataTypeService.MultipleDelete(ids);

            /// <summary>
            /// Returns successful response indicating the data type records were deleted.
            /// </summary>
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}

