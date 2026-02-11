using MEligibilityPlatform.Domain.Models;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Service interface for tenant onboarding operations.
    /// Provides methods for provisioning new tenants with all required initial data.
    /// </summary>
    public interface ITenantOnboardingService
    {
        /// <summary>
        /// Onboards a new tenant with all required initial data.
        /// </summary>
        /// <param name="request">The tenant onboarding request containing tenant and admin user details.</param>
        /// <returns>A task representing the asynchronous operation, with the onboarding result.</returns>
        Task<TenantOnboardingResult> OnboardNewTenantAsync(TenantOnboardingRequest request);

        /// <summary>
        /// Validates that a tenant has all required setup data.
        /// </summary>
        /// <param name="tenantId">The tenant ID to validate.</param>
        /// <returns>A task representing the asynchronous operation, with true if setup is complete.</returns>
        Task<bool> ValidateTenantSetupAsync(int tenantId);
        Task<ApiResponse<TenantModel>> GetById(int tenantId);

    }
}
