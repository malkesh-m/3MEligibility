//using System.Text;
//using System.Text.Json;
//using System.Xml.Linq;
//using AutoMapper;
//using MEligibilityPlatform.Application.Services.Interface;
//using MEligibilityPlatform.Application.UnitOfWork;
//using MEligibilityPlatform.Domain.Models;
//using Microsoft.OpenApi;
//using Microsoft.OpenApi.Models;
//using Microsoft.OpenApi.Readers;
//using Newtonsoft.Json.Linq;

//namespace MEligibilityPlatform.Application.Services
//{
//    /// <summary>
//    /// Service for integrating with APIs, supporting data fetching, Swagger parsing, and request execution.
//    /// </summary>
//    /// <remarks>
//    /// Initializes a new instance of the <see cref="ApiIntegrationService"/> class.
//    /// </remarks>
//    /// <param name="uow">The unit of work instance.</param>
//    /// <param name="httpClient">The HTTP client instance.</param>
//    /// <param name="mapper">The AutoMapper instance.</param>
//    /// <param name="httpClientFactory">The HTTP client factory instance.</param>
//    public class ApiIntegrationService(IUnitOfWork uow, HttpClient httpClient, IMapper mapper, IHttpClientFactory httpClientFactory) : IApiIntegrationService
//    {
//        private readonly IUnitOfWork _uow = uow;
//        private readonly HttpClient _httpClient = httpClient;
//        private readonly IMapper _mapper = mapper;
//        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;


//        #region Rest API Call

//        //public async Task<List<ApiParameterModel>> GetRestApiDetailsAsync(string ApiUrl)
//        //{
//        //    try
//        //    {
//        //        var swaggerContent = await FetchSwaggerContentAsync(ApiUrl);
//        //        var openApiDocument = ParseSwaggerContent(swaggerContent);
//        //        var ListOfApi = ExtractApiEndpoints(openApiDocument);
//        //        return ListOfApi;

//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        throw new Exception($"Error fetching or parsing Swagger file: {ex.Message}");
//        //    }
//        //}


//        //private async Task InsertIntoDbAsync(List<ApiParameterModel> apiList, int nodeId)
//        //{
//        //    if (apiList == null || !apiList.Any()) return;

//        //    List<NodeApiCreateOrUpdateModel> nodeApilist = new List<NodeApiCreateOrUpdateModel>();
//        //    List<ApiParametersCreateUpdateModel> apiParameterList = new List<ApiParametersCreateUpdateModel>();
//        //    List<ApiResponsesCreateUpdateModel> apiResponseList = new List<ApiResponsesCreateUpdateModel>();

//        //    foreach (var api in apiList)
//        //    {
//        //        nodeApilist.Add(new NodeApiCreateOrUpdateModel
//        //        {
//        //            Apiname = api.Name,
//        //            HttpMethodType = api.ActionUrl,
//        //            NodeId = nodeId
//        //        });
//        //    }

//        //    var nodeApiEntities = _mapper.Map<List<NodeApi>>(nodeApilist);
//        //    _uow.NodeApiRepository.AddRange(nodeApiEntities);
//        //    await _uow.CompleteAsync();  // Ensure data is committed

//        //    var savedNodeApis = _uow.NodeApiRepository.Query().Where(ni => ni.NodeId == nodeId).ToList();

//        //    foreach (var api in apiList)
//        //    {
//        //        var apiDetail = savedNodeApis.FirstOrDefault(x => x.Apiname == api.Name && x.HttpMethodType == api.ActionUrl);

//        //        if (apiDetail != null)
//        //        {
//        //            apiParameterList.AddRange(api.Parameters.Select(param => new ApiParametersCreateUpdateModel
//        //            {
//        //                ApiId = apiDetail.Apiid,
//        //                ParameterName = param.Name,
//        //                ParameterType = param.Type,
//        //                IsRequired = param.Required,
//        //                ParameterDirection = "Input"
//        //            }));

//        //            apiParameterList.AddRange(api.OutputParameters.Select(param => new ApiParametersCreateUpdateModel
//        //            {
//        //                ApiId = apiDetail.Apiid,
//        //                ParameterName = param.Name,
//        //                ParameterType = param.Type,
//        //                IsRequired = param.Required,
//        //                ParameterDirection = "Output"
//        //            }));

//        //            apiResponseList.AddRange(api.Responses.Select(responce => new ApiResponsesCreateUpdateModel
//        //            {
//        //                ApiId = apiDetail.Apiid,
//        //                ResponceCode = responce.Key,
//        //                ResponceSchema = responce.Value
//        //            }));
//        //        }
//        //    }

//        //    _uow.ApiParametersRepository.AddRange(_mapper.Map<List<ApiParameters>>(apiParameterList));
//        //    _uow.ApiResponsesRepository.AddRange(_mapper.Map<List<ApiResponses>>(apiResponseList));
//        //    await _uow.CompleteAsync();  // Save parameters and responses
//        //}
//        /// <summary>
//        /// Shared JSON serializer options instance for handling circular references with indented formatting.
//        /// </summary>
//        private static readonly JsonSerializerOptions _jsonOptions = new()
//        {
//            // Sets reference handler to preserve object references during serialization
//            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
//            // Formats JSON output with indentation for readability
//            WriteIndented = true
//        };

//        /// <summary>
//        /// Gets API details from the specified URL, supporting Swagger, JSON, and XML formats.
//        /// </summary>
//        /// <param name="apiUrl">The API URL to fetch details from.</param>
//        /// <returns>A task representing the asynchronous operation, with the API details as an object.</returns>
//        public async Task<object> GetApiDetailsAsync(string apiUrl)
//        {
//            try
//            {
//                // Fetches API content from the specified URL
//                var content = await FetchApiContentAsync(apiUrl);

//                // Determines content type and processes accordingly
//                if (IsSwaggerJson(content))
//                {
//                    // Parses Swagger content and extracts endpoints
//                    var openApiDocument = ParseSwaggerContent(content);
//                    return ExtractApiEndpoints(openApiDocument);
//                }
//                else if (IsJson(content))
//                {


//                    // Deserializes JSON content using pre-configured options
//                    var result = JsonSerializer.Deserialize<object>(content, _jsonOptions);
//                    // Returns deserialized result or new object if null
//                    return result ?? new object();
//                }
//                else if (IsXml(content))
//                {
//                    // Parses XML content
//                    return XDocument.Parse(content);
//                }
//                else
//                {
//                    // Returns raw content for unsupported formats
//                    return content;
//                }
//            }
//            catch (Exception ex)
//            {
//                // Throws exception with error message
//                throw new Exception($"Error fetching API details: {ex.Message}");
//            }
//        }

//        /// <summary>
//        /// Determines if the provided content is JSON.
//        /// </summary>
//        /// <param name="content">The content string to check.</param>
//        /// <returns>True if the content is JSON; otherwise, false.</returns>
//        private static bool IsJson(string content)
//        {
//            // Checks if content starts with JSON object or array syntax
//            return content.TrimStart().StartsWith('{') || content.TrimStart().StartsWith('[');
//        }

//        /// <summary>
//        /// Determines if the provided content is XML.
//        /// </summary>
//        /// <param name="content">The content string to check.</param>
//        /// <returns>True if the content is XML; otherwise, false.</returns>
//        private static bool IsXml(string content)
//        {
//            // Checks if content starts with XML tag syntax
//            return content.TrimStart().StartsWith('<');
//        }

//        /// <summary>
//        /// Fetches API content from the specified URL.
//        /// </summary>
//        /// <param name="apiUrl">The API URL to fetch content from.</param>
//        /// <returns>A task representing the asynchronous operation, with the content as a string.</returns>
//        private async Task<string> FetchApiContentAsync(string apiUrl)
//        {
//            // Uses HTTP client to fetch content from URL
//            return await _httpClient.GetStringAsync(apiUrl);
//        }

//        /// <summary>
//        /// Determines if the provided JSON content is a Swagger/OpenAPI document.
//        /// </summary>
//        /// <param name="jsonContent">The JSON content to check.</param>
//        /// <returns>True if the content is Swagger/OpenAPI; otherwise, false.</returns>
//        private static bool IsSwaggerJson(string jsonContent)
//        {
//            // Checks for Swagger/OpenAPI specific identifiers
//            return jsonContent.Contains("\"openapi\"") || jsonContent.Contains("\"swagger\"");
//        }

//        /// <summary>
//        /// Parses Swagger content into an OpenApiDocument.
//        /// </summary>
//        /// <param name="swaggerContent">The Swagger content as a string.</param>
//        /// <returns>The parsed OpenApiDocument.</returns>
//        private static OpenApiDocument ParseSwaggerContent(string swaggerContent)
//        {
//            // Creates OpenAPI string reader
//            var openApiReader = new OpenApiStringReader();
//            // Reads and parses Swagger content
//            var openApiDocument = openApiReader.Read(swaggerContent, out _);

//            // Returns parsed OpenAPI document
//            return openApiDocument;
//        }

//        /// <summary>
//        /// Extracts API endpoints from an OpenApiDocument.
//        /// </summary>
//        /// <param name="openApiDocument">The OpenApiDocument to extract endpoints from.</param>
//        /// <returns>A list of ApiParameterModel representing the API endpoints.</returns>
//        private static List<ApiParameterModel> ExtractApiEndpoints(OpenApiDocument openApiDocument)
//        {
//            // Initializes list for API endpoints
//            var apiEndpoints = new List<ApiParameterModel>();

//            // Iterates through each path in the OpenAPI document
//            foreach (var path in openApiDocument.Paths)
//            {
//                // Iterates through each operation in the path
//                foreach (var operation in path.Value.Operations)
//                {
//                    // Creates API parameter model for each endpoint
//                    var endpoint = new ApiParameterModel
//                    {
//                        Name = path.Key,
//                        ActionUrl = operation.Key.ToString().ToUpper(),
//                        Parameters = ExtractParameters(operation.Value.Parameters),
//                        RequestBody = ExtractRequestBody(operation.Value.RequestBody),
//                        Responses = ExtractResponses(operation.Value.Responses),
//                        OutputParameters = ExtractOutputParameters(operation.Value.Responses),
//                        Headers = ExtractHeaders(operation.Value.Parameters, operation.Value.Responses)
//                    };

//                    // Adds endpoint to the list
//                    apiEndpoints.Add(endpoint);
//                }
//            }

//            // Returns the list of API endpoints
//            return apiEndpoints;
//        }

//        /// <summary>
//        /// Extracts headers from parameters and responses.
//        /// </summary>
//        /// <param name="parameters">The list of OpenApiParameter objects.</param>
//        /// <param name="responses">The OpenApiResponses object.</param>
//        /// <returns>A list of ApiHeaderModel representing the headers.</returns>
//        private static List<ApiHeaderModel> ExtractHeaders(IList<OpenApiParameter> parameters, OpenApiResponses responses)
//        {
//            // Initializes list for headers
//            var headers = new List<ApiHeaderModel>();

//            // Extracts request headers from parameters
//            foreach (var parameter in parameters.Where(p => p.In == ParameterLocation.Header))
//            {
//                headers.Add(new ApiHeaderModel
//                {
//                    Name = parameter.Name,
//                    Description = parameter.Description,
//                    Required = parameter.Required
//                });
//            }

//            // Extracts response headers from responses
//            foreach (var response in responses)
//            {
//                foreach (var header in response.Value.Headers)
//                {
//                    headers.Add(new ApiHeaderModel
//                    {
//                        Name = header.Key,
//                        Description = header.Value.Description,
//                        Required = false
//                    });
//                }
//            }

//            // Returns the list of headers
//            return headers;
//        }

//        /// <summary>
//        /// Extracts parameters from a list of OpenApiParameter objects.
//        /// </summary>
//        /// <param name="parameters">The list of OpenApiParameter objects.</param>
//        /// <returns>A list of ParameterDetail representing the parameters.</returns>
//        private static List<ParameterDetail> ExtractParameters(IList<OpenApiParameter> parameters)
//        {
//            // Maps OpenAPI parameters to parameter details
//            return [.. parameters.Select(parameter => new ParameterDetail
//            {
//                Name = parameter.Name,
//                Type = parameter.Schema?.Type ?? "string",
//                Required = parameter.Required
//            })];
//        }

//        /// <summary>
//        /// Extracts output parameters from API responses.
//        /// </summary>
//        /// <param name="responses">The dictionary of API responses.</param>
//        /// <returns>A list of OutputParameterDetail representing the output parameters.</returns>
//        private static List<OutputParameterDetail> ExtractOutputParameters(IDictionary<string, OpenApiResponse> responses)
//        {
//            // Initializes lists for output parameters and unique parameter tracking
//            var outputParameters = new List<OutputParameterDetail>();
//            var uniqueParameters = new HashSet<string>();

//            // Iterates through each response
//            foreach (var response in responses)
//            {
//                // Processes response content if available
//                if (response.Value.Content != null)
//                {
//                    foreach (var content in response.Value.Content)
//                    {
//                        // Processes schema properties if available
//                        if (content.Value.Schema?.Properties != null)
//                        {
//                            foreach (var property in content.Value.Schema.Properties)
//                            {
//                                // Creates unique key for parameter
//                                string uniqueKey = $"{property.Key}-{property.Value.Type}";
//                                // Adds parameter if not already processed
//                                if (!uniqueParameters.Contains(uniqueKey))
//                                {
//                                    outputParameters.Add(new OutputParameterDetail
//                                    {
//                                        Name = property.Key,
//                                        Type = property.Value.Type ?? "Unknown",
//                                        Required = content.Value.Schema.Required?.Contains(property.Key) ?? false
//                                    });

//                                    // Adds parameter to unique set
//                                    uniqueParameters.Add(uniqueKey);
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // Returns the list of output parameters
//            return outputParameters;
//        }

//        /// <summary>
//        /// Extracts the request body from an OpenApiRequestBody object.
//        /// </summary>
//        /// <param name="requestBody">The OpenApiRequestBody object.</param>
//        /// <returns>The extracted ApiRequestBody object.</returns>
//        private static ApiRequestBody? ExtractRequestBody(OpenApiRequestBody requestBody)
//        {
//            // Returns null if request body is not provided
//            if (requestBody == null)
//            {
//                return null;
//            }

//            // Initializes list for request body parameters
//            var requestBodyParameters = new List<ParameterDetail>();

//            // Processes request body content
//            foreach (var content in requestBody.Content)
//            {
//                // Extracts parameters from JSON schema
//                if (content.Key == "application/json" && content.Value.Schema != null)
//                {
//                    ExtractSchemaParameters(content.Value.Schema, requestBodyParameters);
//                }
//            }

//            // Returns request body details
//            return new ApiRequestBody
//            {
//                Description = requestBody.Description,
//                ContentType = "application/json",
//                Parameters = requestBodyParameters
//            };
//        }

//        /// <summary>
//        /// Recursively extracts schema parameters from an OpenApiSchema object.
//        /// </summary>
//        /// <param name="schema">The OpenApiSchema object.</param>
//        /// <param name="parameters">The list to populate with ParameterDetail objects.</param>
//        /// <param name="parentName">The parent property name (for nested objects).</param>
//        private static void ExtractSchemaParameters(OpenApiSchema schema, List<ParameterDetail> parameters, string parentName = "")
//        {
//            // Processes schema properties if available
//            if (schema.Properties != null)
//            {
//                foreach (var property in schema.Properties)
//                {
//                    // Creates parameter name with parent prefix for nested properties
//                    var parameterName = string.IsNullOrEmpty(parentName) ? property.Key : $"{parentName}.{property.Key}";
//                    // Adds parameter to the list
//                    parameters.Add(new ParameterDetail
//                    {
//                        Name = parameterName,
//                        Type = property.Value.Type ?? "object",
//                        Required = schema.Required.Contains(property.Key)
//                    });

//                    // Recursively processes nested object properties
//                    if (property.Value.Type == "object" || property.Value.Properties != null)
//                    {
//                        ExtractSchemaParameters(property.Value, parameters, parameterName);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Extracts response details from OpenApiResponses.
//        /// </summary>
//        /// <param name="responses">The OpenApiResponses object.</param>
//        /// <returns>A dictionary mapping status codes to response descriptions.</returns>
//        private static Dictionary<string, string> ExtractResponses(OpenApiResponses responses)
//        {
//            // Initializes dictionary for response details
//            var responseDetails = new Dictionary<string, string>();

//            // Processes each response
//            foreach (var response in responses)
//            {
//                // Extracts status code and description
//                string statusCode = response.Key;
//                string description = response.Value.Description;

//                // Adds response type information if content is available
//                if (response.Value.Content.Count > 0)
//                {
//                    var content = response.Value.Content.First();
//                    var schema = content.Value.Schema;

//                    string responseType = schema.Type ?? "Unknown";
//                    responseDetails[statusCode] = $"{description} (Type: {responseType})";
//                }
//                else
//                {
//                    responseDetails[statusCode] = description;
//                }
//            }

//            // Returns the dictionary of response details
//            return responseDetails;
//        }

//        /// <summary>
//        /// Extracts schema properties from an OpenApiSchema object.
//        /// </summary>
//        /// <param name="schema">The OpenApiSchema object.</param>
//        /// <returns>A list of ApiParameterModel representing the schema properties.</returns>
//        private static List<ApiParameterModel> ExtractSchemaProperties(OpenApiSchema schema)
//        {
//            // Initializes list for schema properties
//            var properties = new List<ApiParameterModel>();

//            // Processes schema properties if available
//            if (schema.Properties != null)
//            {
//                foreach (var property in schema.Properties)
//                {
//                    // Adds property to the list
//                    properties.Add(new ApiParameterModel
//                    {
//                        Name = property.Key,
//                        ActionUrl = property.Value.Type,
//                    });
//                }
//            }

//            // Returns the list of schema properties
//            return properties;
//        }
//        #endregion
//        //public async Task<ApiAnalysisResult> AnalyzeApiAsync(string apiUrl, HttpMethod method, object? requestBody = null)
//        //{
//        //    try
//        //    {
//        //        using var request = new HttpRequestMessage(method, apiUrl);

//        //        // Add request body for POST, PUT, etc.
//        //        if (requestBody != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
//        //        {
//        //            string jsonContent = JsonSerializer.Serialize(requestBody);
//        //            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
//        //        }

//        //        // Measure response time
//        //        var stopwatch = Stopwatch.StartNew();
//        //        using var response = await _httpClient.SendAsync(request);
//        //        stopwatch.Stop();

//        //        // Read response
//        //        string responseContent = await response.Content.ReadAsStringAsync();

//        //        // Parse JSON if applicable
//        //        object? parsedContent = null;
//        //        try
//        //        {
//        //            parsedContent = JsonSerializer.Deserialize<object>(responseContent);
//        //        }
//        //        catch { /* Ignore if not JSON */ }

//        //        return new ApiAnalysisResult
//        //        {
//        //            Url = apiUrl,
//        //            Method = method.Method,
//        //            StatusCode = (int)response.StatusCode,
//        //            ResponseTimeMs = stopwatch.ElapsedMilliseconds,
//        //            Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.FirstOrDefault()),
//        //            ResponseBody = parsedContent ?? responseContent
//        //        };
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        throw new Exception($"Error calling API: {ex.Message}");
//        //    }
//        //}

//        /// <summary>
//        /// Executes an API request based on the provided configuration and parameters.
//        /// </summary>
//        /// <param name="apiConfig">The node API configuration.</param>
//        /// <param name="mappingConfig">The node API mapping configuration.</param>
//        /// <param name="parameters">The request parameters.</param>
//        /// <returns>A task representing the asynchronous operation, with the response as a dynamic object.</returns>
//        public async Task<dynamic> ExecuteRequestAsync(NodeListModel apiConfig, NodeApiListModel mappingConfig, Dictionary<string, object>? parameters = null)
//        {
//            try
//            {
//                // Creates HTTP client
//                var client = _httpClientFactory.CreateClient();
//                // Sets base address from API configuration
//                client.BaseAddress = new Uri(apiConfig.NodeUrl!);

//                // Adds default headers from API configuration
//                if (!string.IsNullOrEmpty(apiConfig.Headers))
//                {
//                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(apiConfig.Headers);
//                    foreach (var header in headers!)
//                    {
//                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
//                    }
//                }

//                // Creates request message with method and endpoint path
//                var httpMethod = mappingConfig.HttpMethodType ?? "GET";
//                var request = new HttpRequestMessage(new HttpMethod(httpMethod), mappingConfig.EndpointPath);
//                // Adds request parameters based on HTTP method
//                if (parameters != null && parameters.Count > 0)
//                {
//                    if (mappingConfig.HttpMethodType!.Equals("GET", StringComparison.OrdinalIgnoreCase))
//                    {
//                        // For GET requests, add parameters to query string
//                        var uriBuilder = new UriBuilder(apiConfig.NodeUrl + mappingConfig.EndpointPath);
//                        var query = new StringBuilder();
//                        foreach (var param in parameters)
//                        {
//                            if (query.Length > 0)
//                                query.Append('&');
//                            query.Append($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value?.ToString() ?? string.Empty)}");
//                        }
//                        uriBuilder.Query = query.ToString();
//                        request.RequestUri = uriBuilder.Uri;
//                    }
//                    else
//                    {
//                        // For POST/PUT/DELETE requests, add parameters to body
//                        var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
//                        request.Content = content;
//                    }
//                }
//                else if (!string.IsNullOrEmpty(mappingConfig.RequestBody) &&
//                         (mappingConfig.HttpMethodType!.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
//                          mappingConfig.HttpMethodType.Equals("PUT", StringComparison.OrdinalIgnoreCase)))
//                {
//                    // Use template request body if provided and no parameters are specified
//                    var content = new StringContent(mappingConfig.RequestBody, Encoding.UTF8, "application/json");
//                    request.Content = content;
//                }

//                // Executes request with retry logic
//                HttpResponseMessage? response = null;
//                int retryCount = 0;
//                bool success = false;

//                while (!success && retryCount <= apiConfig.RetryCount)
//                {
//                    try
//                    {
//                        // Sends HTTP request
//                        response = await client.SendAsync(request);
//                        success = true;
//                    }
//                    catch (Exception)
//                    {
//                        retryCount++;
//                        if (retryCount > apiConfig.RetryCount)
//                        {
//                            // Throws exception if max retries exceeded
//                            throw;
//                        }
//                    }
//                }

//                // Ensures successful response status code
//                response!.EnsureSuccessStatusCode();
//                // Reads response content
//                var responseContent = await response.Content.ReadAsStringAsync();

//                // Parses response as dynamic object
//                dynamic responseData;
//                if (response.Content.Headers.ContentType?.MediaType == "application/json")
//                {
//                    // Parses JSON response
//                    responseData = JObject.Parse(responseContent);

//                    // Extracts data from specified response root path if configured
//                    if (!string.IsNullOrEmpty(mappingConfig.ResponseRootPath))
//                    {
//                        responseData = ((JObject)responseData).SelectToken(mappingConfig.ResponseRootPath) ?? responseData;
//                    }
//                }
//                else
//                {
//                    // Creates simple object for non-JSON responses
//                    var obj = new JObject
//                    {
//                        ["content"] = responseContent
//                    };
//                    responseData = obj;
//                }

//                // Returns response data
//                return responseData;
//            }
//            catch (Exception)
//            {
//                // Throws exception with error details
//                throw;
//            }
//        }
//        public async Task<TestApiResponse> TestApiAsync(TestApiRequest request)
//        {
//            // Replace this block in TestApiAsync:
//            // var mappedParams = await _uow.ApiParameterMapsRepository
//            //     .GetAll()
//            //     .Include(x => x.ApiParameter)
//            //     .Include(x => x.Parameter)
//            //     .Where(x => x.ApiParameter.ApiId == request.ApiId)
//            //     .Select(x => new ApiParameterMapDto
//            //     {
//            //         ApiParamName = x.ApiParameter.ParameterName,
//            //         LocalParamName = x.Parameter.ParameterName,
//            //         Direction = x.ApiParameter.ParameterDirection
//            //     })
//            //     .ToListAsync();

//            // With the following code:
//            var mappedParams = _uow.ApiParameterMapsRepository
//                .Query()
//                .Where(x => x.ApiParameter.ApiId == request.ApiId)
//                .Select(x => new ApiParameterMapDto
//                {
//                    ApiParamName = x.ApiParameter.ParameterName,
//                    LocalParamName = x.Parameter.ParameterName!,
//                    Direction = x.ApiParameter.ParameterDirection
//                })
//                .ToList();

//            // 2️⃣ Deserialize input JSON from frontend
//            var inputDict = JsonSerializer.Deserialize<Dictionary<string, object>>(request.InputJson) ?? [];

//            // 3️⃣ Map input parameters according to mapping
//            var apiInput = new Dictionary<string, object>();
//            foreach (var map in mappedParams.Where(m => m.Direction.Equals("Input", StringComparison.OrdinalIgnoreCase)))
//            {
//                if (inputDict.TryGetValue(map.LocalParamName, out object? value))
//                    apiInput[map.ApiParamName] = value;
//            }

//            // 4️⃣ Call external API
//            using var httpClient = new HttpClient();
//            HttpResponseMessage response;

//            if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
//            {
//                var finalBody = JsonSerializer.Serialize(apiInput);
//                response = await httpClient.PostAsync(request.FullUrl, new StringContent(finalBody, Encoding.UTF8, "application/json"));
//            }
//            else if (request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
//            {
//                var queryString = string.Join("&", apiInput.Select(p => $"{p.Key}={p.Value}"));
//                response = await httpClient.GetAsync($"{request.FullUrl}?{queryString}");
//            }
//            else
//            {
//                throw new InvalidOperationException("Unsupported HTTP Method");
//            }

//            var apiResponseString = await response.Content.ReadAsStringAsync();
//            var apiResponseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(apiResponseString) ?? [];

//            // 5️⃣ Map output parameters according to mapping
//            var mappedOutput = new Dictionary<string, object>();
//            foreach (var map in mappedParams.Where(m => m.Direction.Equals("Output", StringComparison.OrdinalIgnoreCase)))
//            {
//                if (apiResponseDict.TryGetValue(map.ApiParamName, out object? value))
//                    mappedOutput[map.LocalParamName] = value;
//            }

//            // 6️⃣ Optionally log test API call in DB


//            return new TestApiResponse
//            {
//                InputUsed = apiInput,
//                RawApiResponse = apiResponseString,
//                MappedOutput = mappedOutput
//            };
//        }
//    }

//    public class ApiParameterMapDto
//    {
//        public string ApiParamName { get; set; } = string.Empty;
//        public string LocalParamName { get; set; } = string.Empty;
//        public string Direction { get; set; } = string.Empty;
//    }

//    public class TestApiResponse
//    {
//        public Dictionary<string, object> InputUsed { get; set; } = [];
//        public string RawApiResponse { get; set; } = string.Empty;
//        public Dictionary<string, object> MappedOutput { get; set; } = [];
//    }


//}
