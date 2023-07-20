using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedFloat : ManagedPrimitive<float>
{
    public override RuntimeType Type { get; }
    public override float PrimValue { get; protected set; }
    
    public ManagedFloat()
    {
        Type = ManagedRuntime.System.GetType("float");
        PrimValue = default;
    }
    
    public ManagedFloat(float value)
    {
        Type = ManagedRuntime.System.GetType("float");
        PrimValue = value;
    }
    
    public unsafe ManagedFloat(byte[] value)
    {
        Type = ManagedRuntime.System.GetType("float");
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        fixed (byte* ptr = value)
        {
            PrimValue = *(float*)ptr;
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
                var result = PrimValue + (dynamic)prim.Value!;
                return new ManagedFloat(result);
            }
            case OpCode.Sub:
            {
                var result = PrimValue - (dynamic)prim.Value!;
                return new ManagedFloat(result);
            }
            case OpCode.Mul:
            {
                var result = PrimValue * (dynamic)prim.Value!;
                return new ManagedFloat(result);
            }
            case OpCode.Div:
            {
                var result = PrimValue / (dynamic)prim.Value!;
                return new ManagedFloat(result);
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedFloat");
            }
        }
    }
}