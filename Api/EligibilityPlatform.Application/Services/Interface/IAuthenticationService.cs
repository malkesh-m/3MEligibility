using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for authentication operations.
    /// Provides methods for authenticating HTTP requests to external APIs.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates an HTTP request message by adding appropriate authentication headers.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <param name="apiConfig">The <see cref="NodeModel"/> containing API configuration details.</param>
        /// <param name="token">The authentication token to be used for the request.</param>
        /// <returns>An authenticated <see cref="HttpRequestMessage"/> with appropriate headers.</returns>
        Task<HttpRequestMessage> AuthenticateRequestAsync(HttpRequestMessage request, NodeModel apiConfig, string token);
    }
}
