namespace MEligibilityPlatform.Domain.Entities;

public partial class ImportDocument
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime ImportTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool Completed { get; set; }

    public int TotalRecords { get; set; }

    public int SuccessCount { get; set; }

    public int FailureCount { get; set; }

    public byte[]? FileData { get; set; }

    public string? CreatedBy { get; set; }
}
