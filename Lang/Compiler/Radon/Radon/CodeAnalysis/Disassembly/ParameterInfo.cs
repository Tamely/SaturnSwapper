using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Disassembly;

internal sealed record ParameterInfo
{
    public int Ordinal { get; }
    public string Name { get; }
    public TypeInfo Type { get; }
    public MethodInfo Parent { get; }
    public ParameterInfo(Parameter parameter, Metadata metadata, MethodInfo parent, TypeInfo parentType)
    {
        Ordinal = parameter.Ordinal;
        Name = metadata.Strings.Strings[parameter.Name];
        Type = TypeTracker.Add(metadata.Types.Types[parameter.Type], metadata, parentType);
        Parent = parent;
    }
    
    public override string ToString()
    {
        return $"{Type} {Name}";
    }
}