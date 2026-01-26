namespace MEligibilityPlatform.Domain.Entities;

public partial class ParamtersMap
{
    public int Apiid { get; set; }

    public int NodeId { get; set; }

    public int ParameterId { get; set; }

    public int? MapFunctionId { get; set; }

    public string? Xmlparent { get; set; }

    public string? Xmlnode { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public virtual NodeApi Api { get; set; } = null!;

    public virtual MappingFunction? MapFunction { get; set; }

    public virtual Node Node { get; set; } = null!;

    public virtual Parameter Parameter { get; set; } = null!;
}
