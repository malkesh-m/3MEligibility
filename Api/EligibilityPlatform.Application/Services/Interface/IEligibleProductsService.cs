using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for eligible products operations.
    /// Provides methods for retrieving eligible products based on validation criteria.
    /// </summary>
    public interface IEligibleProductsService
    {
        /// <summary>
        /// Retrieves all eligible products and their amounts for a specific entity based on provided criteria.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the entity to retrieve eligible products for.</param>
        /// <param name="keyValues">A dictionary of key-value pairs containing validation and filtering parameters.</param>
        /// <returns>An <see cref="EligibleAmountResults"/> object containing the eligible products and their amounts.</returns>
        EligibleAmountResults GetAllEligibleProducts(int tenantId, Dictionary<int, object> keyValues);
        Task<BREIntegrationResponses> ProcessBREIntegration(Dictionary<string, object> KeyValues, int EntityId, string? RequestId);
        Task<object> CallMOZNApi(MOZNRequest request, EvaluationHistory evaluation);
        Task<object> CallFLIPApi(string nationalId, EvaluationHistory evaluation);
        Task<object> CallYaqeenApi(string nationalId, EvaluationHistory evaluation);
        Task<object> CallFutureWorksApi(string nationalId, EvaluationHistory evaluation);

        Task<object> CallSIMAHApi(string NationalID, EvaluationHistory evaluation);
        Task<string> CallExternalApiAsync(string url, string httpMethod, object payload, int ApiId, EvaluationHistory? evaluation = null, string? headersJson = null);
        Task<string> CallExternalApiWithMappingAsync(DynamicApiRequest request);



    }
}
