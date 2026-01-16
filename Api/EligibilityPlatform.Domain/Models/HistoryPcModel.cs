using System.Text.Json.Serialization;

namespace EligibilityPlatform.Domain.Models
{
    public class HistoryPcModel
    {
        public int TranId { get; set; }

        public int? PcardId { get; set; }

        public string? Expression { get; set; }

        public int? ProductId { get; set; }
        [JsonIgnore]
        public int TenantId { get; set; }

        public string? CustomerId { get; set; }

        public string? TransReference { get; set; }

        public DateTime? TransactionDate { get; set; }

        public int? UserId { get; set; }

        public bool Result { get; set; }

        public string? Type { get; set; }

        public int? TranRef { get; set; }
        public DateTime UpdatedByDateTime { get; set; }

    }
}
