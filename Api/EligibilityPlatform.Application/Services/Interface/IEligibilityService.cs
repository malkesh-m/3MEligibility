using System.ComponentModel.DataAnnotations;
using EligibilityPlatform.Domain.Models;
using ValidationResult = EligibilityPlatform.Domain.Models.ValidationResult;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for eligibility validation operations.
    /// Provides methods for validating user eligibility and finding best-fit products.
    /// </summary>
    public interface IEligibilityService
    {
        /// <summary>
        /// Validates user eligibility for a specific product asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to validate.</param>
        /// <param name="productId">The unique identifier of the product to validate against.</param>
        /// <param name="keyValues">A dictionary of key-value pairs containing validation parameters.</param>
        /// <returns>A <see cref="ValidationResult"/> containing the validation outcome.</returns>
        Task<ValidationResult> ValidAsync(int userId, int productId, Dictionary<int, object> keyValues);

        /// <summary>
        /// Retrieves the best-fit products for a user based on validation criteria asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="keyValues">A dictionary of key-value pairs containing validation parameters.</param>
        /// <returns>An enumerable of <see cref="BestFitProductModel"/> objects containing the best-fit products.</returns>
        Task<IEnumerable<BestFitProductModel>> GetBestFitProductsAsync(int userId, Dictionary<int, object> keyValues);
    }
}
