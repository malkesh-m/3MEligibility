using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing user group entities and their relationships.
    /// Extends the base repository interface with additional user-group specific operations.
    /// </summary>
    public interface IUserGroupRepository : IRepository<UserGroup>
    {
        /// <summary>
        /// Checks if a user group exists by its identifier.
        /// </summary>
        /// <param name="groupId">The unique identifier of the user group to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the user group exists; otherwise, false.</returns>
        Task<bool> GetByUserGroupId(int groupId);

        /// <summary>
        /// Checks if a user group exists by its identifier (alternative method).
        /// </summary>
        /// <param name="groupId">The unique identifier of the user group to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the user group exists; otherwise, false.</returns>
        Task<bool> GetByUserGroupsId(int groupId);

        /// <summary>
        /// Retrieves user information for a specific group identifier.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A list of <see cref="UserInfo"/> objects associated with the specified group.</returns>
        List<UserInfo> GetUserByGroupId(int groupId,ApiResponse<List<UserGetModel>> users);

        /// <summary>
        /// Removes a specific user from a group asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task that represents the asynchronous operation, containing the removed <see cref="UserGroup"/> entity.</returns>
        Task<UserGroup?> DeleteUserGroupAsync(int userId, int groupId);
    }
}
