using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedULong : ManagedPrimitive<ulong>
{
    public override RuntimeType Type { get; }
    public override ulong PrimValue { get; protected set; }
    
    public ManagedULong()
    {
        Type = ManagedRuntime.System.GetType("ulong");
        PrimValue = default;
    }
    
    public ManagedULong(ulong value)
    {
        Type = ManagedRuntime.System.GetType("ulong");
        PrimValue = value;
    }
    
    public unsafe ManagedULong(byte[] value)
    {
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        Type = ManagedRuntime.System.GetType("ulong");
        fixed (byte* ptr = value)
        {
            PrimValue = *(ulong*)ptr;
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
                return new ManagedULong(GetValueAs<ulong>(result));
            }
            case OpCode.Sub:
            {
                var result = BitwiseSubtract(prim);
                return new ManagedULong(GetValueAs<ulong>(result));
            }
            case OpCode.Mul:
            {
                var result = BitwiseMultiply(prim);
                return new ManagedULong(GetValueAs<ulong>(result));
            }
            case OpCode.Div:
            {
                var result = BitwiseDivide(prim);
                return new ManagedULong(GetValueAs<ulong>(result));
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedULong");
            }
        }
    }
}