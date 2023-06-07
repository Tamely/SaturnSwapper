using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MemberReferenceTable
{
    public readonly MemberReference[] MemberReferences;
    
    public MemberReferenceTable(MemberReference[] memberReferences)
    {
        MemberReferences = memberReferences;
    }
}