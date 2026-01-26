using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for API integration operations.
    /// Provides methods for retrieving API details and executing API requests.
    /// </summary>
    public interface IApiIntegrationService
    {
        /// <summary>
        /// Retrieves API details from the specified API URL.
        /// </summary>
        /// <param name="apiUrl">The URL of the API to retrieve details from.</param>
        /// <returns>An object containing the API details.</returns>
        Task<object> GetApiDetailsAsync(string apiUrl);

        /// <summary>
        /// Executes an API request using the provided configuration and parameters.
        /// </summary>
        /// <param name="apiConfig">The API configuration containing endpoint details.</param>
        /// <param name="mappingConfig">The mapping configuration for request/response transformation.</param>
        /// <param name="parameters">Optional dictionary of parameters to be used in the request.</param>
        /// <returns>A dynamic object representing the API response.</returns>
        Task<dynamic> ExecuteRequestAsync(NodeListModel apiConfig, NodeApiListModel mappingConfig, Dictionary<string, object>? parameters = null);
        //Task<TestApiResponse> TestApiAsync(TestApiRequest request);

    }
}
