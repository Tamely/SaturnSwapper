using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MemberReference
{
    public readonly MemberType MemberType;
    public readonly int ParentType;
    public readonly int ReturnType;
    public readonly int MemberDefinition;

    public MemberReference(MemberType memberType, int parentType, int returnType, int memberDefinition)
    {
        MemberType = memberType;
        ParentType = parentType;
        ReturnType = returnType;
        MemberDefinition = memberDefinition;
    }
}