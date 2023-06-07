using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.Runtime.RuntimeInfo;

internal sealed record LocalInfo
{
    public string Name { get; }
    public TypeInfo Type { get; }
    public LocalInfo(Local local, Metadata metadata, TypeInfo parentType)
    {
        Name = metadata.Strings.Strings[local.Name];
        Type = TypeTracker.Add(metadata.Types.Types[local.Type], metadata, parentType);
    }
    
    public override string ToString()
    {
        return $"{Type.ToString(false)} {Name}";
    }
}