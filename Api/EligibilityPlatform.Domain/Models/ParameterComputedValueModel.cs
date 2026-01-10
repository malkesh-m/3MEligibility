using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Domain.Models;
public class ParameterComputedValueModel
{
    public int ComputedValueId { get; set; }
    public ParameterComputedType ComputedParameterType { get; set; } = ParameterComputedType.Single;

    public string? FromValue { get; set; }
    public string? ToValue { get; set; }
    public RangeType? RangeType { get; set; } //  Days or Hours, etc.

    // Optional match for string values
    public string? ParameterExactValue { get; set; }

    public string ComputedValue { get; set; } = string.Empty;
}
