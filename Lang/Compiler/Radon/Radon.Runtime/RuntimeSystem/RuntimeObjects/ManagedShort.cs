using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedShort : ManagedPrimitive<short>
{
    public override RuntimeType Type { get; }
    public override short PrimValue { get; protected set; }
    
    public ManagedShort()
    {
        Type = ManagedRuntime.System.GetType("short");
        PrimValue = default;
    }
    
    public ManagedShort(short value)
    {
        Type = ManagedRuntime.System.GetType("short");
        PrimValue = value;
    }
    
    public unsafe ManagedShort(byte[] value)
    {
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        Type = ManagedRuntime.System.GetType("short");
        fixed (byte* ptr = value)
        {
            PrimValue = *(short*)ptr;
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
                return new ManagedShort(GetValueAs<short>(result));
            }
            case OpCode.Sub:
            {
                var result = BitwiseSubtract(prim);
                return new ManagedShort(GetValueAs<short>(result));
            }
            case OpCode.Mul:
            {
                var result = BitwiseMultiply(prim);
                return new ManagedShort(GetValueAs<short>(result));
            }
            case OpCode.Div:
            {
                var result = BitwiseDivide(prim);
                return new ManagedShort(GetValueAs<short>(result));
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedShort");
            }
        }
    }
}