using System.Security.Claims;

namespace EligibilityPlatform
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
        public static int GetEntityId(this ClaimsPrincipal user)
        {
            var userIdClaim = user?.FindFirst("EntityId");
            return userIdClaim != null
                ? int.Parse(userIdClaim.Value)
                : throw new Exception("EntityId claim not found.");
        }
        public static int GetTenantId(this ClaimsPrincipal user)
        {
            var tenantIdClaim = user?.FindFirst("tenant_id");
            return tenantIdClaim != null
                ? int.Parse(tenantIdClaim.Value)
                : throw new Exception("TenantId claim not found.");
        }
        public static string GetUserName(this ClaimsPrincipal user)
        {
            var nameClaim = user?.FindFirst("name");

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
            var userIdClaim = user?.FindFirst("UserId");
            return userIdClaim != null
                ? int.Parse(userIdClaim.Value)
                : throw new Exception("UserId claim not found.");
        }
    }
}
