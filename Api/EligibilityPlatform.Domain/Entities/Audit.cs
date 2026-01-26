using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Entities;

public partial class Audit
{
    [Key]
    public int AuditId { get; set; }

    public DateTime ActionDate { get; set; }

    public string TableName { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int RecordId { get; set; }

    public string FieldName { get; set; } = null!;
    public string? IPAddress { get; set; }
    public string? Comments { get; set; }
    public string? UserName { get; set; }
    public DateTime UpdatedByDateTime { get; set; }
}
