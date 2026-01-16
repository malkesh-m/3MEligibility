using EligibilityPlatform.Application.Attributes;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EligibilityPlatform.Controllers
{
    /// <summary>
    /// API controller for managing role operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RoleController"/> class.
    /// </remarks>
    /// <param name="roleService">The role service.</param>
    [Route("api/role")]
    [ApiController]
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        private readonly IRoleService _roleService = roleService;

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of roles.</returns>
        /// 
        [RequireRole("View Roles Screen")]

        [HttpGet("getall")]
        public IActionResult Get()
        {
            // Retrieves all roles from the role service
            List<RoleModel> result = _roleService.GetAll();
            // Returns success response with the list of roles
            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
        }

        /// <summary>
        /// Retrieves a role by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the role.</param>
        /// <returns>An <see cref="IActionResult"/> containing the role if found; otherwise, not found.</returns>
        /// 
        [RequireRole("View Roles Screen")]

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Retrieves a specific role by its ID
            var result = _roleService.GetById(id);
            // Checks if the role was found
            if (result != null)
            {
                // Returns success response with the role data
                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
            }
            else
            {
                // Returns not found response when role doesn't exist
                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
            }
        }

        /// <summary>
        /// Adds a new role.
        /// </summary>
        /// <param name="roleModel">The role model to add.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Add Role")]

        [HttpPost]
        public async Task<IActionResult> Post(RoleCreateUpdateModel roleModel)
        {

            var userName = User.GetUserName();
            roleModel.CreatedBy = userName;
            roleModel.UpdatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Adds the new role
            await _roleService.Add(roleModel);
            // Returns success response after creation
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
        }

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        /// <param name="roleModel">The role model to update.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Edit Role")]

        [HttpPut]
        public async Task<IActionResult> Put(RoleCreateUpdateModel roleModel)
        {
            var userName = User.GetUserName();
            roleModel.CreatedBy = userName;
            // Validates the model state
            if (!ModelState.IsValid)
            {
                // Returns bad request if model validation fails
                return BadRequest(ModelState);
            }
            // Updates the existing role
            await _roleService.Update(roleModel);
            // Returns success response after update
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
        }

        /// <summary>
        /// Deletes a role by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the role to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// 
        [RequireRole("Delete Role")]

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Deletes the role by its ID
            await _roleService.Remove(id);
            // Returns success response after deletion
            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
        }
    }
}
