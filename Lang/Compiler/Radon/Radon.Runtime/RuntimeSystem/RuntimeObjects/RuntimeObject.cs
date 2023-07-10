using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal abstract class RuntimeObject : IRuntimeObject
{
    public abstract int Size { get; }
    public abstract RuntimeType Type { get; }
    public abstract object? Value { get; }
    public abstract int ResolveSize();

    public abstract byte[] Serialize();

    public abstract IRuntimeObject? ComputeOperation(OpCode operation, IRuntimeObject? other);

    public abstract IRuntimeObject? ConvertTo(RuntimeType type);
    
    public T GetValue<T>()
    {
        if (Value is null)
        {
            throw new InvalidOperationException("Value is null.");
        }
        
        return (T)Value;
    }
}