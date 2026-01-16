using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class EcardModel
    {

        public int EcardId { get; set; }
        [Required(ErrorMessage = "ECard name is required.")]
        [StringLength(50, ErrorMessage = "ECard name cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "ECard name contains invalid characters.")]
        public string? EcardName { get; set; }
        [StringLength(50, ErrorMessage = "ECard description cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "ECard description contains invalid characters.")]
        public string? EcardDesc { get; set; }
        [Required(ErrorMessage = "Expression is required.")]
        public string? Expression { get; set; }

        [JsonIgnore]
        public int TenantId { get; set; }

        public string? Expshown { get; set; }

    }
    public class EcardListModel : EcardModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsImport { get; set; }
    }
    public class EcardAddUpdateModel : EcardModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class EcardUpdateModel : EcardModel
    {
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class EcardModelDescription
    {

        public int EcardId { get; set; }

        public string? EcardName { get; set; }

        public string? EcardDesc { get; set; }

        public string? Expression { get; set; }

        //public int EntityId { get; set; }
        //public string? EntityName { get; set; }
        public string? Expshown { get; set; }


    }
}
