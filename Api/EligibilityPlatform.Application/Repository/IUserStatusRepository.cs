using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing user status entities.
    /// Extends the base repository interface with additional user status-specific operations.
    /// </summary>
    public interface IUserStatusRepository : IRepository<UserStatus>
    {
        /// <summary>
        /// Retrieves the most recently created or modified user status.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the last <see cref="UserStatus"/> entity.</returns>
        Task<UserStatus?> GetLastUserStatus();
    }
}
