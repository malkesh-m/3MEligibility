namespace MEligibilityPlatform.Domain.Entities;

public partial class MakerChecker
{
    public int MakerCheckerId { get; set; }

    public int MakerId { get; set; }

    public int? CheckerId { get; set; }

    public DateTime MakerDate { get; set; }

    public DateTime? CheckerDate { get; set; }

    public string TableName { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public string OldValueJson { get; set; } = null!;

    public string NewValueJson { get; set; } = null!;

    public int RecordId { get; set; }

    public byte Status { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string? Comment { get; set; }

    public virtual User? Checker { get; set; }

    public virtual User Maker { get; set; } = null!;
}
