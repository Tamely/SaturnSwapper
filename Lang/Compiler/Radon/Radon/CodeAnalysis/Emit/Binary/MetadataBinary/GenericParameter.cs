using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct GenericParameter
{
    public readonly int Name;
    public readonly int Parent; // This can either point to a type or a method.
    public readonly int Ordinal;
    
    public GenericParameter(int name, int parent, int ordinal)
    {
        Name = name;
        Parent = parent;
        Ordinal = ordinal;
    }
}