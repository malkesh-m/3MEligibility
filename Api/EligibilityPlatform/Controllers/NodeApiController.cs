using MEligibilityPlatform.Application.Attributes;
using MEligibilityPlatform.Application.Constants;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing node API operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="NodeApiController"/> class.
    /// </remarks>
    /// <param name="nodeApiService">The node API service.</param>
    [Route("api/nodeapi")]
    [ApiController]
    public class NodeApiController(INodeApiService nodeApiService) : ControllerBase
    {
        private readonly INodeApiService _nodeApiService = nodeApiService;

        /// <summary>
        /// Retrieves all node API records.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="NodeApiListModel"/> objects.</returns>
        /// 
        [Authorize(Policy = Permissions.NodeApi.View)]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            var tenantId = User.GetTenantId();
            // Retrieves all node API records
            List<NodeApiListModel> result = _nodeApiService.GetAll(tenantId);
            // Returns success response with the retrieved data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }
        [Authorize(Policy = Permissions.NodeApi.Edit)]

        [HttpPut("updatestatus")]
        public async Task<IActionResult> UpdateStatusAsync(int Apiid, bool Isactive)
        {
            await _nodeApiService.UpdateStatus(Apiid, Isactive);

            return Ok(new ResponseModel { IsSuccess = true });
        }

        /// <summary>
        /// Retrieves a node API record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the node API record.</param>
        /// <returns>An <see cref="IActionResult"/> containing the <see cref="NodeApiListModel"/> if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.NodeApi.View)]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var tenantId = User.GetTenantId();
            // Retrieves a node API record by ID
            var result = _nodeApiService.GetById(id,tenantId);
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
        /// Adds a new node API record.
        /// </summary>
        /// <param name="nodeApi">The <see cref="NodeApiCreateOrUpdateModel"/> to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.NodeApi.Create)]

        [HttpPost]
        public async Task<IActionResult> Post(NodeApiCreateOrUpdateModel nodeApi)
        {
            var tenantId = User.GetTenantId();
            var userName = User.GetUserName();
            nodeApi.TenantId = tenantId;
            nodeApi.CreatedBy = userName;
            nodeApi.UpdatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new node API record
            await _nodeApiService.Add(nodeApi);
            // Returns success response for created operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing node API record.
        /// </summary>
        /// <param name="nodeApi">The <see cref="NodeApiCreateOrUpdateModel"/> to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.NodeApi.Edit)]
        [HttpPut]
        public async Task<IActionResult> Put(NodeApiCreateOrUpdateModel nodeApi)
        {
            var tenantId = User.GetTenantId();
            nodeApi.TenantId = tenantId;
            var userName = User.GetUserName();
            nodeApi.UpdatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            //var Token =User.Identit
            // Updates the existing node API record
            await _nodeApiService.Update(nodeApi);
            // Returns success response for updated operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a node API record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the node API record to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.NodeApi.Delete)]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the node API record by ID
            await _nodeApiService.Delete(id);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Deletes multiple node API records by their unique identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the node API records to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [Authorize(Policy = Permissions.NodeApi.Delete)]

        [HttpDelete("multipledelete")]
        public async Task<IActionResult> MultipleDelete(List<int> ids)
        {
            // Validates if IDs are provided
            if (ids.Count == 0 || ids == null)
            {
                // Returns bad request if no IDs are provided
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No id's provided" });
            }
            // Deletes multiple node API records
            await _nodeApiService.MultipleDelete(ids);
            // Returns success response for deleted operation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }

        /// <summary>
        /// Retrieves the binary XML by ID and node ID.
        /// </summary>
        /// <param name="id">The ID of the binary XML.</param>
        /// <param name="nodeid">The node ID.</param>
        /// <returns>An <see cref="IActionResult"/> containing the binary XML data if found; otherwise, not found.</returns>
        /// 
        [Authorize(Policy = Permissions.NodeApi.View)]

        [HttpGet("getbinaryxmlbyid")]
        public IActionResult GetBinaryXmlbyId(int id, int nodeid)
        {
            // Retrieves binary XML data by ID and node ID
            var result = _nodeApiService.GetBinaryXmlById(id, nodeid);
            // Checks if binary XML data was found
            if (string.IsNullOrEmpty(result))
            {
                // Returns bad request if binary XML data is not found
                return BadRequest(new ResponseModel { IsSuccess = false, Message = "Binary XML data not found." });
            }
            // Returns success response with the retrieved binary XML data
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }
    }
}
