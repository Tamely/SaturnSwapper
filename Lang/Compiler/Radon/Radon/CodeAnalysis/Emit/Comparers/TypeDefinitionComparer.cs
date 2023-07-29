using System;
using System.Collections.Generic;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Emit.Comparers;

internal sealed class TypeDefinitionComparer : IEqualityComparer<TypeDefinition>
{
    public bool Equals(TypeDefinition x, TypeDefinition y)
    {
        return x.Flags == y.Flags && x.Kind == y.Kind && x.Name == y.Name && x.Size == y.Size &&
               x.UnderlyingType == y.UnderlyingType && x.FieldCount == y.FieldCount &&
               x.FieldStartOffset == y.FieldStartOffset && x.EnumMemberCount == y.EnumMemberCount &&
               x.EnumMemberStartOffset == y.EnumMemberStartOffset && x.MethodCount == y.MethodCount &&
               x.MethodStartOffset == y.MethodStartOffset && x.ConstructorCount == y.ConstructorCount &&
               x.ConstructorStartOffset == y.ConstructorStartOffset && x.StaticConstructor == y.StaticConstructor;
    }

    public int GetHashCode(TypeDefinition obj)
    {
        var hashCode = new HashCode();
        hashCode.Add((int)obj.Flags);
        hashCode.Add((int)obj.Kind);
        hashCode.Add(obj.Name);
        hashCode.Add(obj.Size);
        hashCode.Add(obj.UnderlyingType);
        hashCode.Add(obj.FieldCount);
        hashCode.Add(obj.FieldStartOffset);
        hashCode.Add(obj.EnumMemberCount);
        hashCode.Add(obj.EnumMemberStartOffset);
        hashCode.Add(obj.MethodCount);
        hashCode.Add(obj.MethodStartOffset);
        hashCode.Add(obj.ConstructorCount);
        hashCode.Add(obj.ConstructorStartOffset);
        hashCode.Add(obj.StaticConstructor);
        return hashCode.ToHashCode();
    }
}
