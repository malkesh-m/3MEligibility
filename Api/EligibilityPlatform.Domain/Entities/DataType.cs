using System.Reflection.Metadata;

namespace EligibilityPlatform.Domain.Entities;

public partial class DataType
{
    public int DataTypeId { get; set; }

    public string? DataTypeName { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual ICollection<Apidetail> Apidetails { get; set; } = [];

    public virtual ICollection<Parameter> Parameters { get; set; } = [];
}
