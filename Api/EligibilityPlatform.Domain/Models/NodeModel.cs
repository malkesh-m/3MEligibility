using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Models
{
    public class NodeModel
    {
        public int NodeId { get; set; }
        [RegularExpression(
@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
ErrorMessage = "Node Name can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
)]
        [StringLength(50, ErrorMessage = "Node Name cannot exceed 50 characters.")]
        public string? NodeName { get; set; }

        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
        public string? Code { get; set; }

        [RegularExpression(
@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$",
ErrorMessage = "Description can contain Arabic letters, English letters, numbers, spaces, hyphens, and underscores only."
)]
        [StringLength(50, ErrorMessage = "Node Description cannot exceed 50 characters.")]
        public string? NodeDesc { get; set; }

        public string? NodeUrl { get; set; }

        [StringLength(50, ErrorMessage = "API Username cannot exceed 50 characters.")]
        public string? ApiuserName { get; set; }

        [StringLength(50, ErrorMessage = "API Password cannot exceed 50 characters.")]
        public string? Apipassword { get; set; }

        [Required(ErrorMessage = "TenantId is required.")]
        public int TenantId { get; set; }

        public bool IsAuthenticationRequired { get; set; }

        [StringLength(20, ErrorMessage = "AuthType cannot exceed 20 characters.")]
        public string AuthType { get; set; } = string.Empty; // none, basic, oauth2, apikey

        public string AuthSettings { get; set; } = string.Empty;

        public string UsernameField { get; set; } = string.Empty;

        public string PasswordField { get; set; } = string.Empty;

        public bool IsTokenKeywordExist { get; set; }

        public string TokenKeyword { get; set; } = string.Empty;

        public string Headers { get; set; } = string.Empty; // JSON serialized headers

        [Range(1, int.MaxValue, ErrorMessage = "TimeoutSeconds must be greater than 0.")]
        public int TimeoutSeconds { get; set; } = 30;

        [Range(0, 10, ErrorMessage = "RetryCount must be between 0 and 10.")]
        public int RetryCount { get; set; } = 3;

        [Required]
        [StringLength(10, ErrorMessage = "UrlType cannot exceed 10 characters.")]
        public string UrlType { get; set; } = string.Empty;

        //  public virtual Entity? Entity { get; set; }


    }

    public class NodeListModel : NodeModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class NodeCreateUpdateModel : NodeModel
    {

    }
}
