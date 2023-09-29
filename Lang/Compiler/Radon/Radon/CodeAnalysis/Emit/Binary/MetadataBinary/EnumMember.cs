using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct EnumMember
{
    public readonly BindingFlags MemberBindingFlags;
    public readonly int Name;
    public readonly int Type;
    public readonly int ValueIndex;
    public readonly int Parent;
    
    public EnumMember(BindingFlags memberBindingFlags, int name, int type, int valueIndex, int parent)
    {
        MemberBindingFlags = memberBindingFlags;
        Name = name;
        Type = type;
        ValueIndex = valueIndex;
        Parent = parent;
    }
}