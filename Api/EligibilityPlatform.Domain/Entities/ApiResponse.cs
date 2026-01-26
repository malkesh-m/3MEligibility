namespace MEligibilityPlatform.Domain.Entities;

public partial class ApiResponse
{
    public int ResponceId { get; set; }

    public int ApiId { get; set; }

    public string ResponceCode { get; set; } = null!;

    public string? ResponceDescription { get; set; }

    public string ResponceSchema { get; set; } = null!;

    public int? NodeApiApiid { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual NodeApi? NodeApiApi { get; set; }
}
