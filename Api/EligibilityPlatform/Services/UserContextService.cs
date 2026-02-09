using MEligibilityPlatform.Application.Services.Interface;

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
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == "UserId")?.Value;

            // Converts the claim value from string to integer and returns the result
            return Convert.ToInt32(userIdClaim);
        }
    }
}
