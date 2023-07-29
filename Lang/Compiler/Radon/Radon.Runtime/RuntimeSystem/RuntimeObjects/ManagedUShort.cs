using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedUShort : ManagedPrimitive<ushort>
{
    public override RuntimeType Type { get; }
    public override ushort PrimValue { get; protected set; }
    
    public ManagedUShort()
    {
        Type = ManagedRuntime.System.GetType("ushort");
        PrimValue = default;
    }
    
    public ManagedUShort(ushort value)
    {
        Type = ManagedRuntime.System.GetType("ushort");
        PrimValue = value;
    }
    
    public unsafe ManagedUShort(byte[] value)
    {
        Type = ManagedRuntime.System.GetType("ushort");
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        fixed (byte* ptr = value)
        {
            PrimValue = *(ushort*)ptr;
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
                return new ManagedUShort(GetValueAs<ushort>(result));
            }
            case OpCode.Sub:
            {
                var result = BitwiseSubtract(prim);
                return new ManagedUShort(GetValueAs<ushort>(result));
            }
            case OpCode.Mul:
            {
                var result = BitwiseMultiply(prim);
                return new ManagedUShort(GetValueAs<ushort>(result));
            }
            case OpCode.Div:
            {
                var result = BitwiseDivide(prim);
                return new ManagedUShort(GetValueAs<ushort>(result));
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedUShort");
            }
        }
    }
}