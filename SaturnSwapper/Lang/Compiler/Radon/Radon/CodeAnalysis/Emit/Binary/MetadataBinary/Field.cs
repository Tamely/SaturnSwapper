using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Field(BindingFlags BindingFlags, int Name, int Type, int Parent, int Offset)
{
    public readonly BindingFlags BindingFlags = BindingFlags;
    public readonly int Name = Name;
    public readonly int Type = Type;
    public readonly int Parent = Parent;
    public readonly int Offset = Offset;
}