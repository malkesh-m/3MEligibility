using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Infrastructure.Context;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Infrastructure.Repository
{  /// <summary>
   /// Repository implementation for managing <see cref="User"/> entities.
   /// Provides data access logic for users using the base <see cref="Repository{TEntity}"/> class.
   /// </summary>
   /// <remarks>
   /// Initializes a new instance of the <see cref="UserRepository"/> class.
   /// </remarks>
   /// <param name="context">The database context used for data operations.</param>
   /// <param name="userContext">Provides access to the current HTTP context for user-related data.</param>
    public class UserRepository(EligibilityDbContext context, IUserContextService userContext) : Repository<User>(context, userContext), IUserRepository
    {

        

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The <see cref="User"/> entity with the specified email, or null if not found.</returns>
        public async Task<User?> GetByEmail(string email)
        {
            // Queries the Users DbSet to find the first user with matching email address
            // Uses case-sensitive comparison to ensure exact email match
            // Returns null if no user is found with the specified email
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Retrieves a user by their password reset token.
        /// </summary>
        /// <param name="token">The reset password token to search for.</param>
        /// <returns>The <see cref="User"/> entity with the specified reset token, or null if not found.</returns>
        //public async Task<User?> GetByResetToken(string token)
        //{
        //    // Queries the Users DbSet to find the first user with matching password reset token
        //    // Compares the ResetPasswordToken property with the provided token value
        //    // Returns null if no user is found with the specified reset token

        //    return await _context.Users
        //        .FirstOrDefaultAsync(user => user.ResetPasswordToken == token);
        //}

        ///// <summary>
        ///// Retrieves a user by their profile picture.
        ///// </summary>
        ///// <param name="UserPicture">The user picture byte array to search for.</param>
        ///// <returns>The <see cref="User"/> entity with the specified profile picture, or null if not found.</returns>
        //public async Task<User?> GetByImage(byte[]? UserPicture)
        //{
        //    // Queries the Users DbSet to find the first user with matching profile picture bytes
        //    // Compares the UserPicture byte array with the provided image data
        //    // Returns null if no user is found with the specified profile picture
        //    return await _context.Users.FirstOrDefaultAsync(user => user.UserPicture == UserPicture);
        //}
    }
}

