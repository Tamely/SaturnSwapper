using System;
using System.Collections.Generic;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Emit.Comparers;

internal sealed class MethodComparer : IEqualityComparer<Method>
{
    public bool Equals(Method x, Method y)
    {
        return x.Flags == y.Flags && x.Name == y.Name && x.ReturnType == y.ReturnType &&
               x.Parent == y.Parent && x.ParameterCount == y.ParameterCount &&
               x.FirstParameter == y.FirstParameter;
    }

    public int GetHashCode(Method obj)
    {
        var hashCode = new HashCode();
        hashCode.Add((int)obj.Flags);
        hashCode.Add(obj.Name);
        hashCode.Add(obj.ReturnType);
        hashCode.Add(obj.Parent);
        hashCode.Add(obj.ParameterCount);
        hashCode.Add(obj.FirstParameter);
        hashCode.Add(obj.LocalCount);
        hashCode.Add(obj.FirstLocal);
        hashCode.Add(obj.InstructionCount);
        hashCode.Add(obj.FirstInstruction);
        return hashCode.ToHashCode();
    }
}
