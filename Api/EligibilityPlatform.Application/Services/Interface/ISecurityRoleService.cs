using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for security role management operations.
    /// Provides methods for performing CRUD operations and bulk actions on security role records.
    /// </summary>
    public interface ISecurityRoleService
    {
        /// <summary>
        /// Retrieves all security role records.
        /// </summary>
        /// <returns>A list of <see cref="SecurityRoleModel"/> objects containing all security role records.</returns>
        List<SecurityRoleModel> GetAll(int tenantId);

        /// <summary>
        /// Retrieves a specific security role record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the security role record to retrieve.</param>
        /// <returns>The <see cref="SecurityRoleModel"/> with the specified ID.</returns>
        SecurityRoleModel GetById(int id, int tenantId);

        /// <summary>
        /// Adds a new security role record.
        /// </summary>
        /// <param name="securityRoleModel">The <see cref="SecurityRoleModel"/> containing the security role details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(SecurityRoleUpdateModel securityRoleModel);

        /// <summary>
        /// Updates an existing security role record.
        /// </summary>
        /// <param name="securityRoleModel">The <see cref="SecurityRoleModel"/> containing the updated security role details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(SecurityRoleUpdateModel securityRoleModel);

        /// <summary>
        /// Removes a security role record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the security role record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);

        /// <summary>
        /// Deletes multiple security role records in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the security role records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
