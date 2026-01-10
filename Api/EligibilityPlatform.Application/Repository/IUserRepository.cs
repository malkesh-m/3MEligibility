using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing user entities.
    /// Extends the base repository interface with additional user-specific operations.
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="User"/> entity if found.</returns>
        Task<User?> GetByEmail(string email);

        /// <summary>
        /// Retrieves a user by their password reset token.
        /// </summary>
        /// <param name="token">The reset token associated with the user.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="User"/> entity if found.</returns>
        Task<User?> GetByResetToken(string token);

        /// <summary>
        /// Retrieves a user by their profile image.
        /// </summary>
        /// <param name="UserPicture">The byte array representing the user's profile picture.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="User"/> entity if found.</returns>
        Task<User?> GetByImage(byte[]? UserPicture);
    }
}
