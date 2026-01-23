using System.Security.Claims;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for user management operations.
    /// Provides methods for performing CRUD operations, authentication, authorization, and user management tasks.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all user records for a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity.</param>
        /// <returns>A list of <see cref="UserGetModel"/> objects containing all user records for the specified entity.</returns>
        Task<ApiResponse<List<UserGetModel>>> GetAll(int tenantId);

        ///// <summary>
        ///// Retrieves a specific user record by its identifier within a specific entity.
        ///// </summary>
        ///// <param name="entityId">The unique identifier of the entity.</param>
        ///// <param name="id">The unique identifier of the user record to retrieve.</param>
        ///// <returns>The <see cref="UserGetModel"/> with the specified ID within the given entity.</returns>
        //UserGetModel GetById(int entityId, int id);

        ///// <summary>
        ///// Retrieves group information for a specific user within an entity.
        ///// </summary>
        ///// <param name="entityId">The unique identifier of the entity.</param>
        ///// <param name="id">The unique identifier of the user.</param>
        ///// <returns>A list of <see cref="GroupModel"/> objects associated with the specified user.</returns>
        //List<GroupModel> GetGroupList(int entityId, int id);

        ///// <summary>
        ///// Adds a new user record with optional file data.
        ///// </summary>
        ///// <param name="fileData">The optional file data (e.g., user profile picture) to upload.</param>
        ///// <param name="userModel">The <see cref="UserAddModel"/> containing the user details to add.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //Task Add(IFormFile? fileData, UserAddModel userModel, string userName);

        ///// <summary>
        ///// Updates an existing user record with optional file data.
        ///// </summary>
        ///// <param name="fileData">The optional file data (e.g., updated user profile picture) to upload.</param>
        ///// <param name="userModel">The <see cref="UserEditModel"/> containing the updated user details.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //Task Update(IFormFile? fileData, UserEditModel userModel, string updatedBy);

        ///// <summary>
        ///// Removes a user record by its identifier within a specific entity.
        ///// </summary>
        ///// <param name="entityId">The unique identifier of the entity.</param>
        ///// <param name="id">The unique identifier of the user record to remove.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //Task Remove(int entityId, int id);
        //Task RemoveUsers(int entityId, List<int> ids);

        ///// <summary>
        ///// Suspends a user account within a specific entity.
        ///// </summary>
        ///// <param name="entityId">The unique identifier of the entity.</param>
        ///// <param name="userId">The unique identifier of the user to suspend.</param>
        ///// <returns>A task representing the asynchronous operation.</returns>
        //Task SuspendUser(int entityId, int userId);

        ///// <summary>
        ///// Authenticates a user with email and password credentials.
        ///// </summary>
        ///// <param name="email">The email address of the user.</param>
        ///// <param name="password">The password of the user.</param>
        ///// <returns>A task that represents the asynchronous operation, containing the authenticated <see cref="User"/> object.</returns>
        //Task<User?> AuthenticateUser(string email, string password);

        ///// <summary>
        ///// Checks if a user has permission to access a specific controller and menu.
        ///// </summary>
        ///// <param name="user">The claims principal representing the current user.</param>
        ///// <param name="controllerName">The name of the controller to check access for.</param>
        ///// <param name="menuUrl">The menu URL to check access for.</param>
        ///// <returns>A task that represents the asynchronous operation, containing true if the user has permission; otherwise, false.</returns>
        //Task<bool> UserHasPermission(ClaimsPrincipal user, string controllerName, string menuUrl);

        ///// <summary>
        ///// Retrieves roles associated with a specific group identifier.
        ///// </summary>
        ///// <param name="groupId">The unique identifier of the group.</param>
        ///// <returns>A task that represents the asynchronous operation, containing a list of <see cref="RoleModel"/> objects associated with the group.</returns>
        //Task<List<RoleModel>> GetRolesByGroupId(int groupId);

        ///// <summary>
        ///// Retrieves roles associated with multiple group identifiers.
        ///// </summary>
        ///// <param name="groupIds">A list of group models containing group identifiers.</param>
        ///// <returns>A task that represents the asynchronous operation, containing a list of <see cref="RoleModel"/> objects associated with the groups.</returns>
        //Task<List<RoleModel>> GetRolesByGroupIds(List<GroupModel> groupIds);

        ///// <summary>
        ///// Verifies the current password for a user.
        ///// </summary>
        ///// <param name="currentUserId">The unique identifier of the current user.</param>
        ///// <param name="currentPassword">The current password to verify.</param>
        ///// <returns>A task that represents the asynchronous operation, containing true if the password is correct; otherwise, false.</returns>
        //Task<bool> VerifyPassword(int currentUserId, string currentPassword);

        ///// <summary>
        ///// Changes the password for a user.
        ///// </summary>
        ///// <param name="userId">The unique identifier of the user.</param>
        ///// <param name="currentPassword">The current password for verification.</param>
        ///// <param name="newPassword">The new password to set.</param>
        ///// <returns>A task that represents the asynchronous operation, containing true if the password was changed successfully; otherwise, false.</returns>
        //Task<bool> ChangePassword(int userId, string currentPassword, string newPassword);

        ///// <summary>
        ///// Requests a password reset for a user by email.
        ///// </summary>
        ///// <param name="email">The email address of the user requesting password reset.</param>
        ///// <param name="resetLink">The password reset link to be sent to the user.</param>
        ///// <returns>A task that represents the asynchronous operation, containing true if the request was successful; otherwise, false.</returns>
        //Task<bool> RequestPasswordReset(string email, string resetLink);

        ///// <summary>
        ///// Resets a user's password using a reset token.
        ///// </summary>
        ///// <param name="token">The password reset token.</param>
        ///// <param name="newPassword">The new password to set.</param>
        ///// <returns>A task that represents the asynchronous operation, containing true if the password was reset successfully; otherwise, false.</returns>
        //Task<bool> ResetPassword(string token, string newPassword);

        ///// <summary>
        ///// Updates the profile picture for a user.
        ///// </summary>
        ///// <param name="entityId">The unique identifier of the entity.</param>
        ///// <param name="fileData">The file data containing the new profile picture.</param>
        ///// <param name="model">The <see cref="UserPictureModel"/> containing the user picture details.</param>
        ///// <returns>A task that represents the asynchronous operation, containing true if the picture was updated successfully; otherwise, false.</returns>
        //Task<bool> UpdatePic(int entityId, IFormFile? fileData, UserPictureModel model);
        //Task<string> ReActivateUser(ReActivationModel user);

        ///// <summary>
        ///// Logs out a user by their identifier.
        ///// </summary>
        ///// <param name="id">The unique identifier of the user to log out.</param>
        ///// <returns>The <see cref="User"/> object that was logged out.</returns>
        //UserGetModel Logout(int id);
        void RemoveUserPermissionsCache(int userId);

        Task<List<string>> GetUserPermissionsAsync(int userId);

    }
}
