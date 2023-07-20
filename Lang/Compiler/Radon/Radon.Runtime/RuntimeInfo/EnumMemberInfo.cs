using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.Runtime.RuntimeInfo;

internal sealed record EnumMemberInfo : IMemberInfo
{
    public string Name { get; }
    public byte[] Value { get; }
    public TypeInfo Type { get; }
    public TypeInfo Parent { get; }
    public EnumMemberInfo(EnumMember enumMember, Metadata metadata, TypeInfo parent)
    {
        Name = metadata.Strings.Strings[enumMember.Name];
        var constant = metadata.Constants.Constants[enumMember.ValueIndex];
        var type = metadata.Types.Types[enumMember.Type];
        var size = type.Size;
        var bytes = new byte[size];
        for (var i = 0; i < size; i++)
        {
            bytes[i] = metadata.ConstantsPool.Values[constant.ValueOffset + i];
        }
        
        Value = bytes;
        Type = TypeTracker.Add(type, metadata, parent);
        Parent = parent;
    }

    public override string ToString()
    {
        return $"{Parent.ToString(false)}.{Name} = {Value}";
    }
}