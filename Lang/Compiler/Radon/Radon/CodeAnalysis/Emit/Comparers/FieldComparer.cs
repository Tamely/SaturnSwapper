using System;
using System.Collections.Generic;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Emit.Comparers;

internal sealed class FieldComparer : IEqualityComparer<Field>
{
    public bool Equals(Field x, Field y)
    {
        return x.Flags == y.Flags && x.Name == y.Name && x.Type == y.Type &&
               x.Parent == y.Parent;
    }

    public int GetHashCode(Field obj)
    {
        return HashCode.Combine((int)obj.Flags, obj.Name, obj.Type, obj.Parent, obj.Offset);
    }
}
