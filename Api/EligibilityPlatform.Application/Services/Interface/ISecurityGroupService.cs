using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for security group management operations.
    /// Provides methods for performing CRUD operations and bulk actions on security group records.
    /// </summary>
    public interface ISecurityGroupService
    {
        /// <summary>
        /// Retrieves all security group records.
        /// </summary>
        /// <returns>A list of <see cref="SecurityGroupModel"/> objects containing all security group records.</returns>
        List<SecurityGroupModel> GetAll();

        /// <summary>
        /// Retrieves a specific security group record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the security group record to retrieve.</param>
        /// <returns>The <see cref="SecurityGroupModel"/> with the specified ID.</returns>
        SecurityGroupModel GetById(int id);

        /// <summary>
        /// Adds a new security group record.
        /// </summary>
        /// <param name="securityGroupModel">The <see cref="SecurityGroupModel"/> containing the security group details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(SecurityGroupUpdateModel securityGroupModel);

        /// <summary>
        /// Updates an existing security group record.
        /// </summary>
        /// <param name="securityGroupModel">The <see cref="SecurityGroupModel"/> containing the updated security group details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(SecurityGroupUpdateModel securityGroupModel);

        /// <summary>
        /// Removes a security group record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the security group record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);

        /// <summary>
        /// Deletes multiple security group records in a single operation.
        /// </summary>
        /// <param name="ids">A list of unique identifiers of the security group records to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
