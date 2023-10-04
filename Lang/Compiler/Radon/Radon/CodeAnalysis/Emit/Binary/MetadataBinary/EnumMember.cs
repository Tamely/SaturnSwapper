using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct EnumMember(BindingFlags MemberBindingFlags, int Name, int Type, int ValueIndex, int Parent)
{
    public readonly BindingFlags MemberBindingFlags = MemberBindingFlags;
    public readonly int Name = Name;
    public readonly int Type = Type;
    public readonly int ValueIndex = ValueIndex;
    public readonly int Parent = Parent;
}