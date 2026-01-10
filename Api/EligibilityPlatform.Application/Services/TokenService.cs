using EligibilityPlatform.Application.Services.Inteface;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for generating and validating reset tokens.
    /// </summary>
    public class TokenService : ITokenService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class.
        /// </summary>
        public TokenService()
        {
        }

        /// <summary>
        /// Generates a new reset token as a GUID string.
        /// </summary>
        /// <returns>A new reset token string.</returns>
        public string GenerateResetToken()
        {
            // Generates a new GUID and converts it to string
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Validates whether the provided token is a valid GUID.
        /// </summary>
        /// <param name="token">The token string to validate.</param>
        /// <returns>True if the token is valid; otherwise, false.</returns>
        public bool ValidateResetToken(string token)
        {
            // Attempts to parse the token as GUID and returns validation result
            return Guid.TryParse(token, out _);
        }
    }
}
