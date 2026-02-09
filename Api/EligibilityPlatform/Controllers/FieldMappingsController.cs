using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MEligibilityPlatform.Controllers
{     /// <summary>
      /// API controller for managing field mapping operations between external APIs and internal parameters.
      /// </summary>
      /// <remarks>
      /// Initializes a new instance of the <see cref="FieldMappingsController"/> class.
      /// </remarks>
      /// <param name="nodeApiService">The node API service.</param>
      /// <param name="nodeService">The node service.</param>
      /// <param name="apiClientService">The API client service.</param>
      /// <param name="configuration">The configuration.</param>
      /// <param name="userService">The user service.</param>
      /// <param name="apiParametersService">The API parameters service.</param>
    [Route("api/fieldmappings")]
    [ApiController]
    public partial class FieldMappingsController(INodeApiService nodeApiService, INodeService nodeService, IApiClientService apiClientService, IConfiguration configuration, /*IUserService userService,*/ IApiParametersService apiParametersService) : ControllerBase
    {
        /// <summary>
        /// The node API service instance.
        /// </summary>
        private readonly INodeApiService _nodeApiService = nodeApiService;

        /// <summary>
        /// The node service instance.
        /// </summary>
        private readonly INodeService _nodeService = nodeService;

        /// <summary>
        /// The API client service instance.
        /// </summary>
        private readonly IApiClientService _apiClientService = apiClientService;

        /// <summary>
        /// The configuration instance.
        /// </summary>
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// The user service instance.
        /// </summary>
        //private readonly IUserService _userService = userService;

        /// <summary>
        /// The API parameters service instance.
        /// </summary>
        private readonly IApiParametersService _apiParametersService = apiParametersService;

        /// <summary>
        /// The JWT secret key.
        /// </summary>
        private readonly string _key = configuration["Jwt:Key"]!;

        /// <summary>
        /// The JWT issuer.
        /// </summary>
        private readonly string _issuer = configuration["Jwt:Issuer"]!;

        /// <summary>
        /// The JWT audience.
        /// </summary>
        private readonly string _audience = configuration["Jwt:Audience"]!;

        /// <summary>
        /// The JWT expiration duration in minutes.
        /// </summary>
        private readonly double _expiryDuration = Convert.ToDouble(configuration["Jwt:ExpiresInMinutes"]);

        /// <summary>
        /// Suggests field mappings based on the provided request.
        /// </summary>
        /// <param name="request">The suggest field mappings request.</param>
        /// <returns>A list of suggested field mappings.</returns>
        //[HttpPost("suggest")]
        //public async Task<ActionResult<IEnumerable<SuggestedFieldMapping>>> SuggestFieldMappings(SuggestFieldMappingsRequest request)
        //{
        //    try
        //    {
        //        // Validates that the request object is not null
        //        if (request == null)
        //        {
        //            // Returns a bad request response if the request is null
        //            return BadRequest("Request cannot be null");
        //        }

        //        // Retrieves the mapping configuration using the provided mapping configuration ID
        //        var mappingConfiguration = _nodeApiService.GetById(request.MappingConfigurationId);

        //        // Checks if the mapping configuration was found
        //        if (mappingConfiguration == null)
        //        {
        //            // Returns a not found response when mapping configuration doesn't exist
        //            return NotFound("Mapping configuration not found");
        //        }

        //        // Retrieves the API configuration associated with the mapping configuration
        //        var apiConfiguration = _nodeService.GetById(User.GetTenantId(), mappingConfiguration.NodeId ?? 0);

        //        // Validates that the API configuration exists
        //        if (apiConfiguration == null)
        //        {
        //            // Returns a not found response when API configuration doesn't exist
        //            return NotFound("API configuration not found");
        //        }

        //        // Initializes an empty token string for API authentication
        //        var token = string.Empty;

        //        // Checks if the API configuration has username credentials for authentication
        //        if (apiConfiguration.ApiuserName != null)
        //        {
        //            // Authenticates the user using the API configuration credentials
        //            var user = await _userService.AuthenticateUser(apiConfiguration.ApiuserName, apiConfiguration.Apipassword!);

        //            // Generates a JWT token for the authenticated user
        //            token = GenerateToken(user!);
        //        }

        //        // Handles GET request body processing to replace URL parameters
        //        if (!string.IsNullOrEmpty(request.RequestBody) && mappingConfiguration.HttpMethodType == "GET")
        //        {
        //            // Parses the JSON request body into a dictionary for parameter extraction
        //            var parsedRequestBody = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.RequestBody);

        //            // Checks if the request body contains an 'id' parameter
        //            if (parsedRequestBody!.TryGetValue("id", out object? value))
        //            {
        //                // Extracts the id value from the request body
        //                var idValue = value.ToString();

        //                // Replaces {id} placeholder in the endpoint URL with the extracted id
        //                mappingConfiguration.Apiname = mappingConfiguration.Apiname!.Replace("{id}", idValue);
        //            }
        //        }

                // Fetches sample data from the external API
        //        dynamic sampleData = await _apiClientService.FetchSampleResponseAsync(
        //            apiConfiguration,
        //            mappingConfiguration.Apiname!,
        //            mappingConfiguration.HttpMethodType!,
        //            token,
        //            request.RequestBody);

        //        // Converts the sample data to JToken for easier manipulation
        //        JToken tokenData = (JToken)sampleData;

        //        // Extracts wildcard paths from the sample JSON data
        //        List<string> paths = ExtractPathsWithWildcard(tokenData);

        //        // Creates suggested field mappings collection
        //        var suggestions = new List<SuggestedFieldMapping>();

        //        // Iterates through each extracted path to create field mapping suggestions
        //        foreach (var path in paths)
        //        {
        //            // Extracts sample value from the path
        //            var value = ExtractValueFromPath(sampleData, path);

        //            // Determines data type based on the extracted value
        //            var dataType = DetermineDataType(value);

        //            // Creates and adds a new suggested field mapping
        //            suggestions.Add(new SuggestedFieldMapping
        //            {
        //                SourcePath = path,
        //                TargetField = SuggestTargetFieldName(path),
        //                DataType = DetermineDataType(value),
        //                SampleValue = value?.ToString() ?? ""
        //            });
        //        }

        //        // Deletes existing API parameters for this mapping configuration
        //        await _apiParametersService.DeleteByApiIdAsync(request.MappingConfigurationId);

        //        // Converts suggestions to API parameter models
        //        var apiParameters = suggestions.Select(s => new ApiParametersCreateUpdateModel
        //        {
        //            ParameterName = s.TargetField ?? "",
        //            ParameterType = s.DataType,
        //            ParameterDirection = "",
        //            IsRequired = false,
        //            ApiId = request.MappingConfigurationId
        //        }).ToList();

        //        // Adds the new API parameters to the database
        //        await _apiParametersService.AddRange(apiParameters);

        //        // Returns success response with the suggested field mappings
        //        return Ok(suggestions);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Returns internal server error response with exception details
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error suggesting field mappings: {ex.Message}");
        //    }
        //}

        #region Helper Methods

        /// <summary>
        /// Extracts paths from JSON data with wildcard support for arrays.
        /// </summary>
        /// <param name="sampleData">The JSON sample data.</param>
        /// <returns>A list of extracted paths with wildcard notation.</returns>
        private static List<string> ExtractPathsWithWildcard(JToken sampleData)
        {
            // Uses HashSet to avoid duplicate paths
            HashSet<string> paths = [];

            // Recursively traverses JSON to extract paths
            TraverseJsonWithWildcard(sampleData, paths, "");

            // Converts HashSet to List and returns
            return [.. paths];
        }

        /// <summary>
        /// Recursively traverses JSON token to extract paths with wildcard notation.
        /// </summary>
        /// <param name="token">The JSON token to traverse.</param>
        /// <param name="paths">The collection to store extracted paths.</param>
        /// <param name="currentPath">The current path being built.</param>
        private static void TraverseJsonWithWildcard(JToken token, HashSet<string> paths, string currentPath)
        {
            // Handles JSON object traversal
            if (token.Type == JTokenType.Object)
            {
                // Iterates through each property of the object
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    // Builds the new path for the current property
                    string newPath = string.IsNullOrEmpty(currentPath) ? prop.Name : $"{currentPath}.{prop.Name}";

                    // Recursively traverses the property value
                    TraverseJsonWithWildcard(prop.Value, paths, newPath);
                }
            }
            // Handles JSON array traversal
            else if (token.Type == JTokenType.Array)
            {
                // Adds wildcard notation for arrays
                string newPath = $"{currentPath}[*]";

                // Processes only the first array item to determine structure
                var firstItem = token.First;
                if (firstItem != null)
                {
                    // Recursively traverses the first array item
                    TraverseJsonWithWildcard(firstItem, paths, newPath);
                }
            }
            // Handles leaf nodes (primitive values)
            else
            {
                // Adds the complete path for leaf nodes
                paths.Add(currentPath);
            }
        }

        /// <summary>
        /// Extracts a value from JSON data using the specified path.
        /// </summary>
        /// <param name="data">The JSON data.</param>
        /// <param name="path">The path to extract value from.</param>
        /// <returns>The extracted value or null if not found.</returns>
        private static object? ExtractValueFromPath(dynamic data, string path)
        {
            // Returns null if data or path is null/empty
            if (data == null || string.IsNullOrEmpty(path))
                return null;

            try
            {
                // Converts dynamic data to JToken for path navigation
                var token = Newtonsoft.Json.Linq.JToken.FromObject(data);

                // Handles paths with wildcard notation
                if (path.Contains("[*]"))
                {
                    // Splits path into components for manual traversal
                    string[] parts = path.Split('.');
                    JToken current = token;

                    // Manually traverses each path component
                    foreach (var part in parts)
                    {
                        if (part.Contains("[*]"))
                        {
                            // Extracts property name from wildcard notation
                            var propName = part.Replace("[*]", "");
                            current = current[propName]!;

                            // Handles arrays by using first element
                            if (current is JArray arr && arr.Count > 0)
                            {
                                current = arr[0];
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            current = current[part]!;
                            if (current == null)
                                return null;
                        }
                    }

                    return current?.ToObject<object>();
                }
                else
                {
                    // Handles regular paths without wildcards
                    var arrayIndexPattern = MyRegex1();
                    path = arrayIndexPattern.Replace(path, "$1.$2");

                    // Uses SelectToken for regular path navigation
                    var value = token.SelectToken(path);
                    return !value?.ToObject<object>();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Determines the data type of a value.
        /// </summary>
        /// <param name="value">The value to analyze.</param>
        /// <returns>The determined data type as string.</returns>
        private static string DetermineDataType(object value)
        {
            // Returns string as default for null values
            if (value == null)
            {
                return "string";
            }

            // Checks for integer types
            if (value is int || value is long)
            {
                return "int";
            }

            // Checks for floating-point types
            if (value is float || value is double || value is decimal)
            {
                return "decimal";
            }

            // Checks for boolean type
            if (value is bool)
            {
                return "boolean";
            }

            // Checks for DateTime type
            if (value is DateTime)
            {
                return "datetime";
            }

            // Attempts to parse string as DateTime
            if (value is string strValue && DateTime.TryParse(strValue, out _))
            {
                return "datetime";
            }

            // Defaults to string for all other types
            return "string";
        }

        /// <summary>
        /// Suggests a target field name based on the source path.
        /// </summary>
        /// <param name="sourcePath">The source path to derive field name from.</param>
        /// <returns>The suggested target field name in camelCase.</returns>
        private static string SuggestTargetFieldName(string sourcePath)
        {
            // Returns empty string for null/empty source path
            if (string.IsNullOrEmpty(sourcePath))
            {
                return string.Empty;
            }

            // Removes array indices from the path
            var arrayIndexPattern = MyRegex();
            var pathWithoutIndices = arrayIndexPattern.Replace(sourcePath, string.Empty);

            // Splits path into components
            var parts = pathWithoutIndices.Split('.');

            // Gets the last component of the path
            var lastPart = parts[^1];

            // Converts to camelCase format
            return char.ToLowerInvariant(lastPart[0]) + lastPart[1..];
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate token for.</param>
        /// <returns>The generated JWT token.</returns>
        string GenerateToken(User user)
        {
            // Validates that user is not null
            ArgumentNullException.ThrowIfNull(user);

            // Sets token expiration duration
            double expirationInMinutes = _expiryDuration;

            // Creates claims list for the token
            var claimsList = new List<Claim>
            {
                new("UserId",user.UserId.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                //new("EntityId", user.EntityId.ToString())
            };

            // Adds role claim if user has security group
            if (user.SecurityGroup != null && !string.IsNullOrEmpty(user.SecurityGroup.GroupName))
            {
                claimsList.Add(new Claim(ClaimTypes.Role, user.SecurityGroup.GroupId.ToString()));
            }

            // Converts claims list to array
            var claims = claimsList.ToArray();

            // Creates security key from configured secret
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));

            // Creates signing credentials
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Creates token descriptor with all token properties
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationInMinutes),
                signingCredentials: credentials
            );

            // Writes and returns the generated token
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"\[\d+\]")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
        [System.Text.RegularExpressions.GeneratedRegex(@"(\w+)\[(\d+)\]")]
        private static partial System.Text.RegularExpressions.Regex MyRegex1();
        #endregion
    }

    /// <summary>
    /// Represents a request for suggesting field mappings.
    /// </summary>
    public class SuggestFieldMappingsRequest
    {
        /// <summary>
        /// Gets or sets the mapping configuration ID.
        /// </summary>
        public int MappingConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the request body for API call.
        /// </summary>
        public required string RequestBody { get; set; }
    }

    /// <summary>
    /// Represents a suggested field mapping between source and target.
    /// </summary>
    public class SuggestedFieldMapping
    {
        /// <summary>
        /// Gets or sets the source JSON path.
        /// </summary>
        public string? SourcePath { get; set; }

        /// <summary>
        /// Gets or sets the target field name.
        /// </summary>
        public string? TargetField { get; set; }

        /// <summary>
        /// Gets or sets the data type of the field.
        /// </summary>
        public string? DataType { get; set; }

        /// <summary>
        /// Gets or sets the sample value from the source.
        /// </summary>
        public string? SampleValue { get; set; }
    }
}
