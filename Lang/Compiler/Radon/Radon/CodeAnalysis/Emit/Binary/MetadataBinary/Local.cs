using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Local(int ParentMethod, int Ordinal, int Name, int Type)
{
    public readonly int ParentMethod = ParentMethod;
    public readonly int Ordinal = Ordinal;
    public readonly int Name = Name;
    public readonly int Type = Type;
}