using System.Security.Claims;

namespace MEligibilityPlatform
{
    /// <summary>
    /// Provides extension methods for <see cref="ClaimsPrincipal"/> to extract custom claim values.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Retrieves the EntityId claim from the current <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the current user.</param>
        /// <returns>The EntityId claim value as an <see cref="int"/>.</returns>
        /// <exception cref="Exception">Thrown when the EntityId claim is not found.</exception>
        //public static int GetEntityId(this ClaimsPrincipal user)
        //{
        //    var userIdClaim = user?.FindFirst("EntityId");
        //    return userIdClaim != null
        //        ? int.Parse(userIdClaim.Value)
        //        : throw new Exception("EntityId claim not found.");
        //}
        public static int GetTenantId(this ClaimsPrincipal user)
        {
            var tenantIdClaim = user?.FindFirst("tenant_id");
            return tenantIdClaim != null
                ? int.Parse(tenantIdClaim.Value)
                : throw new Exception("TenantId claim not found.");
        }
        public static string GetUserSubId(this ClaimsPrincipal user)
        {
            var subIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);
            return subIdClaim?.Value
                ?? throw new Exception("sub (NameIdentifier) claim not found.");
        }
        public static string GetUserEmail(this ClaimsPrincipal user)
        {
            var emailClaim = user?.FindFirst(ClaimTypes.Email);
            return emailClaim?.Value
                ?? throw new Exception("Email claim not found.");
        }
        public static string GetUserName(this ClaimsPrincipal user)
        {
            var nameClaim = user?.FindFirst("preferred_username");

            return nameClaim == null ? throw new Exception("User Name claim not found.") : nameClaim.Value;
        }
        /// <summary>
        /// Retrieves the UserId claim from the current <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the current user.</param>
        /// <returns>The UserId claim value as an <see cref="int"/>.</returns>
        /// <exception cref="Exception">Thrown when the UserId claim is not found.</exception>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user?.FindFirst("user_id");
            return userIdClaim != null
                ? int.Parse(userIdClaim.Value)
                : throw new Exception("UserId claim not found.");
        }
    }
}
