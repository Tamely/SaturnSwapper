using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedChar : ManagedPrimitive<byte>
{
    public override RuntimeType Type { get; }
    public override byte PrimValue { get; protected set; }
    
    public ManagedChar()
    {
        Type = ManagedRuntime.System.GetType("char");
        PrimValue = default;
    }
    
    public ManagedChar(char value)
    {
        Type = ManagedRuntime.System.GetType("char");
        PrimValue = (byte)value;
    }
    
    public ManagedChar(byte[] value)
    {
        Type = ManagedRuntime.System.GetType("char");
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
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
                return new ManagedChar(GetValueAs<char>(result));
            }
            case OpCode.Sub:
            {
                var result = BitwiseSubtract(prim);
                return new ManagedChar(GetValueAs<char>(result));
            }
            case OpCode.Mul:
            {
                var result = BitwiseMultiply(prim);
                return new ManagedChar(GetValueAs<char>(result));
            }
            case OpCode.Div:
            {
                var result = BitwiseDivide(prim);
                return new ManagedChar(GetValueAs<char>(result));
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedChar");
            }
        }
    }
}