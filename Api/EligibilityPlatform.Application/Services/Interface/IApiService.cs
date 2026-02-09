using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for API operations.
    /// Provides methods for retrieving API details and calling various types of APIs.
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// Retrieves API details for a specific node.
        /// </summary>
        /// <param name="nodeId">The unique identifier of the node to retrieve API details for.</param>
        /// <returns>A list of <see cref="ApiParameterModel"/> objects containing the API details.</returns>
        Task<List<ApiParameterModel>> GetApiDetailsAsync(int nodeId);

        /// <summary>
        /// Calls a SOAP API with the provided model.
        /// </summary>
        /// <param name="model">The <see cref="SoapApiModel"/> containing SOAP API configuration and parameters.</param>
        /// <returns>A string containing the SOAP API response.</returns>
        Task<string> CallSoapApi(SoapApiModel model);

        /// <summary>
        /// Calls a REST API with the provided execution model.
        /// </summary>
        /// <param name="request">The <see cref="ExecuteApiModel"/> containing REST API configuration and parameters.</param>
        /// <returns>A <see cref="ResponseModel"/> containing the REST API response details.</returns>
        Task<ResponseModel> CallRestApi(ExecuteApiModel request);

        /// <summary>
        /// Retrieves REST API details for a specific node.
        /// </summary>
        /// <param name="nodeId">The unique identifier of the node to retrieve REST API details for.</param>
        /// <returns>A list of <see cref="ApiParameterModel"/> objects containing the REST API details.</returns>
        Task<List<ApiParameterModel>> GetRestApiDetailsAsync(int nodeId);
    }
}
