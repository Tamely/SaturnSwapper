using System;
using System.Collections.Generic;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Emit.Comparers;

internal sealed class EnumMemberComparer : IEqualityComparer<EnumMember>
{
    public bool Equals(EnumMember x, EnumMember y)
    {
        return x.MemberBindingFlags == y.MemberBindingFlags && x.Name == y.Name && x.Type == y.Type &&
               x.ValueIndex == y.ValueIndex && x.Parent == y.Parent;
    }

    public int GetHashCode(EnumMember obj)
    {
        return HashCode.Combine((int)obj.MemberBindingFlags, obj.Name, obj.Type, obj.ValueIndex, obj.Parent);
    }
}
