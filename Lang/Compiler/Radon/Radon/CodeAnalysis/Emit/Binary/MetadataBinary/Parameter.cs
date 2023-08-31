using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Parameter
{
    public readonly int Ordinal;
    public readonly int Name;
    public readonly int Type;
    
    public Parameter(int name, int type, int ordinal)
    {
        Name = name;
        Type = type;
        Ordinal = ordinal;
    }
}