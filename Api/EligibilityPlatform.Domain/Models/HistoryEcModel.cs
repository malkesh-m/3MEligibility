using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class HistoryEcModel
    {
        public int Seq { get; set; }

        public int? TranId { get; set; }

        public string? Expression { get; set; }

        public int? EcardId { get; set; }

        public bool Result { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
    }
}
