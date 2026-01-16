using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class PcardModel
    {
        public int PcardId { get; set; }
        [StringLength(50, ErrorMessage = "Pcard name cannot exceed 50 characters.")]
        [RegularExpression(@"^[\u0600-\u06FFa-zA-Z0-9_\-\s]+$", ErrorMessage = "Pcard name contains invalid characters.")]
        public string? PcardName { get; set; }

        [StringLength(50, ErrorMessage = "Pcard description cannot exceed 50 characters.")]
        public string? PcardDesc { get; set; }

        public string? Expression { get; set; }
        [JsonIgnore]
        public int TenantId { get; set; }

        public int? ProductId { get; set; }

        public string? Expshown { get; set; }

        [StringLength(10, ErrorMessage = "Pstatus cannot exceed 10 characters.")]
        public string? Pstatus { get; set; }

    }
    public class PcardListModel : PcardModel
    {
        public DateTime UpdatedByDateTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedByDateTime { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsImport { get; set; }

    }
    public class PcardAddUpdateModel : PcardModel
    {
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class PcardUpdateModel : PcardModel
    {
        [JsonIgnore]
        public string? UpdatedBy { get; set; }
    }
    public class PcardCsvModel
    {

        public int PcardId { get; set; }

        public string? PcardName { get; set; }

        public string? PcardDesc { get; set; }

        //public int EntityId { get; set; }
        //public string? EntityName { get; set; }

        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Expression { get; set; }
        public string? Expshown { get; set; }

        public string? Pstatus { get; set; }


    }

}
