using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedLong : ManagedPrimitive<long>
{
    public override RuntimeType Type { get; }
    public override long PrimValue { get; protected set; }
    
    public ManagedLong()
    {
        Type = ManagedRuntime.System.GetType("long");
        PrimValue = default;
    }
    
    public ManagedLong(long value)
    {
        Type = ManagedRuntime.System.GetType("long");
        PrimValue = value;
    }
    
    public unsafe ManagedLong(byte[] value)
    {
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        Type = ManagedRuntime.System.GetType("long");
        fixed (byte* ptr = value)
        {
            PrimValue = *(long*)ptr;
        }
    }
    
    public override IRuntimeObject ComputeOperation(OpCode operation, IRuntimeObject? other)
    {
        if (other is not ManagedPrimitive prim)
        {
            throw new ArgumentException($"Other must be a {nameof(ManagedPrimitive)}", nameof(other));
        }
        
        switch (operation)
        {
            case OpCode.Add:
            {
                var result = BitwiseAdd(prim);
                return new ManagedLong(GetValueAs<long>(result));
            }
            case OpCode.Sub:
            {
                var result = BitwiseSubtract(prim);
                return new ManagedLong(GetValueAs<long>(result));
            }
            case OpCode.Mul:
            {
                var result = BitwiseMultiply(prim);
                return new ManagedLong(GetValueAs<long>(result));
            }
            case OpCode.Div:
            {
                var result = BitwiseDivide(prim);
                return new ManagedLong(GetValueAs<long>(result));
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedLong");
            }
        }
    }
}