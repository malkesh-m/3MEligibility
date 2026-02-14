using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
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

        /// <summary>
        /// Gets the number of users assigned to a group within a tenant.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The number of users in the group.</returns>
        Task<int> GetUserCountByGroupId(int groupId, int tenantId);

        /// <summary>
        /// Gets the number of groups a user belongs to within a tenant.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The number of groups assigned to the user.</returns>
        Task<int> GetGroupCountByUserId(int userId, int tenantId);

        /// <summary>
        /// Retrieves group names for a specific user within a tenant.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A list of group names the user belongs to.</returns>
        Task<List<string>> GetGroupNamesForUser(int userId, int tenantId);
        Task<Dictionary<int, string>> GetGroupNamesByIds(List<int> groupIds,int tenantId);
        

        /// <summary>
        /// Retrieves a group name by group ID within a tenant.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The group name if found; otherwise, null.</returns>
        Task<string?> GetGroupNameById(int groupId, int tenantId);
        Rank GetRank(string groupName);
        Rank GetHighestRank(IEnumerable<string> groupNames);

    }
}
