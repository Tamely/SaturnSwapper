using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedByte : ManagedPrimitive<byte>
{
    public override RuntimeType Type { get; }
    public override byte PrimValue { get; protected set; }
    
    public ManagedByte()
    {
        Type = ManagedRuntime.System.GetType("byte");
        PrimValue = default;
    }
    
    public ManagedByte(byte value)
    {
        Type = ManagedRuntime.System.GetType("byte");
        PrimValue = value;
    }
    
    public unsafe ManagedByte(byte[] value)
    {
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        Type = ManagedRuntime.System.GetType("byte");
        PrimValue = GetValueAs<byte>(value);
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
                return new ManagedByte(GetValueAs<byte>(result));
            }
            case OpCode.Sub:
            {
                var result = BitwiseSubtract(prim);
                return new ManagedByte(GetValueAs<byte>(result));
            }
            case OpCode.Mul:
            {
                var result = BitwiseMultiply(prim);
                return new ManagedByte(GetValueAs<byte>(result));
            }
            case OpCode.Div:
            {
                var result = BitwiseDivide(prim);
                return new ManagedByte(GetValueAs<byte>(result));
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedByte");
            }
        }
    }
}