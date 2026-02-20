using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class EruleModel
    {
        public int EruleId { get; set; }

        //[JsonIgnore]

        [Required(ErrorMessage = "Erule master ID is required.")]
        public int EruleMasterId { get; set; }

        [Required(ErrorMessage = "Expression is required.")]
        [StringLength(2000, ErrorMessage = "Expression cannot exceed 2000 characters.")]
        public required string Expression { get; set; }

        [JsonIgnore]
        public int TenantId { get; set; }

        [StringLength(2000, ErrorMessage = "ExpShown cannot exceed 2000 characters.")]
        public string? ExpShown { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Version must be greater than 0.")]
        public int? Version { get; set; }

        public bool? IsActive { get; set; }

        [Required(ErrorMessage = "ValidFrom date is required.")]
        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool IsPublished { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string? Comment { get; set; }
    }

    public class EruleListModel : EruleModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public string? EruleName { get; set; }
        public string? Description { get; set; }

    }

    public class EruleCreateOrUpdateModel : EruleModel
    {
        [Required(ErrorMessage = "Erule name is required.")]
        [StringLength(100, ErrorMessage = "Erule name cannot exceed 100 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Erule name contains invalid characters.")]
        public required string EruleName { get; set; }
    }

    public class EruleCreateModel : EruleModel
    {
        [Required(ErrorMessage = "Erule name is required.")]
        [StringLength(100, ErrorMessage = "Erule name cannot exceed 100 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Erule name contains invalid characters.")]
        public required string EruleName { get; set; }
        public string? Description { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]
        public DateTime CreatedByDateTime { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
        [JsonIgnore]
        public int UserID { get; set; }

    }

    public class EruleCreateVersionModel : EruleModel
    {
    }

    public class EruleUpdateModel : EruleModel
    {
        [Required(ErrorMessage = "Erule name is required.")]
        [StringLength(100, ErrorMessage = "Erule name cannot exceed 100 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Erule name contains invalid characters.")]
        public required string EruleName { get; set; }
        public string? Description { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }

    }

    public class EruleModelDescription
    {
        public int EruleId { get; set; }

        //public required string EruleName { get; set; }

        //public string? EruleDesc { get; set; }

        public required string Expression { get; set; }

        //public int EntityId { get; set; }
        //public string? EntityName { get; set; }

        public string? ExpShown { get; set; }

    }
    public sealed class RuleExportRow
    {
        [System.ComponentModel.DataAnnotations.Display(Order = 1)]
        public int EruleId { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Order = 2)]
        public string? RuleName { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Order = 3)]
        public string? RuleDescription { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Order = 4)]
        public bool? IsActive { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Order = 5)]
        public string? CreatedBy { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Order = 6)]
        public DateTime? CreatedByDateTime { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Order = 7)]
        public string? UpdatedBy { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Order = 8)]
        public DateTime? UpdatedByDateTime { get; set; }
    }
}
