namespace EligibilityPlatform.Domain.Entities;
public class ParameterComputedValue
{
    public int ComputedValueId { get; set; }

    public int ParameterId { get; set; }
    public Parameter Parameter { get; set; } = default!;

    public ParameterComputedType ComputedParameterType { get; set; } = ParameterComputedType.Single;

    public string? FromValue { get; set; }
    public string? ToValue { get; set; }

    public RangeType? RangeType { get; set; } //  Days or Hours, etc.

    // Optional match for string values
    public string? ParameterExactValue { get; set; }

    public string ComputedValue { get; set; } = string.Empty;
}

public enum ParameterComputedType
{
    Single = 0,
    Range = 1
}

public enum RangeType
{
    Hours = 0,
    Days = 1,
}