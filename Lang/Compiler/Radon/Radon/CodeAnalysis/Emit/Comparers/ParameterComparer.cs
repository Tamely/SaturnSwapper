using System;
using System.Collections.Generic;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Emit.Comparers;

internal sealed class ParameterComparer : IEqualityComparer<Parameter>
{
    public bool Equals(Parameter x, Parameter y)
    {
        return x.Flags == y.Flags && x.Name == y.Name && x.Type == y.Type &&
               x.Parent == y.Parent && x.Ordinal == y.Ordinal;
    }

    public int GetHashCode(Parameter obj)
    {
        return HashCode.Combine((int)obj.Flags, obj.Name, obj.Type, obj.Parent, obj.Ordinal);
    }
}
