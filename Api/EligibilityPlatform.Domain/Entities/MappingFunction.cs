namespace EligibilityPlatform.Domain.Entities;

public partial class MappingFunction
{
    public int MapFunctionId { get; set; }

    public string? MapFunctionValue { get; set; }

    public DateTime UpdatedByDateTime { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedByDateTime { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<ParamtersMap> ParamtersMaps { get; set; } = [];
}
