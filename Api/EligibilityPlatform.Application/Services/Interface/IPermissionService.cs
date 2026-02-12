using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for role management operations.
    /// Provides methods for performing CRUD operations on role records.
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Retrieves all role records.
        /// </summary>
        /// <returns>A list of <see cref="PermissionModel"/> objects containing all role records.</returns>
        List<PermissionModel> GetAll();

        /// <summary>
        /// Retrieves a specific role record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the role record to retrieve.</param>
        /// <returns>The <see cref="PermissionModel"/> with the specified ID.</returns>
        PermissionModel GetById(int id);

        /// <summary>
        /// Adds a new role record.
        /// </summary>
        /// <param name="roleModel">The <see cref="PermissionModel"/> containing the role details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(PermissionCreateUpdateModel roleModel);

        /// <summary>
        /// Updates an existing role record.
        /// </summary>
        /// <param name="roleModel">The <see cref="PermissionModel"/> containing the updated role details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(PermissionCreateUpdateModel roleModel);

        /// <summary>
        /// Removes a role record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the role record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);
    }
}

