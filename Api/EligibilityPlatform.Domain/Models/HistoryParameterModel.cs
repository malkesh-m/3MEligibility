using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class HistoryParameterModel
    {
        public int Seq { get; set; }

        public int? TranId { get; set; }

        public string? Expression { get; set; }

        public int? EruleId { get; set; }

        public int? ParameterId { get; set; }

        public string? ValueRet { get; set; }

        public string? Condition { get; set; }

        public bool Result { get; set; }

        public int? FactorId { get; set; }
        [JsonIgnore]
        public DateTime UpdatedByDateTime { get; set; }
    }
}
