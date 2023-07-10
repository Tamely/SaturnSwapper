using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedSByte : ManagedPrimitive<sbyte>
{
    public override RuntimeType Type { get; }
    public override sbyte PrimValue { get; protected set; }

    public ManagedSByte()
    {
        Type = ManagedRuntime.System.GetType("sbyte");
        PrimValue = default;
    }
    
    public ManagedSByte(sbyte value)
    {
        Type = ManagedRuntime.System.GetType("sbyte");
        PrimValue = value;
    }

    public unsafe ManagedSByte(byte[] value)
    {
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        Type = ManagedRuntime.System.GetType("sbyte");
        fixed (byte* ptr = value)
        {
            PrimValue = *(sbyte*)ptr;
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
                return new ManagedSByte(GetValueAs<sbyte>(result));
            }
            case OpCode.Sub:
            {
                var result = BitwiseSubtract(prim);
                return new ManagedSByte(GetValueAs<sbyte>(result));
            }
            case OpCode.Mul:
            {
                var result = BitwiseMultiply(prim);
                return new ManagedSByte(GetValueAs<sbyte>(result));
            }
            case OpCode.Div:
            {
                var result = BitwiseDivide(prim);
                return new ManagedSByte(GetValueAs<sbyte>(result));
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedSByte");
            }
        }
    }
}