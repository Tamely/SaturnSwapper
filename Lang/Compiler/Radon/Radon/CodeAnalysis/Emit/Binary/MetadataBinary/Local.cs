using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Local
{
    public readonly int Ordinal;
    public readonly int Name;
    public readonly int Type;

    public Local(int ordinal, int name, int type)
    {
        Ordinal = ordinal;
        Name = name;
        Type = type;
    }
}