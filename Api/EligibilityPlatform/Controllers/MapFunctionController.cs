using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing mapping function operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MapFunctionController"/> class.
    /// </remarks>
    /// <param name="mappingfunctionService">The mapping function service.</param>
    [Route("api/mappingfunction")]
    [ApiController]
    [Authorize]
    public class MapFunctionController(IMappingfunctionService mappingfunctionService) : ControllerBase
    {
        private readonly IMappingfunctionService _mappingfunctionService = mappingfunctionService;

        /// <summary>
        /// Retrieves all mapping function records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of mapping function records.</returns>
        [Authorize(Policy = Permissions.MapFunction.View)]
        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all mapping function records
            List<MappingFunctionModel> result = _mappingfunctionService.GetAll();
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a mapping function record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the mapping function record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the record if found; otherwise, not found.</returns>
        [Authorize(Policy = Permissions.MapFunction.View)]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a mapping function record by ID
            var result = _mappingfunctionService.GetById(id);
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
        /// Adds a new mapping function record.
        /// </summary>
        /// <param name="mappingFunction">The mapping function model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.MapFunction.Create)]
        [HttpPost]
        public async Task<IActionResult> Post(MappingFunctionModel mappingFunction)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new mapping function record
            await _mappingfunctionService.Add(mappingFunction);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing mapping function record.
        /// </summary>
        /// <param name="mappingFunction">The mapping function model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.MapFunction.Edit)]
        [HttpPut]
        public async Task<IActionResult> Put(MappingFunctionModel mappingFunction)
        {
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing mapping function record
            await _mappingfunctionService.Update(mappingFunction);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a mapping function record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the mapping function record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.MapFunction.Delete)]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the mapping function record by ID
            await _mappingfunctionService.Delete(id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple mapping function records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the mapping function records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Policy = Permissions.MapFunction.Delete)]
        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete([FromBody] List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple mapping function records
            await _mappingfunctionService.MultipleDelete(ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}