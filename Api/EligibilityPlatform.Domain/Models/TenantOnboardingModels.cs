using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Models
{
    /// <summary>
    /// Request model for onboarding a new tenant.
    /// </summary>
    public class TenantOnboardingRequest
    {
        /// <summary>
        /// The tenant ID (must already exist in your system).
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// User ID from MIdentity API for the initial super admin user.
        /// </summary>

        [Required]
        public int AdminUserId { get; set; }
    }

    /// <summary>
    /// Result model for tenant onboarding operation.
    /// </summary>
    public class TenantOnboardingResult
    {
        /// <summary>
        /// Indicates if the onboarding was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The created tenant ID.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// List of errors encountered during onboarding.
        /// </summary>
        public List<string> Errors { get; set; } = [];

        /// <summary>
        /// Detailed message about the onboarding result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
    public class TenantEligibilityValidation
    {
        public bool IsEligible { get; set; }
        public List<string> Errors { get; set; } = [];
        public string Message { get; set; } = string.Empty;
    }
}
