using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing node operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="NodeController"/> class.
    /// </remarks>
    /// <param name="nodeService">The node service.</param>
    [Route("api/node")]
    [ApiController]
    public class NodeController(INodeService nodeService) : ControllerBase
    {
        private readonly INodeService _nodeService = nodeService;

        /// <summary>
        /// Retrieves all node records for the current entity.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="NodeListModel"/> objects.</returns>
        /// 
        [RequireRole("View Integration screen")]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all node records for the current entity
            List<NodeListModel> result = _nodeService.GetAll(User.GetEntityId());
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a node record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the node record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="NodeListModel"/> if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Integration screen")]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a node record by ID for the current entity
            var result = _nodeService.GetById(User.GetEntityId(), id);
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
        /// Adds a new node record for the current entity.
        /// </summary>
        /// <param name="node">The node model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Add new Node")]

        [HttpPost]
        public async Task<IActionResult> Post(NodeCreateUpdateModel node)
        {
            // Sets entity ID from the current user
            node.EntityId = User.GetEntityId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new node record
            await _nodeService.Add(node);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing node record for the current entity.
        /// </summary>
        /// <param name="node">The node model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Node")]

        [HttpPut]
        public async Task<IActionResult> Put(NodeCreateUpdateModel node)
        {
            // Sets entity ID from the current user
            node.EntityId = User.GetEntityId();
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing node record
            await _nodeService.Update(node);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a node record by its unique identifier for the current entity.
        /// </summary>
        /// <param name="id">The unique identifier of the node record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Node")]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the node record by ID for the current entity
            await _nodeService.Delete(User.GetEntityId(), id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple node records by their unique identifiers for the current entity.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the node records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Node")]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple node records
            await _nodeService.MultipleDelete(User.GetEntityId(), ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
