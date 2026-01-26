using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides services for interacting with APIs, including SOAP and RESTful services.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work.</param>
    /// <param name="httpClient">The HTTP client.</param>
    public class ApiService(IUnitOfWork uow, HttpClient httpClient) : IApiService
    {
        /// <summary>
        /// The unit of work instance for database operations.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// The HTTP client instance for making HTTP requests.
        /// </summary>
        private readonly HttpClient _httpClient = httpClient;

        #region GetApi list
        /// <summary>
        /// Retrieves API details by fetching and parsing the WSDL file from a given node URL.
        /// </summary>
        /// <param name="nodeId">The unique identifier of the node.</param>
        /// <returns>A list of <see cref="ApiParameterModel"/> representing the extracted API details.</returns>
        /// <exception cref="ArgumentException">Thrown when the WSDL URL is null or empty.</exception>
        /// <exception cref="Exception">Thrown when fetching or parsing the WSDL fails.</exception>
        public async Task<List<ApiParameterModel>> GetApiDetailsAsync(int nodeId)
        {
            // Retrieves the node entity by ID from the repository
            var node = _uow.NodeModelRepository.GetById(nodeId);

            // Validates that the node URL is not null or empty
            if (string.IsNullOrEmpty(node.NodeUrl))
            {
                ArgumentException argumentException1 = new("WSDL URL cannot be null or empty.", nameof(nodeId));
                ArgumentException argumentException = argumentException1;
                throw argumentException;
            }

            try
            {
                // Creates a new HTTP client instance for making the request
                using HttpClient client = new();
                // Sends a GET request to the node URL
                HttpResponseMessage response = await client.GetAsync(node.NodeUrl);
                // Ensures the response status code indicates success
                response.EnsureSuccessStatusCode();

                // Reads the response content as a string
                string wsdlContent = await response.Content.ReadAsStringAsync();

                // Parses the WSDL content and returns the extracted API details
                return ParseWsdl(wsdlContent);
            }
            catch (Exception ex)
            {
                // Throws a new exception with a descriptive message if fetching or parsing fails
                throw new Exception($"Failed to fetch or parse WSDL: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parses the WSDL content and extracts API details including operations and parameters.
        /// </summary>
        /// <param name="wsdlContent">The WSDL content as an XML string.</param>
        /// <returns>A list of <see cref="ApiParameterModel"/> containing the extracted API details.</returns>
        private static List<ApiParameterModel> ParseWsdl(string wsdlContent)
        {
            // Initializes a list to store the extracted API details
            var apiDetails = new List<ApiParameterModel>();

            // Creates a new XML document instance
            XmlDocument wsdlDoc = new();
            // Loads the WSDL content into the XML document
            wsdlDoc.LoadXml(wsdlContent);

            // Creates a namespace manager to handle XML namespaces
            XmlNamespaceManager nsmgr = new(wsdlDoc.NameTable);
            // Adds the WSDL namespace
            nsmgr.AddNamespace("wsdl", "http://schemas.xmlsoap.org/wsdl/");
            // Adds the SOAP namespace
            nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/wsdl/soap/");
            // Adds the XML schema namespace
            nsmgr.AddNamespace("s", "http://www.w3.org/2001/XMLSchema");

            // Extracts all operation nodes from the WSDL document
            XmlNodeList? operationNodes = wsdlDoc.SelectNodes("//wsdl:portType/wsdl:operation", nsmgr);

            // Iterates through each operation node
            foreach (XmlNode operationNode in operationNodes!)
            {
                // Creates a new API parameter model for the current operation
                var apiDetail = new ApiParameterModel
                {
                    // Sets the operation name from the node attributes
                    Name = operationNode.Attributes?["name"]?.Value ?? ""
                };

                // Finds the corresponding binding node for the current operation
                XmlNode? bindingNode = wsdlDoc.SelectSingleNode($"//wsdl:binding[wsdl:operation[@name='{apiDetail.Name}']]", nsmgr);
                // Checks if the binding node exists
                if (bindingNode != null)
                {
                    // Extracts the SOAP action from the binding node
                    XmlNode? soapActionNode = bindingNode.SelectSingleNode($"wsdl:operation[@name='{apiDetail.Name}']/soap:operation", nsmgr);
                    // Sets the action URL from the SOAP action attribute
                    apiDetail.ActionUrl = soapActionNode?.Attributes!["soapAction"]?.Value ?? string.Empty;
                }

                // Finds the input message node for the current operation
                XmlNode? inputNode = operationNode.SelectSingleNode("wsdl:input", nsmgr);
                // Checks if the input node exists
                if (inputNode != null)
                {
                    // Extracts the input message name and removes any namespace prefix
                    string? inputMessageName = inputNode.Attributes?["message"]?.Value.Split(':')[1];
                    // Validates that the message name is not empty
                    if (!string.IsNullOrEmpty(inputMessageName))
                    {
                        // Finds the message definition node
                        XmlNode? messageNode = wsdlDoc.SelectSingleNode($"//wsdl:message[@name='{inputMessageName}']", nsmgr);
                        // Checks if the message node exists
                        if (messageNode != null)
                        {
                            // Extracts all part nodes from the message
                            XmlNodeList partNodes = messageNode.SelectNodes("wsdl:part", nsmgr)!;
                            // Iterates through each part node
                            foreach (XmlNode partNode in partNodes)
                            {
                                // Extracts the element type and removes any namespace prefix
                                string? elementType = partNode.Attributes?["element"]?.Value.Split(':')[1];
                                // Validates that the element type is not empty
                                if (!string.IsNullOrEmpty(elementType))
                                {
                                    // Finds the element definition node
                                    XmlNode? elementNode = wsdlDoc.SelectSingleNode($"//s:element[@name='{elementType}']", nsmgr);
                                    // Checks if the element node exists
                                    if (elementNode != null)
                                    {
                                        // Extracts all child element nodes from the complex type sequence
                                        XmlNodeList? elementChildNodes = elementNode?.SelectNodes("s:complexType/s:sequence/s:element", nsmgr);
                                        // Iterates through each child element node
                                        if (elementChildNodes != null)
                                        {
                                            foreach (XmlNode elementChildNode in elementChildNodes)
                                            {
                                                // Creates a new parameter detail for the current element
                                                var paramDetail = new ParameterDetail
                                                {
                                                    // Sets the parameter name from the element attributes
                                                    Name = elementChildNode.Attributes!["name"]?.Value!,
                                                    // Sets the parameter type from the element attributes
                                                    Type = elementChildNode.Attributes["type"]?.Value!
                                                };
                                                // Adds the parameter detail to the API detail parameters list
                                                apiDetail.Parameters.Add(paramDetail);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Validates that the API detail has a name and is not already in the list
                if (!string.IsNullOrEmpty(apiDetail.Name) && !apiDetails.Exists(x => x.Name == apiDetail.Name))
                {
                    // Adds the API detail to the results list
                    apiDetails.Add(apiDetail);
                }
            }

            // Returns the list of extracted API details
            return apiDetails;
        }
        #endregion

        #region Call soap apis
        /// <summary>
        /// Sends a SOAP request to the specified API and retrieves the response.
        /// </summary>
        /// <param name="model">A <see cref="SoapApiModel"/> containing the API URL, action, and SOAP content.</param>
        /// <returns>A string containing the SOAP response from the API.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="Exception">Returns an error message if an exception occurs.</exception>
        public async Task<string> CallSoapApi(SoapApiModel model)
        {
            try
            {
                // Constructs the SOAP envelope with the provided content
                string soapEnvelope = $@"
                <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:web='http://tempuri.org/'>
                    <soapenv:Header/>
                    <soapenv:Body>
                        {model.Content}
                    </soapenv:Body>
                </soapenv:Envelope>";

                // Creates a new string content with the SOAP envelope
                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

                // Adds the SOAP action header to the request
                content.Headers.Add("SOAPAction", model.Action);

                // Creates a new HTTP client instance
                using HttpClient client = new();

                // Sends a POST request to the specified URL with the SOAP content
                var response = await client.PostAsync(model.Url, content);

                // Ensures the response status code indicates success
                response.EnsureSuccessStatusCode();

                // Reads the response content as a string
                string responseContent = await response.Content.ReadAsStringAsync();
                // Returns the response content
                return responseContent;

            }
            catch (Exception ex)
            {
                // Returns an error message if an exception occurs
                return $"Error: {ex.Message}";
            }
        }
        #endregion

        #region Get Rest API List
        /// <summary>
        /// Fetches and parses the Swagger (OpenAPI) documentation for the given node ID, extracting available REST API details.
        /// </summary>
        /// <param name="nodeId">The ID of the node containing the Swagger URL.</param>
        /// <returns>A list of <see cref="ApiParameterModel"/> representing the available API endpoints.</returns>
        /// <exception cref="ArgumentException">Thrown if the Swagger URL is null or empty.</exception>
        /// <exception cref="Exception">Thrown if an error occurs while fetching or parsing the Swagger file.</exception>
        public async Task<List<ApiParameterModel>> GetRestApiDetailsAsync(int nodeId)
        {
            // Retrieves the node entity by ID from the repository
            var node = _uow.NodeModelRepository.GetById(nodeId);

            // Validates that the node URL is not null or empty
            if (string.IsNullOrEmpty(node.NodeUrl))
            {
                ArgumentException argumentException = new("Swagger URL cannot be null or empty.", nameof(nodeId));
                throw argumentException;
            }

            try
            {
                // Fetches the Swagger content from the node URL
                var swaggerContent = await FetchSwaggerContentAsync(node.NodeUrl);
                // Parses the Swagger content into an OpenAPI document
                var openApiDocument = ParseSwaggerContent(swaggerContent);
                // Extracts API endpoints from the OpenAPI document and returns them
                return ExtractApiEndpoints(openApiDocument);
            }
            catch (Exception ex)
            {
                // Throws a new exception with a descriptive message if fetching or parsing fails
                throw new Exception($"Error fetching or parsing Swagger file: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetches the Swagger (OpenAPI) content from the given URL.
        /// </summary>
        /// <param name="swaggerUrl">The URL of the Swagger documentation.</param>
        /// <returns>A string containing the Swagger JSON content.</returns>
        private async Task<string> FetchSwaggerContentAsync(string swaggerUrl)
        {
            // Uses the HTTP client to get the Swagger content as a string
            return await _httpClient.GetStringAsync(swaggerUrl);
        }

        /// <summary>
        /// Parses the Swagger JSON content into an OpenApiDocument.
        /// </summary>
        /// <param name="swaggerContent">The raw Swagger JSON string.</param>
        /// <returns>An <see cref="OpenApiDocument"/> representing the API structure.</returns>
        private static OpenApiDocument ParseSwaggerContent(string swaggerContent)
        {
            // Creates a new OpenAPI string reader
            var openApiReader = new OpenApiStringReader();
            // Reads the Swagger content into an OpenAPI document
            var openApiDocument = openApiReader.Read(swaggerContent, out _);

            // Returns the parsed OpenAPI document
            return openApiDocument;
        }

        /// <summary>
        /// Extracts API endpoints and their details from the parsed OpenAPI document.
        /// </summary>
        /// <param name="openApiDocument">The parsed OpenAPI document.</param>
        /// <returns>A list of API details.</returns>
        private static List<ApiParameterModel> ExtractApiEndpoints(OpenApiDocument openApiDocument)
        {
            // Initializes a list to store the extracted API endpoints
            var apiEndpoints = new List<ApiParameterModel>();

            // Iterates through each path in the OpenAPI document
            foreach (var path in openApiDocument.Paths)
            {
                // Iterates through each operation in the current path
                foreach (var operation in path.Value.Operations)
                {
                    // Creates a new API parameter model for the current endpoint
                    var endpoint = new ApiParameterModel
                    {
                        // Sets the endpoint name from the path key
                        Name = path.Key,
                        // Sets the HTTP method from the operation key
                        ActionUrl = operation.Key.ToString().ToUpper(),
                        // Extracts parameters from the operation
                        Parameters = ExtractParameters(operation.Value.Parameters),
                        // Extracts request body from the operation
                        RequestBody = ExtractRequestBody(operation.Value.RequestBody),
                    };

                    // Adds the endpoint to the results list
                    apiEndpoints.Add(endpoint);
                }
            }

            // Returns the list of extracted API endpoints
            return apiEndpoints;
        }

        /// <summary>
        /// Extracts parameter details from an API operation.
        /// </summary>
        /// <param name="parameters">The list of parameters from the API operation.</param>
        /// <returns>A list of parameter details.</returns>
        private static List<ParameterDetail> ExtractParameters(IList<OpenApiParameter> parameters)
        {
            // Projects each OpenAPI parameter to a ParameterDetail object
            return [.. parameters.Select(parameter => new ParameterDetail
            {
                // Sets the parameter name
                Name = parameter.Name,
                // Sets the parameter type from the schema, defaults to "string" if null
                Type = parameter.Schema?.Type ?? "string",
                // Sets whether the parameter is required
                Required = parameter.Required
            })];
        }

        /// <summary>
        /// Extracts request body details from an API operation.
        /// </summary>
        /// <param name="requestBody">The request body definition.</param>
        /// <returns>A structured request body model.</returns>
        private static ApiRequestBody? ExtractRequestBody(OpenApiRequestBody requestBody)
        {
            // Returns null if the request body is null
            if (requestBody == null)
            {
                return null;
            }

            // Initializes a list to store request body parameters
            var requestBodyParameters = new List<ParameterDetail>();

            // Iterates through each content type in the request body
            foreach (var content in requestBody.Content)
            {
                // Processes JSON content specifically
                if (content.Key == "application/json" && content.Value.Schema != null)
                {
                    // Extracts schema parameters from the JSON schema
                    ExtractSchemaParameters(content.Value.Schema, requestBodyParameters);
                }
            }

            // Creates and returns a new API request body model
            return new ApiRequestBody
            {
                // Sets the request body description
                Description = requestBody.Description,
                // Sets the content type
                ContentType = "application/json",
                // Sets the extracted parameters
                Parameters = requestBodyParameters
            };
        }

        /// <summary>
        /// Recursively extracts schema parameters from an OpenAPI schema definition.
        /// </summary>
        /// <param name="schema">The OpenAPI schema.</param>
        /// <param name="parameters">The list where extracted parameters will be stored.</param>
        /// <param name="parentName">The parent name for nested parameters.</param>
        private static void ExtractSchemaParameters(OpenApiSchema schema, List<ParameterDetail> parameters, string parentName = "")
        {
            // Checks if the schema has properties
            if (schema.Properties != null)
            {
                // Iterates through each property in the schema
                foreach (var property in schema.Properties)
                {
                    // Constructs the full parameter name with parent prefix if applicable
                    var parameterName = string.IsNullOrEmpty(parentName) ? property.Key : $"{parentName}.{property.Key}";
                    // Adds a new parameter detail to the list
                    parameters.Add(new ParameterDetail
                    {
                        // Sets the parameter name
                        Name = parameterName,
                        // Sets the parameter type, defaults to "object" if null
                        Type = property.Value.Type ?? "object",
                        // Sets whether the parameter is required based on the schema required list
                        Required = schema.Required.Contains(property.Key)
                    });

                    // Recursively extracts parameters for nested objects
                    if (property.Value.Type == "object" || property.Value.Properties != null)
                    {
                        ExtractSchemaParameters(property.Value, parameters, parameterName);
                    }
                }
            }
        }
        #endregion

        #region Rest API Call
        /// <summary>
        /// Calls a REST API based on the provided request details.
        /// </summary>
        /// <param name="request">The request model containing API ID and parameters.</param>
        /// <returns>A response model indicating success or failure.</returns>
        public async Task<ResponseModel> CallRestApi(ExecuteApiModel request)
        {
            // Retrieves the API details by ID from the repository
            var apiDetails = _uow.NodeApiRepository.GetById(request.ApiId);
            // Retrieves the node entity by ID from the repository
            var node = _uow.NodeModelRepository.GetById((int)apiDetails.NodeId!);

            // Extracts the base URL by removing the "swagger" part from the node URL
            var baseUrl = node.NodeUrl!.Split(["swagger"], StringSplitOptions.None)[0];
            // Constructs the full API URL by combining base URL and API name
            var url = new Uri(new Uri(baseUrl), apiDetails.Apiname).ToString();

            // Checks if authentication is required for the node
            if (node.IsAuthenticationRequired)
            {
                // Retrieves an authentication token from the login API
                ArgumentException argumentException = new("Swagger URL cannot be null or empty.", nameof(request));
                var token = await GetTokenFromLoginApiAsync(node, argumentException);
                // Checks if a token keyword exists for authentication header
                if (node.IsTokenKeywordExist)
                {
                    // Sets the authorization header with the token keyword and token
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(node.TokenKeyword, token);
                }
                else
                {
                    // Sets the authorization header with just the token
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
                }
            }

            // Makes the API request and gets the response
            var response = await MakeApiRequestAsync(url, apiDetails.HttpMethodType?.ToUpper() ?? "", request.Parameters!);

            // Checks if the response indicates success
            if (response.IsSuccessStatusCode)
            {
                // Returns a success response model with the response data
                return new ResponseModel { IsSuccess = true, Data = await response.Content.ReadAsStringAsync() };
            }

            // Returns a failure response model with an error message
            return new ResponseModel { IsSuccess = false, Message = "Something went wrong" };
        }

        /// <summary>
        /// Makes an API request with the specified HTTP method and parameters.
        /// </summary>
        /// <param name="url">The API endpoint URL.</param>
        /// <param name="method">The HTTP method (GET, POST, PUT, DELETE).</param>
        /// <param name="parameters">The parameters to be sent with the request.</param>
        /// <returns>An HttpResponseMessage containing the API response.</returns>
        private async Task<HttpResponseMessage> MakeApiRequestAsync(string url, string method, Dictionary<string, object> parameters)
        {
            // Handles different HTTP methods
            switch (method)
            {
                case "GET":
                    // Constructs a query string from parameters for GET requests
                    var queryString = parameters == null ? "" : string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
                    // Sends a GET request with the query string
                    return await _httpClient.GetAsync($"{url}?{queryString}");

                case "POST":
                    // Serializes parameters to JSON for POST requests
                    var postContent = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
                    // Sends a POST request with the JSON content
                    return await _httpClient.PostAsync(url, postContent);

                case "PUT":
                    // Serializes parameters to JSON for PUT requests
                    var putContent = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
                    // Sends a PUT request with the JSON content
                    return await _httpClient.PutAsync(url, putContent);

                case "DELETE":
                    // Constructs a query string from parameters for DELETE requests
                    var deleteQueryString = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
                    // Sends a DELETE request with the query string
                    return await _httpClient.DeleteAsync($"{url}?{deleteQueryString}");

                default:
                    // Throws an exception for invalid HTTP methods
                    throw new ArgumentException("Invalid HTTP method.");
            }
        }
        #endregion

        #region Get Token for Authentication
        /// <summary>
        /// Retrieves an authentication token by calling the login API based on the provided node details.
        /// </summary>
        /// <param name="node">The node containing authentication details and API information.</param>
        /// <returns>A string containing the authentication token.</returns>
        /// <exception cref="ArgumentException">Thrown when the Swagger URL is null or empty.</exception>
        /// <exception cref="Exception">Thrown when fetching or parsing the Swagger file fails, or when the login endpoint is not found.</exception>
        public async Task<string> GetTokenFromLoginApiAsync(Node node, Exception argumentException)
        {
            // Validates that the node URL is not null or empty
            if (string.IsNullOrEmpty(node.NodeUrl))
                throw argumentException;

            // Fetches and parses the Swagger document from the node URL
            var openApiDocument = await FetchAndParseSwaggerAsync(node.NodeUrl) ?? throw new Exception("Failed to fetch or parse the Swagger file.");

            // Finds the login endpoint in the OpenAPI document
            var loginEndpoint = FindLoginEndpoint(openApiDocument) ?? throw new Exception("Login endpoint not found in the Swagger file.");

            // Extracts the base URL by removing the "swagger" part from the node URL
            var baseUrl = node.NodeUrl.Split(["swagger"], StringSplitOptions.None)[0];
            // Constructs the full login API URL by combining base URL and endpoint path
            var url = new Uri(new Uri(baseUrl), loginEndpoint.ActionUrl).ToString();

            // Gets login credentials from the node configuration
            var parameters = GetLoginCredentials(node);

            // Makes the login API request
            var response = await MakeApiRequestAsync(url, loginEndpoint.Method, parameters);

            // Checks if the login request was successful
            if (response.IsSuccessStatusCode)
            {
                // Reads the response content as a string
                var responseBody = await response.Content.ReadAsStringAsync();
                // Parses the response content as JSON
                var jsonDocument = JsonDocument.Parse(responseBody);

                // Iterates through each property in the JSON response
                foreach (var property in jsonDocument.RootElement.EnumerateObject())
                {
                    // Looks for a property named "token" (case-insensitive)
                    if (property.Name.Equals("token", StringComparison.OrdinalIgnoreCase))
                    {
                        // Returns the token value as a string
                        return property.Value.GetString()!;
                    }
                }
            }

            // Throws an exception if token extraction fails
            throw new Exception("Failed to get token from the login API.");
        }

        /// <summary>
        /// Retrieves login credentials based on the node configuration.
        /// </summary>
        /// <param name="node">The node containing API username and password fields.</param>
        /// <returns>A dictionary containing login credentials.</returns>
        private static Dictionary<string, object> GetLoginCredentials(Node node)
        {
            // Creates a new dictionary for login credentials
            var loginCred = new Dictionary<string, object>
            {
                // Adds the username field with the API username value
                [node.UsernameField] = node.ApiuserName!,
                // Adds the password field with the API password value
                [node.PasswordField] = node.Apipassword!
            };
            // Returns the login credentials dictionary
            return loginCred;
        }

        /// <summary>
        /// Finds the login API endpoint from the Swagger document.
        /// </summary>
        /// <param name="openApiDocument">The OpenAPI document representing the API specification.</param>
        /// <returns>An <see cref="ApiParameterModel"/> containing login API details, or null if not found.</returns>
        private static ApiParameterModel? FindLoginEndpoint(OpenApiDocument openApiDocument)
        {
            // Iterates through each path in the OpenAPI document
            foreach (var path in openApiDocument.Paths)
            {
                // Iterates through each operation in the current path
                foreach (var operation in path.Value.Operations)
                {
                    // Checks if the operation summary, description, or path contains "login"
                    if (operation.Value.Summary?.ToLower().Contains("login", StringComparison.CurrentCultureIgnoreCase) == true ||
                        operation.Value.Description?.ToLower().Contains("login", StringComparison.CurrentCultureIgnoreCase) == true ||
                        path.Key.Contains("login", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Creates and returns a new API parameter model for the login endpoint
                        return new ApiParameterModel
                        {
                            // Sets the endpoint path
                            Name = path.Key,
                            // Sets the endpoint path as action URL
                            ActionUrl = path.Key,
                            // Sets the HTTP method
                            Method = operation.Key.ToString().ToUpper(),
                            // Extracts parameters from the operation
                            Parameters = ExtractParameters(operation.Value.Parameters),
                            // Extracts request body from the operation
                            RequestBody = ExtractRequestBody(operation.Value.RequestBody)
                        };
                    }
                }
            }

            // Returns null if no login endpoint is found
            return null;
        }

        /// <summary>
        /// Fetches and parses the Swagger document from the given URL.
        /// </summary>
        /// <param name="swaggerUrl">The URL of the Swagger API documentation.</param>
        /// <returns>An <see cref="OpenApiDocument"/> object representing the parsed API specification.</returns>
        /// <exception cref="Exception">Thrown when there is an error fetching or parsing the Swagger file.</exception>
        private async Task<OpenApiDocument> FetchAndParseSwaggerAsync(string swaggerUrl)
        {
            try
            {
                // Fetches the Swagger content from the URL
                var swaggerContent = await _httpClient.GetStringAsync(swaggerUrl);
                // Creates a new OpenAPI string reader
                var openApiReader = new OpenApiStringReader();
                // Reads the Swagger content into an OpenAPI document
                var openApiDocument = openApiReader.Read(swaggerContent, out var diagnostic);

                // Returns the parsed OpenAPI document
                return openApiDocument;
            }
            catch (Exception ex)
            {
                // Throws a new exception with a descriptive message if fetching or parsing fails
                throw new Exception($"Error fetching or parsing Swagger file: {ex.Message}");
            }
        }
        #endregion
    }
}
