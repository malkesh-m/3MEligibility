using MEligibilityPlatform.Domain.Enums;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for user role management operations.
    /// Provides methods for performing CRUD operations and user-role relationship management.
    /// </summary>
    public interface IUserRoleService
    {
        /// <summary>
        /// Retrieves all user role records.
        /// </summary>
        /// <returns>A list of <see cref="UserRoleModel"/> objects containing all user role records.</returns>
        List<UserRoleModel> GetAll();

        /// <summary>
        /// Retrieves user information for a specific role identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the role.</param>
        /// <returns>A list of <see cref="UserInfo"/> objects associated with the specified role.</returns>
        List<UserInfo> GetUserByRoleId(int id, ApiResponse<List<UserGetModel>> users);

        /// <summary>
        /// Adds a new user role record.
        /// </summary>
        /// <param name="userRoleModel">The <see cref="UserRoleModel"/> containing the user role details to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<string> Add(UserRoleCreateUpdateModel userRoleModel);

        /// <summary>
        /// Removes a user role record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user role record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Remove(int id);

        /// <summary>
        /// Removes a specific user from a role.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="roleId">The unique identifier of the role.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveUserRole(int userId, int roleId);

        /// <summary>
        /// Checks if a specific role exists by its identifier.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the role exists; otherwise, false.</returns>
        Task<bool> GetByUserRoleId(int roleId);

        /// <summary>
        /// Checks if a specific role exists by its identifier (alternative method).
        /// </summary>
        /// <param name="roleId">The unique identifier of the role to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the role exists; otherwise, false.</returns>
        Task<bool> GetByUserRolesId(int roleId);

        /// <summary>
        /// Gets the number of users assigned to a role within a tenant.
        /// </summary>
        /// <param name="roleId">The role ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The number of users in the role.</returns>
        Task<int> GetUserCountByRoleId(int roleId, int tenantId);

        /// <summary>
        /// Gets the number of roles a user belongs to within a tenant.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The number of roles assigned to the user.</returns>
        Task<int> GetRoleCountByUserId(int userId, int tenantId);

        /// <summary>
        /// Retrieves role names for a specific user within a tenant.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A list of role names the user belongs to.</returns>
        Task<List<string>> GetRoleNamesForUser(int userId, int tenantId);
        Task<Dictionary<int, string>> GetRoleNamesByIds(List<int> roleIds, int tenantId);


        /// <summary>
        /// Retrieves a role name by role ID within a tenant.
        /// </summary>
        /// <param name="roleId">The role ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The role name if found; otherwise, null.</returns>
        Task<string?> GetRoleNameById(int roleId, int tenantId);
        Rank GetRank(string roleName);
        Rank GetHighestRank(IEnumerable<string> roleNames);

    }
}
