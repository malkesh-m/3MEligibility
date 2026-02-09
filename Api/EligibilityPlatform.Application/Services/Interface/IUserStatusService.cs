using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for user status management operations.
    /// Provides methods for performing CRUD operations on user status records.
    /// </summary>
    public interface IUserStatusService
    {
        /// <summary>
        /// Retrieves all user status records.
        /// </summary>
        /// <returns>A list of <see cref="UserStatusModel"/> objects containing all user status records.</returns>
        List<UserStatusModel> GetAll();

        /// <summary>
        /// Retrieves a specific user status record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user status record.</param>
        /// <returns>The <see cref="UserStatusModel"/> with the specified identifier, if found.</returns>
        UserStatusModel GetById(int id);

        /// <summary>
        /// Adds a new user status record.
        /// </summary>
        /// <param name="userStatusModel">The <see cref="UserStatusModel"/> containing the user status details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(UserStatusAddModel userStatusModel);

        /// <summary>
        /// Updates an existing user status record.
        /// </summary>
        /// <param name="userStatusModel">The <see cref="UserStatusModel"/> containing the updated user status details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Update(UserStatusAddModel userStatusModel);

        /// <summary>
        /// Removes a user status record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user status record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);

        /// <summary>
        /// Removes multiple user status records by their identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the user status records to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task MultipleDelete(List<int> ids);
    }
}
