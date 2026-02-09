using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides authentication services for HTTP requests based on API configuration.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </remarks>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="configuration">The configuration instance.</param>
    public class AuthenticationService(
        IHttpClientFactory httpClientFactory,
        ILogger<AuthenticationService> logger,
      /*  IUserService userService,*/ IConfiguration configuration) : IAuthenticationService
    {
        /// <summary>
        /// The HTTP client factory instance for creating HTTP clients.
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        /// <summary>
        /// The logger instance for logging authentication operations.
        /// </summary>
        private readonly ILogger<AuthenticationService> _logger = logger;

        /// <summary>
        /// The user service instance for user-related operations.
        /// </summary>
        //private readonly IUserService _userService = userService;
        /// <summary>
        /// The user service instance for configuration-related operations.
        /// </summary>
        private readonly IConfiguration configuration = configuration;

        /// <summary>
        /// Authenticates an HTTP request based on the API configuration.
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate.</param>
        /// <param name="apiConfig">The API configuration node model.</param>
        /// <param name="token">The authentication token, if any.</param>
        /// <returns>The authenticated HTTP request message.</returns>
        public async Task<HttpRequestMessage> AuthenticateRequestAsync(HttpRequestMessage request, NodeModel apiConfig, string token)
        {
            try
            {
                // Logs the authentication type being used
                _logger.LogInformation("Authenticating request using auth type: {AuthType}", apiConfig.AuthType);

                // If no authentication is required, return the request as is
                if (string.IsNullOrEmpty(apiConfig.AuthType) || apiConfig.AuthType.Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    // Returns the request unchanged if no authentication is required
                    return request;
                }

                // Parses authentication settings from JSON string to dictionary
                var authSettings = !string.IsNullOrEmpty(apiConfig.AuthSettings)
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(apiConfig.AuthSettings)
                   ?? []
    : [];

                // Apply authentication based on the auth type
                switch (apiConfig.AuthType.ToLowerInvariant())
                {
                    // Applies Basic authentication
                    case "basic":
                        return await ApplyBasicAuthAsync(request, authSettings!);

                    // Applies OAuth2 authentication
                    case "oauth2":
                        return await ApplyOAuth2AuthAsync(request, authSettings);

                    // Applies API Key authentication
                    case "apikey":
                        return await ApplyApiKeyAuthAsync(request, authSettings);

                    // Applies Bearer Token authentication
                    case "bearer":
                        return await ApplyBearerTokenAuthAsync(request, token);

                    // Handles unsupported authentication types
                    default:
                        _logger.LogWarning("Unsupported auth type: {AuthType}", apiConfig.AuthType);
                        return request;
                }
            }
            catch (Exception ex)
            {
                // Logs any errors that occur during authentication
                _logger.LogError(ex, "Error authenticating request");
                throw;
            }
        }

        /// <summary>
        /// Applies Basic Authentication to the request.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="authSettings">The authentication settings.</param>
        /// <returns>The HTTP request message with Basic Authentication applied.</returns>
        private Task<HttpRequestMessage> ApplyBasicAuthAsync(HttpRequestMessage request, Dictionary<string, string> authSettings)
        {
            // Checks if username and password are present in auth settings
            if (authSettings.TryGetValue("username", out var username) && authSettings.TryGetValue("password", out var password))
            {
                // Encodes username and password as Base64 for Basic authentication
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                // Adds Authorization header with Basic authentication scheme
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                // Logs successful application of Basic authentication
                _logger.LogInformation("Applied Basic Authentication");
            }
            else
            {
                // Logs warning if username or password is missing
                _logger.LogWarning("Missing username or password for Basic Authentication");
            }

            // Returns the modified request
            return Task.FromResult(request);
        }

        /// <summary>
        /// Applies OAuth2 Authentication to the request.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="authSettings">The authentication settings.</param>
        /// <returns>The HTTP request message with OAuth2 Authentication applied.</returns>
        private async Task<HttpRequestMessage> ApplyOAuth2AuthAsync(HttpRequestMessage request, Dictionary<string, string> authSettings)
        {
            try
            {
                // Check if we have all required settings for OAuth2
                if (!authSettings.TryGetValue("tokenUrl", out var tokenUrl) ||
                    !authSettings.TryGetValue("clientId", out var clientId) ||
                    !authSettings.TryGetValue("clientSecret", out var clientSecret))
                {
                    // Logs warning if required OAuth2 settings are missing
                    _logger.LogWarning("Missing required OAuth2 settings");
                    return request;
                }

                // Gets grant type from settings or defaults to client_credentials
                var grantType = authSettings.TryGetValue("grantType", out var gt) ? gt : "client_credentials";

                // Creates token request parameters
                var tokenRequest = new Dictionary<string, string>
                {
                    ["grant_type"] = grantType,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret
                };

                // Add additional parameters based on grant type
                if (grantType == "password" &&
                    authSettings.TryGetValue("username", out var username) &&
                    authSettings.TryGetValue("password", out var password))
                {
                    // Adds username and password for password grant type
                    tokenRequest["username"] = username;
                    tokenRequest["password"] = password;
                }
                else if (grantType == "authorization_code" &&
                         authSettings.TryGetValue("code", out var code) &&
                         authSettings.TryGetValue("redirectUri", out var redirectUri))
                {
                    // Adds code and redirect URI for authorization_code grant type
                    tokenRequest["code"] = code;
                    tokenRequest["redirect_uri"] = redirectUri;
                }

                // Creates HTTP client for token request
                var client = _httpClientFactory.CreateClient();

                // Sends token request to token endpoint
                var content = new FormUrlEncodedContent(tokenRequest);
                var tokenResponse = await client.PostAsync(tokenUrl, content);
                // Ensures the token request was successful
                tokenResponse.EnsureSuccessStatusCode();

                // Parses token response
                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

                // Extracts access token from response
                if (tokenData.TryGetProperty("access_token", out var accessTokenElement))
                {
                    // Gets access token string
                    var accessToken = accessTokenElement.GetString();
                    // Adds Authorization header with Bearer token
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    // Logs successful application of OAuth2 authentication
                    _logger.LogInformation("Applied OAuth2 Authentication");
                }
                else
                {
                    // Logs warning if access token is missing in response
                    _logger.LogWarning("OAuth2 token response did not contain access_token");
                }
            }
            catch (Exception ex)
            {
                // Logs any errors that occur during OAuth2 authentication
                _logger.LogError(ex, "Error applying OAuth2 Authentication");
            }

            // Returns the modified request
            return request;
        }

        /// <summary>
        /// Applies API Key Authentication to the request.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="authSettings">The authentication settings.</param>
        /// <returns>The HTTP request message with API Key Authentication applied.</returns>
        private Task<HttpRequestMessage> ApplyApiKeyAuthAsync(HttpRequestMessage request, Dictionary<string, string> authSettings)
        {
            // Checks if API key is present in auth settings
            if (!authSettings.TryGetValue("apiKey", out var apiKey))
            {
                // Logs warning if API key is missing
                _logger.LogWarning("Missing API key for API Key Authentication");
                return Task.FromResult(request);
            }

            // Gets API key location from settings or defaults to header
            var location = authSettings.TryGetValue("location", out var loc) ? loc : "header";
            // Gets API key name from settings or defaults to X-API-Key
            var name = authSettings.TryGetValue("name", out var n) ? n : "X-API-Key";

            // Applies API key based on location
            switch (location.ToLowerInvariant())
            {
                // Adds API key to request headers
                case "header":
                    request.Headers.Add(name, apiKey);
                    break;

                // Adds API key to query parameters
                case "query":
                    var uri = request.RequestUri;
                    var uriBuilder = new UriBuilder(uri!);
                    var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                    query[name] = apiKey;
                    uriBuilder.Query = query.ToString();
                    request.RequestUri = uriBuilder.Uri;
                    break;

                // Handles unsupported API key locations
                default:
                    _logger.LogWarning("Unsupported API key location: {Location}", location);
                    break;
            }

            // Logs successful application of API Key authentication
            _logger.LogInformation("Applied API Key Authentication");
            // Returns the modified request
            return Task.FromResult(request);
        }

        /// <summary>
        /// Applies Bearer Token Authentication to the request.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="token">The bearer token.</param>
        /// <returns>The HTTP request message with Bearer Token Authentication applied.</returns>
        private Task<HttpRequestMessage> ApplyBearerTokenAuthAsync(HttpRequestMessage request, string token)
        {
            // Checks if token is provided
            if (!string.IsNullOrEmpty(token))
            {
                // Adds Authorization header with Bearer token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Logs successful application of Bearer Token authentication
                _logger.LogInformation("Applied Bearer Token Authentication");
            }
            else
            {
                // Logs warning if token is missing
                _logger.LogWarning("Missing token for Bearer Token Authentication");
            }

            // Returns the modified request
            return Task.FromResult(request);
        }
    }
}
