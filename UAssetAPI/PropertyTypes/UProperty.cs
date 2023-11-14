namespace UAssetAPI.PropertyTypes;

public class UProperty
{
    public string Type;
    public string? InnerType;
    public string? EnumType;
    public string? StructType;
    public string Name;
    public object? Value;
    public bool IsZero;

    public override string ToString() => $"{Value} ({Type})";
}