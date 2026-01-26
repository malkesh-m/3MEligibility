using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class SettingModel
    {
        public bool IsMakerCheckerEnable { get; set; }
        [JsonIgnore]
        public int EntityId { get; set; }
    }
}
