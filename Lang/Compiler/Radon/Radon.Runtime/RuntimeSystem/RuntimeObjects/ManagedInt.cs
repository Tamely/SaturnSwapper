using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedInt : ManagedPrimitive<int>
{
    public override RuntimeType Type { get; }
    public override int PrimValue { get; protected set; }
    
    public ManagedInt()
    {
        Type = ManagedRuntime.System.GetType("int");
        PrimValue = default;
    }
    
    public ManagedInt(int value)
    {
        Type = ManagedRuntime.System.GetType("int");
        PrimValue = value;
    }
    
    public unsafe ManagedInt(byte[] value)
    {
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        Type = ManagedRuntime.System.GetType("int");
        fixed (byte* ptr = value)
        {
            PrimValue = *(int*)ptr;
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
                return new ManagedInt(GetValueAs<int>(result));
            }
            case OpCode.Sub:
            {
                var result = BitwiseSubtract(prim);
                return new ManagedInt(GetValueAs<int>(result));
            }
            case OpCode.Mul:
            {
                var result = BitwiseMultiply(prim);
                return new ManagedInt(GetValueAs<int>(result));
            }
            case OpCode.Div:
            {
                var result = BitwiseDivide(prim);
                return new ManagedInt(GetValueAs<int>(result));
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedInt");
            }
        }
    }
}