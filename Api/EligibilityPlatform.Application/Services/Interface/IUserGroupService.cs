using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for user group management operations.
    /// Provides methods for performing CRUD operations and user-group relationship management.
    /// </summary>
    public interface IUserGroupService
    {
        /// <summary>
        /// Retrieves all user group records.
        /// </summary>
        /// <returns>A list of <see cref="UserGroupModel"/> objects containing all user group records.</returns>
        List<UserGroupModel> GetAll();

        /// <summary>
        /// Retrieves user information for a specific group identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the group.</param>
        /// <returns>A list of <see cref="UserInfo"/> objects associated with the specified group.</returns>
        List<UserInfo> GetUserByGroupId(int id,ApiResponse<List<UserGetModel>> users);

        /// <summary>
        /// Adds a new user group record.
        /// </summary>
        /// <param name="userGroupModel">The <see cref="UserGroupModel"/> containing the user group details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<string> Add(UserGroupCreateUpdateModel userGroupModel);

        /// <summary>
        /// Removes a user group record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user group record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);

        /// <summary>
        /// Removes a specific user from a group.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveUserGroup(int userId, int groupId);

        /// <summary>
        /// Checks if a specific group exists by its identifier.
        /// </summary>
        /// <param name="groupId">The unique identifier of the group to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the group exists; otherwise, false.</returns>
        Task<bool> GetByUserGroupId(int groupId);

        /// <summary>
        /// Checks if a specific group exists by its identifier (alternative method).
        /// </summary>
        /// <param name="groupId">The unique identifier of the group to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the group exists; otherwise, false.</returns>
        Task<bool> GetByUserGroupsId(int groupId);
    }
}
