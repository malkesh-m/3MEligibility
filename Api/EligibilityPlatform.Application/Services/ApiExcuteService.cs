//using AutoMapper;
//using MEligibilityPlatform.Application.Services.Inteface;
//using MEligibilityPlatform.Application.UnitOfWork;
//using MEligibilityPlatform.Domain.Entities;
//using MEligibilityPlatform.Domain.Models;
//using Microsoft.OpenApi.Models;
//using Microsoft.OpenApi.Readers;

//namespace MEligibilityPlatform.Application.Services
//{
//    /// <summary>
//    /// Provides services for executing API calls and processing Swagger file details.
//    /// </summary>
//    /// <remarks>
//    /// Initializes a new instance of the <see cref="ApiExcuteService"/> class.
//    /// </remarks>
//    /// <param name="uow">The unit of work.</param>
//    /// <param name="httpClient">The HTTP client.</param>
//    /// <param name="mapper">The AutoMapper instance.</param>
//    public class ApiExcuteService(IUnitOfWork uow, HttpClient httpClient, IMapper mapper) : IApiExcuteService
//    {
//        private readonly IUnitOfWork _uow = uow;
//        private readonly HttpClient _httpClient = httpClient;
//        private readonly IMapper _mapper = mapper;

//        #region Rest API Call

//        /// <summary>
//        /// Fetches and processes API details from a Swagger file based on the provided node ID.
//        /// </summary>
//        /// <param name="nodeId">The ID of the node from which to retrieve API details.</param>
//        /// <returns>A task representing an asynchronous operation that returns a list of API parameters.</returns>
//        /// <exception cref="ArgumentException">Thrown when the Swagger URL is null or empty.</exception>
//        /// <exception cref="Exception">Thrown when an error occurs while fetching or parsing the Swagger file.</exception>
//        public async Task<List<ApiParameterModel>> GetRestApiDetailsAsync(int nodeId)
//        {
//            // Retrieves node by ID from repository
//            var node = _uow.NodeModelRepository.GetById(nodeId);

//            // Validates that node URL is not null or empty
//            if (string.IsNullOrEmpty(node.NodeUrl))
//                throw new ArgumentException("Swagger URL cannot be null or empty.", nameof(nodeId));

//            try
//            {
//                // Fetches Swagger content from the URL
//                var swaggerContent = await FetchSwaggerContentAsync(node.NodeUrl);
//                // Parses the Swagger content into OpenAPI document
//                var openApiDocument = ParseSwaggerContent(swaggerContent);
//                // Extracts API endpoints from the document
//                var ListOfApi = ExtractApiEndpoints(openApiDocument);
//                // Inserts API details into database
//                await InsertIntoDbAsync(ListOfApi, nodeId);
//                // Returns the list of API parameters
//                return ListOfApi;
//            }
//            catch (Exception ex)
//            {
//                // Throws exception with error message
//                throw new Exception($"Error fetching or parsing Swagger file: {ex.Message}");
//            }
//        }

//        /// <summary>
//        /// Inserts API details, parameters, and responses into the database.
//        /// </summary>
//        /// <param name="apiList">The list of API details to be inserted.</param>
//        /// <param name="nodeId">The ID of the node associated with the API details.</param>
//        /// <returns>A task representing the asynchronous operation.</returns>
//        private async Task InsertIntoDbAsync(List<ApiParameterModel> apiList, int nodeId)
//        {
//            // Returns if API list is null or empty
//            if (apiList == null || apiList.Count == 0) return;

//            // Initializes lists for node APIs, parameters, and responses
//            List<NodeApiCreateOrUpdateModel> nodeApilist = [];
//            List<ApiParametersCreateUpdateModel> apiParameterList = [];
//            List<ApiResponsesCreateUpdateModel> apiResponseList = [];

//            // Populates node API list from API details
//            foreach (var api in apiList)
//            {
//                nodeApilist.Add(new NodeApiCreateOrUpdateModel
//                {
//                    Apiname = api.Name,
//                    HttpMethodType = api.ActionUrl,
//                    NodeId = nodeId
//                });
//            }

//            // Maps node API models to entities
//            var nodeApiEntities = _mapper.Map<List<NodeApi>>(nodeApilist);
//            // Adds node API entities to repository
//            _uow.NodeApiRepository.AddRange(nodeApiEntities);
//            // Completes the unit of work transaction
//            await _uow.CompleteAsync();

//            // Retrieves saved node APIs for the given node ID
//            var savedNodeApis = _uow.NodeApiRepository.Query().Where(ni => ni.NodeId == nodeId).ToList();

//            // Populates parameter and response lists for each API
//            foreach (var api in apiList)
//            {
//                // Finds the corresponding saved API detail
//                var apiDetail = savedNodeApis.FirstOrDefault(x => x.Apiname == api.Name && x.HttpMethodType == api.ActionUrl);

//                if (apiDetail != null)
//                {
//                    // Adds input parameters to parameter list
//                    apiParameterList.AddRange(api.Parameters.Select(param => new ApiParametersCreateUpdateModel
//                    {
//                        ApiId = apiDetail.Apiid,
//                        ParameterName = param.Name,
//                        ParameterType = param.Type,
//                        IsRequired = param.Required,
//                        ParameterDirection = "Input"
//                    }));

//                    // Adds output parameters to parameter list
//                    apiParameterList.AddRange(api.OutputParameters.Select(param => new ApiParametersCreateUpdateModel
//                    {
//                        ApiId = apiDetail.Apiid,
//                        ParameterName = param.Name,
//                        ParameterType = param.Type,
//                        IsRequired = param.Required,
//                        ParameterDirection = "Output"
//                    }));

//                    // Adds responses to response list
//                    apiResponseList.AddRange(api.Responses.Select(responce => new ApiResponsesCreateUpdateModel
//                    {
//                        ApiId = apiDetail.Apiid,
//                        ResponceCode = responce.Key,
//                        ResponceSchema = responce.Value
//                    }));
//                }
//            }

//            // Maps and adds parameters to repository
//            _uow.ApiParametersRepository.AddRange(_mapper.Map<List<ApiParameter>>(apiParameterList));
//            // Maps and adds responses to repository
//            _uow.ApiResponsesRepository.AddRange(_mapper.Map<List<ApiResponse>>(apiResponseList));
//            // Completes the unit of work transaction
//            await _uow.CompleteAsync();
//        }

//        /// <summary>
//        /// Fetches the Swagger content from the given URL.
//        /// </summary>
//        /// <param name="swaggerUrl">The URL of the Swagger API definition.</param>
//        /// <returns>A task representing the asynchronous operation that returns the Swagger content as a string.</returns>
//        /// <exception cref="HttpRequestException">Thrown if the request fails due to network issues or an invalid URL.</exception>
//        private async Task<string> FetchSwaggerContentAsync(string swaggerUrl)
//        {
//            // Fetches Swagger content using HTTP client
//            return await _httpClient.GetStringAsync(swaggerUrl);
//        }

//        /// <summary>
//        /// Parses the Swagger content and converts it into an OpenAPI document.
//        /// </summary>
//        /// <param name="swaggerContent">The Swagger JSON or YAML content as a string.</param>
//        /// <returns>An <see cref="OpenApiDocument"/> representing the parsed Swagger definition.</returns>
//        /// <exception cref="OpenApiException">Thrown if the Swagger content is invalid or cannot be parsed.</exception>
//        private static Microsoft.OpenApi.OpenApiDocument ParseSwaggerContent(string swaggerContent)
//        {
//            // Creates OpenAPI string reader
//            var openApiReader = new OpenApiStringReader();
//            // Reads and parses Swagger content
//            var openApiDocument = openApiReader.Read(swaggerContent, out _);

//            // Returns parsed OpenAPI document
//            return openApiDocument;
//        }

//        /// <summary>
//        /// Extracts API endpoints from an OpenAPI document.
//        /// </summary>
//        /// <param name="openApiDocument">The parsed OpenAPI document containing API definitions.</param>
//        /// <returns>A list of <see cref="ApiParameterModel"/> representing the extracted API endpoints.</returns>
//        private static List<ApiParameterModel> ExtractApiEndpoints(Microsoft.OpenApi.OpenApiDocument openApiDocument)
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
//        /// Extracts request and response headers from OpenAPI parameters and responses.
//        /// </summary>
//        /// <param name="parameters">A list of <see cref="OpenApiParameter"/> objects representing request parameters.</param>
//        /// <param name="responses">An <see cref="OpenApiResponses"/> object containing API response details.</param>
//        /// <returns>A list of <see cref="ApiHeaderModel"/> representing the extracted headers.</returns>
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
//        /// Extracts parameter details from a list of OpenAPI parameters.
//        /// </summary>
//        /// <param name="parameters">A list of <see cref="OpenApiParameter"/> objects representing API request parameters.</param>
//        /// <returns>A list of <see cref="ParameterDetail"/> containing parameter names, types, and required flags.</returns>
//        private static List<ParameterDetail> ExtractParameters(IList<OpenApiParameter> parameters) =>
//            // Maps OpenAPI parameters to parameter details
//            [.. parameters.Select(parameter => new ParameterDetail
//            {
//                Name = parameter.Name,
//                Type = parameter.Schema?.Type ?? "string",
//                Required = parameter.Required
//            })];

//        /// <summary>
//        /// Extracts unique output parameters from API responses.
//        /// </summary>
//        /// <param name="responses">A dictionary of <see cref="OpenApiResponse"/> objects representing API responses.</param>
//        /// <returns>A list of <see cref="OutputParameterDetail"/> containing output parameter names, types, and required flags.</returns>
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
//        /// Extracts request body details from an OpenAPI request body definition.
//        /// </summary>
//        /// <param name="requestBody">An <see cref="OpenApiRequestBody"/> object containing request body details.</param>
//        /// <returns>An <see cref="ApiRequestBody"/> object containing request body description, content type, and extracted parameters.
//        /// Returns null if the request body is not provided.</returns>
//        private static ApiRequestBody? ExtractRequestBody(Microsoft.OpenApi.OpenApiRequestBody requestBody)
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
//        /// Recursively extracts parameters from an OpenAPI schema and adds them to the provided list.
//        /// </summary>
//        /// <param name="schema">The <see cref="OpenApiSchema"/> containing the parameter definitions.</param>
//        /// <param name="parameters">A list of <see cref="ParameterDetail"/> where extracted parameters will be stored.</param>
//        /// <param name="parentName">Optional. A prefix for nested properties to represent hierarchy.</param>
//        private static void ExtractSchemaParameters(Microsoft.OpenApi.OpenApiSchema schema, List<ParameterDetail> parameters, string parentName = "")
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
//        /// Extracts response details from an OpenAPI response object and returns them as a dictionary.
//        /// </summary>
//        /// <param name="responses">The OpenAPI responses containing status codes and descriptions.</param>
//        /// <returns>A dictionary where the key is the response status code (e.g., "200", "400") 
//        /// and the value is the response description along with its data type if available.</returns>
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
//        /// Extracts schema properties from an OpenAPI schema.
//        /// </summary>
//        /// <param name="schema">The OpenAPI schema to extract properties from.</param>
//        /// <returns>A list of <see cref="ApiParameterModel"/> representing the extracted schema properties.</returns>
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
//    }
//}
