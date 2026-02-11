using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Controllers
{
    /// <summary>
    /// Controller for tenant onboarding operations.
    /// </summary>
    [Route("api/tenantonboarding")]
    [ApiController]
    public class TenantOnboardingController(ITenantOnboardingService onboardingService) : ControllerBase
    {
        private readonly ITenantOnboardingService _onboardingService = onboardingService;

        /// <summary>
        /// Onboards a new tenant with all required initial data.
        /// </summary>
        /// <param name="request">The tenant onboarding request.</param>
        /// <returns>The onboarding result.</returns>
        [HttpPost("onboard")]
        [AllowAnonymous] // Allow anonymous for initial tenant creation
        public async Task<ActionResult<TenantOnboardingResult>> OnboardTenant([FromBody] TenantOnboardingRequest request)
        {
            var result = await _onboardingService.OnboardNewTenantAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Validates that a tenant has all required setup data.
        /// </summary>
        /// <param name="tenantId">The tenant ID to validate.</param>
        /// <returns>True if the tenant setup is complete.</returns>
        [HttpGet("validate/{tenantId}")]
        public async Task<ActionResult<bool>> ValidateTenantSetup(int tenantId)
        {
            var isValid = await _onboardingService.ValidateTenantSetupAsync(tenantId);
            return Ok(isValid);
        }
    }
}
