using System;
using System.Collections.Generic;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Emit.Comparers;

internal sealed class LocalComparer : IEqualityComparer<Local>
{
    public bool Equals(Local x, Local y)
    {
        return x.Name == y.Name && x.Type == y.Type && x.Ordinal == y.Ordinal && x.ParentMethod == y.ParentMethod;
    }

    public int GetHashCode(Local obj)
    {
        return HashCode.Combine(obj.Name, obj.Type);
    }
}
