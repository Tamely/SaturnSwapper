using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct Parameter
{
    public readonly BindingFlags Flags;
    public readonly int Name;
    public readonly int Type;
    public readonly int Parent;
    public readonly int Ordinal;
    
    public Parameter(BindingFlags flags, int name, int type, int parent, int ordinal)
    {
        Flags = flags;
        Name = name;
        Type = type;
        Parent = parent;
        Ordinal = ordinal;
    }
}