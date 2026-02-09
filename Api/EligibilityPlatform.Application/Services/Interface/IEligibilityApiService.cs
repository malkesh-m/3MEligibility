using System.ComponentModel.DataAnnotations;
using MEligibilityPlatform.Domain.Models;
using ValidationResult = MEligibilityPlatform.Domain.Models.ValidationResult;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for eligibility API operations.
    /// Provides methods for validating eligibility through API calls.
    /// </summary>
    public interface IEligibilityApiService
    {
        /// <summary>
        /// Validates eligibility asynchronously through API for a specific user and product.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to validate.</param>
        /// <param name="productId">The unique identifier of the product to validate against.</param>
        /// <param name="keyValues">A dictionary of key-value pairs containing additional validation parameters.</param>
        /// <returns>A <see cref="ValidationResult"/> containing the validation outcome.</returns>
        Task<ValidationResult> ValidAsyncAPI(int userId, int productId, Dictionary<int, object> keyValues);
    }
}
