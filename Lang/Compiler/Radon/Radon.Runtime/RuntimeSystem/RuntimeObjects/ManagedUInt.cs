using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedUInt : ManagedPrimitive<uint>
{
    public override RuntimeType Type { get; }
    public override uint PrimValue { get; protected set; }
    
    public ManagedUInt()
    {
        Type = ManagedRuntime.System.GetType("uint");
        PrimValue = default;
    }
    
    public ManagedUInt(uint value)
    {
        Type = ManagedRuntime.System.GetType("uint");
        PrimValue = value;
    }
    
    public unsafe ManagedUInt(byte[] value)
    {
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        Type = ManagedRuntime.System.GetType("uint");
        fixed (byte* ptr = value)
        {
            PrimValue = *(uint*)ptr;
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
                return new ManagedUInt(GetValueAs<uint>(result));
            }
            case OpCode.Sub:
            {
                var result = BitwiseSubtract(prim);
                return new ManagedUInt(GetValueAs<uint>(result));
            }
            case OpCode.Mul:
            {
                var result = BitwiseMultiply(prim);
                return new ManagedUInt(GetValueAs<uint>(result));
            }
            case OpCode.Div:
            {
                var result = BitwiseDivide(prim);
                return new ManagedUInt(GetValueAs<uint>(result));
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedUInt");
            }
        }
    }
}