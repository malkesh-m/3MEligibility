using System.Text.Json.Serialization;

namespace MEligibilityPlatform.Domain.Models
{
    public class AuditModel
    {
        public int AuditId { get; set; }

        public DateTime ActionDate { get; set; }

        public required string TableName { get; set; }

        public required string ActionName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        //public int EntityId { get; set; }

        //public int? UserId { get; set; }

        public int RecordId { get; set; }

        public required string FieldName { get; set; }
        public string? IPAddress { get; set; }
        public string? Comments { get; set; }
        public string? UserName { get; set; }

        public DateTime UpdatedByDateTime { get; set; }
    }
    public class AuditCreateUpdateModel
    {
        public int AuditId { get; set; }

        [JsonIgnore]
        public DateTime ActionDate { get; set; }

        public required string TableName { get; set; }

        public required string ActionName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        //public int EntityId { get; set; }

        //public int? UserId { get; set; }

        public int RecordId { get; set; }

        public required string FieldName { get; set; }
        public string? IPAddress { get; set; }
        public string? Comments { get; set; }
        [JsonIgnore]

        public string? UserName { get; set; }
        [JsonIgnore]

        public DateTime UpdatedByDateTime { get; set; }
    }
}
