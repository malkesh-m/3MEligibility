using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for role management operations.
    /// Provides methods for performing CRUD operations on role records.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Retrieves all role records.
        /// </summary>
        /// <returns>A list of <see cref="RoleModel"/> objects containing all role records.</returns>
        List<RoleModel> GetAll();

        /// <summary>
        /// Retrieves a specific role record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the role record to retrieve.</param>
        /// <returns>The <see cref="RoleModel"/> with the specified ID.</returns>
        RoleModel GetById(int id);

        /// <summary>
        /// Adds a new role record.
        /// </summary>
        /// <param name="roleModel">The <see cref="RoleModel"/> containing the role details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(RoleCreateUpdateModel roleModel);

        /// <summary>
        /// Updates an existing role record.
        /// </summary>
        /// <param name="roleModel">The <see cref="RoleModel"/> containing the updated role details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(RoleCreateUpdateModel roleModel);

        /// <summary>
        /// Removes a role record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the role record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);
    }
}
