using MEligibilityPlatform;
using MEligibilityPlatform.Application.Services.Interface;
using Microsoft.AspNetCore.Routing;

namespace MEligibilityPlatform.Services
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserContextService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor used to retrieve user claims.</param>
    public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Retrieves the User ID from the current HTTP context claims.
        /// </summary>
        /// <returns>The User ID as an integer.</returns>
        /// <exception cref="FormatException">Thrown when the claim value cannot be converted to an integer.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the UserId claim is missing.</exception>
        public int GetUserId()
        {
            // Accesses the current HTTP context and retrieves the UserId claim from the user's claims principal
            var user = _httpContextAccessor.HttpContext?.User;
            return user.GetUserIdOrDefault();
        }

        /// <summary>
        /// Retrieves the Tenant ID from the current HTTP context claims.
        /// </summary>
        /// <returns>The Tenant ID as an integer, or 0 when unavailable.</returns>
        public int GetTenantId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user.GetTenantIdOrDefault();
        }

        /// <summary>
        /// Retrieves the current user name from claims.
        /// </summary>
        public string? GetUserName()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user.GetUserNameOrDefault();
        }

        /// <summary>
        /// Retrieves the remote IP address for the current request.
        /// </summary>
        public string? GetIpAddress() =>
            _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        /// <summary>
        /// Retrieves the current controller name from route data.
        /// </summary>
        public string? GetControllerName() =>
            _httpContextAccessor.HttpContext?.GetRouteData()?.Values["controller"]?.ToString();

        /// <summary>
        /// Retrieves the current action name from route data.
        /// </summary>
        public string? GetActionName() =>
            _httpContextAccessor.HttpContext?.GetRouteData()?.Values["action"]?.ToString();
    }
}
