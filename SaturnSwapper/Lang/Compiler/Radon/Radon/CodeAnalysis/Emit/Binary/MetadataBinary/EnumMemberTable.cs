using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct EnumMemberTable
{
    public readonly EnumMember[] Members;
    
    public EnumMemberTable(EnumMember[] members)
    {
        Members = members;
    }
}