using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Parameter(int Name, int Type, int Ordinal)
{
    public readonly int Ordinal = Ordinal;
    public readonly int Name = Name;
    public readonly int Type = Type;
}