using System.ComponentModel.DataAnnotations;

namespace MEligibilityPlatform.Domain.Entities;

public partial class Apidetail
{
    public int ApidetailsId { get; set; }

    public int? FromApiid { get; set; }

    public string? CallingParamName { get; set; }

    public int? Apiid { get; set; }

    public string? SourceApiparam { get; set; }

    public int? DataTypeId { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual NodeApi? Api { get; set; }

    public virtual DataType? DataType { get; set; }
}
