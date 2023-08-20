using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Disassembly;

internal sealed class TypeReferenceInfo
{
    public TypeInfo TypeDefinition { get; }
    public MemberReferenceInfo ConstructorReference { get; }
    public TypeReferenceInfo(TypeReference typeReference, Metadata metadata)
    {
        var definition = metadata.Types.Types[typeReference.TypeDefinition];
        TypeDefinition = TypeTracker.Add(definition, metadata, null);
        var memberRef = metadata.MemberReferences.MemberReferences[typeReference.ConstructorReference];
        ConstructorReference = new MemberReferenceInfo(memberRef, metadata);
    }
}