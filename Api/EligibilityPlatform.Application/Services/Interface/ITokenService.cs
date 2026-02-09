namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for token management operations.
    /// Provides methods for generating and validating reset tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a new reset token.
        /// </summary>
        /// <returns>A string containing the generated reset token.</returns>
        string GenerateResetToken();

        /// <summary>
        /// Validates whether the provided reset token is valid.
        /// </summary>
        /// <param name="token">The reset token to validate.</param>
        /// <returns>True if the token is valid; otherwise, false.</returns>
        bool ValidateResetToken(string token);
    }
}
