using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct EnumMember
{
    public readonly BindingFlags Flags;
    public readonly int Name;
    public readonly int Type;
    public readonly int ValueIndex;
    public readonly int Parent;
    
    public EnumMember(BindingFlags flags, int name, int type, int valueIndex, int parent)
    {
        Flags = flags;
        Name = name;
        Type = type;
        ValueIndex = valueIndex;
        Parent = parent;
    }
}