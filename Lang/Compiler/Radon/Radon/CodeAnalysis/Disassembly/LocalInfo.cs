using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Disassembly;

public sealed record LocalInfo
{
    public int Ordinal { get; }
    public string Name { get; }
    public TypeInfo Type { get; }
    public LocalInfo(Local local, Metadata metadata, TypeInfo parentType)
    {
        Ordinal = local.Ordinal;
        Name = metadata.Strings.Strings[local.Name];
        Type = TypeTracker.Add(metadata.Types.Types[local.Type], metadata, parentType);
    }
    
    public override string ToString()
    {
        return $"{Type} {Name}";
    }
}