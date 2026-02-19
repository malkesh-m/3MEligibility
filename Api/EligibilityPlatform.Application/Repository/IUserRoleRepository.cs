using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing user role entities and their relationships.
    /// Extends the base repository interface with additional user-role specific operations.
    /// </summary>
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        /// <summary>
        /// Checks if a user role exists by its identifier.
        /// </summary>
        /// <param name="roleId">The unique identifier of the user role to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the user role exists; otherwise, false.</returns>
        Task<bool> GetByUserRoleId(int roleId);

        /// <summary>
        /// Checks if a user role exists by its identifier (alternative method).
        /// </summary>
        /// <param name="roleId">The unique identifier of the user role to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the user role exists; otherwise, false.</returns>
        Task<bool> GetByUserRolesId(int roleId);

        /// <summary>
        /// Retrieves user information for a specific role identifier.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role.</param>
        /// <returns>A list of <see cref="UserInfo"/> objects associated with the specified role.</returns>
        List<UserInfo> GetUserByRoleId(int roleId, ApiResponse<List<UserGetModel>> users);

        /// <summary>
        /// Removes a specific user from a role asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="roleId">The unique identifier of the role.</param>
        /// <returns>A task that represents the asynchronous operation, containing the removed <see cref="UserRole"/> entity.</returns>
        Task<UserRole?> DeleteUserRoleAsync(int userId, int roleId);
    }
}
