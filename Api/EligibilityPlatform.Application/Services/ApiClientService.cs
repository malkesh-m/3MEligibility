using System.Text;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides methods to communicate with external APIs.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiClientService"/> class.
    /// </remarks>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="authenticationService">The authentication service.</param>
    public class ApiClientService(IHttpClientFactory httpClientFactory, IAuthenticationService authenticationService) : IApiClientService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IAuthenticationService _authenticationService = authenticationService;

        /// <summary>
        /// Fetches a sample response from an API endpoint for field mapping configuration.
        /// </summary>
        /// <param name="apiConfig">The API configuration node model.</param>
        /// <param name="endpointPath">The endpoint path to call.</param>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="token">The authentication token, if any.</param>
        /// <param name="requestBody">The request body as a JSON string, if any.</param>
        /// <returns>A dynamic object containing the sample response.</returns>
        public async Task<dynamic> FetchSampleResponseAsync(
           NodeModel apiConfig,
            string endpointPath,
            string method, string? token = null,
            string? requestBody = null,
            CancellationToken ct = default)
        {
            try
            {
                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                // Sets base address from API configuration
                client.BaseAddress = new Uri(apiConfig.NodeUrl ?? "");
                // Sets timeout from API configuration
                client.Timeout = TimeSpan.FromSeconds(apiConfig.TimeoutSeconds);

                // Add default headers
                //if (!string.IsNullOrEmpty(apiConfig.Headers))
                //{
                //    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(apiConfig.Headers);
                //    foreach (var header in headers)
                //    {
                //        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                //    }
                //}

                // Deserializes API key configuration from auth settings
                var apiKeyConfig = JsonConvert.DeserializeObject<ApiKeyConfig>(apiConfig.AuthSettings);
                string requestUri = endpointPath;

                // Handles API key authentication
                if (apiConfig.AuthType == "apikey")
                {
                    string? apiKey = apiKeyConfig?.Token;

                    // Appends API key to request URI
                    if (requestUri.Contains('?'))
                        requestUri += $"&apikey={apiKey}";
                    else
                        requestUri += $"?apikey={apiKey}";
                }

                // Creates HTTP request message
                var request = new HttpRequestMessage(new HttpMethod(method), requestUri);

                // Applies authentication to the request
                request = await _authenticationService.AuthenticateRequestAsync(request, apiConfig, token!, ct);

                // Adds request body if provided for POST or PUT methods
                if (!string.IsNullOrEmpty(requestBody) &&
                    (method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                     method.Equals("PUT", StringComparison.OrdinalIgnoreCase)))
                {
                    // Creates string content with UTF-8 encoding and JSON media type
                    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Content = content;
                }

                // Executes the HTTP request
                var response = await client.SendAsync(request, ct);
                // Ensures the response indicates success
                response.EnsureSuccessStatusCode();
                // Reads response content as string
                var responseContent = await response.Content.ReadAsStringAsync(ct);

                // Parses response as dynamic object
                dynamic responseData;
                // Checks if response content type is JSON
                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    // Parses JSON response
                    responseData = JObject.Parse(responseContent);
                }
                else
                {
                    // For non-JSON responses, creates a simple object with the content
                    var obj = new JObject
                    {
                        ["content"] = responseContent
                    };
                    responseData = obj;
                }

                // Returns the parsed response data
                return responseData;
            }
            catch (Exception)
            {
                // Re-throws the exception for higher level handling
                throw;
            }
        }
    }

    /// <summary>
    /// Represents API key configuration settings.
    /// </summary>
    public class ApiKeyConfig
    {
        /// <summary>
        /// Gets or sets the API token.
        /// </summary>
        public string? Token { get; set; }
    }

}
