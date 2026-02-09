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

        /// <summary>
        /// Retrieves the current tenant identifier.
        /// </summary>
        /// <returns>The tenant identifier, or 0 when unavailable.</returns>
        int GetTenantId();

        /// <summary>
        /// Retrieves the current user name from claims.
        /// </summary>
        /// <returns>The user name, or null when unavailable.</returns>
        string? GetUserName();

        /// <summary>
        /// Retrieves the remote IP address for the current request.
        /// </summary>
        /// <returns>The IP address, or null when unavailable.</returns>
        string? GetIpAddress();

        /// <summary>
        /// Retrieves the current controller name from route data.
        /// </summary>
        /// <returns>The controller name, or null when unavailable.</returns>
        string? GetControllerName();

        /// <summary>
        /// Retrieves the current action name from route data.
        /// </summary>
        /// <returns>The action name, or null when unavailable.</returns>
        string? GetActionName();
    }
}
