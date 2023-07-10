using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.Runtime.RuntimeInfo;

internal sealed class MemberReferenceInfo
{
    public MemberType MemberType { get; }
    public TypeInfo ParentType { get; }
    public TypeInfo ReturnType { get; }
    public IMemberInfo MemberInfo { get; }
    public MemberReferenceInfo(MemberReference memberReference, Metadata metadata)
    {
        MemberType = memberReference.MemberType;
        var type = metadata.Types.Types[memberReference.ReturnType];
        var parent = metadata.Types.Types[memberReference.ParentType];
        var parentType = TypeTracker.Add(parent, metadata, null);
        ParentType = parentType;
        ReturnType = TypeTracker.Add(type, metadata, null);
        MemberInfo = parentType.GetByRef<IMemberInfo>(MemberType, memberReference);
    }
}