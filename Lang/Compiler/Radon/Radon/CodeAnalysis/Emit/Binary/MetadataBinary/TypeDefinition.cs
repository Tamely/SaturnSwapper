using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct TypeDefinition(BindingFlags Flags, TypeKind Kind, int Name, int Size,
    int UnderlyingType, int FieldCount, int FieldStartOffset, int EnumMemberCount, int EnumMemberStartOffset,
    int MethodCount, int MethodStartOffset, int ConstructorCount, int ConstructorStartOffset, int StaticConstructor)
{
    public readonly BindingFlags Flags = Flags;
    public readonly TypeKind Kind = Kind;
    public readonly int Name = Name;
    public readonly int Size = Size;
    public readonly int UnderlyingType = UnderlyingType;
    public readonly int FieldCount = FieldCount;
    public readonly int FieldStartOffset = FieldStartOffset;
    public readonly int EnumMemberCount = EnumMemberCount;
    public readonly int EnumMemberStartOffset = EnumMemberStartOffset;
    public readonly int MethodCount = MethodCount;
    public readonly int MethodStartOffset = MethodStartOffset;
    public readonly int ConstructorCount = ConstructorCount;
    public readonly int ConstructorStartOffset = ConstructorStartOffset;
    public readonly int StaticConstructor = StaticConstructor;
}