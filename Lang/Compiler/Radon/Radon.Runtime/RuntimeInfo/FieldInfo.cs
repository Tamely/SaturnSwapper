using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.Runtime.RuntimeInfo;

internal sealed record FieldInfo : IMemberInfo
{
    public bool IsStatic { get; }
    public string Name { get; }
    public TypeInfo Type { get; }
    public TypeInfo Parent { get; }
    public int Offset { get; }
    public FieldInfo(Field field, Metadata metadata, TypeInfo parent)
    {
        IsStatic = field.Flags.HasFlag(BindingFlags.Static);
        Name = metadata.Strings.Strings[field.Name];
        Type = TypeTracker.Add(metadata.Types.Types[field.Type], metadata, parent);
        Parent = parent;
        Offset = field.Offset;
    }
    
    public override string ToString()
    {
        return $"{Parent.ToString(false)}.{Name}";
    }
}