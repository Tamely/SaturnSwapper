using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct Local
{
    public readonly BindingFlags Flags;
    public readonly int Name;
    public readonly int Type;

    public Local(BindingFlags flags, int name, int type)
    {
        Flags = flags;
        Name = name;
        Type = type;
    }
}