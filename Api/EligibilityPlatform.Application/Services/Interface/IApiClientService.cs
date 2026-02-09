using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for API client operations.
    /// Provides methods for fetching sample responses from external APIs.
    /// </summary>
    public interface IApiClientService
    {
        /// <summary>
        /// Fetches a sample response from an external API using the specified configuration and parameters.
        /// </summary>
        /// <param name="ApiConfig">The API configuration containing connection details.</param>
        /// <param name="endpointPath">The specific endpoint path to call.</param>
        /// <param name="method">The HTTP method to use for the request (GET, POST, PUT, etc.).</param>
        /// <param name="token">The authentication token for the API request.</param>
        /// <param name="requestBody">The request body content (optional, used for POST/PUT requests).</param>
        /// <returns>A dynamic object representing the API response.</returns>
        Task<dynamic> FetchSampleResponseAsync(NodeModel ApiConfig, string endpointPath, string method, string token, string requestBody = "");
    }
}
