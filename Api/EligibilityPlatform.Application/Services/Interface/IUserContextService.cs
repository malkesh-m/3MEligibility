namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for user context management operations.
    /// Provides methods for accessing current user context information.
    /// </summary>
    public interface IUserContextService
    {
        /// <summary>
        /// Retrieves the unique identifier of the current authenticated user.
        /// </summary>
        /// <returns>The unique identifier of the current user.</returns>
        int GetUserId();
    }
}
