using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedDouble : ManagedPrimitive<double>
{
    public override RuntimeType Type { get; }
    public override double PrimValue { get; protected set; }
    
    public ManagedDouble()
    {
        Type = ManagedRuntime.System.GetType("double");
        PrimValue = default;
    }
    
    public ManagedDouble(double value)
    {
        Type = ManagedRuntime.System.GetType("double");
        PrimValue = value;
    }
    
    public unsafe ManagedDouble(byte[] value)
    {
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        Type = ManagedRuntime.System.GetType("double");
        fixed (byte* ptr = value)
        {
            PrimValue = *(double*)ptr;
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
                return new ManagedDouble(result);
            }
            case OpCode.Sub:
            {
                var result = PrimValue - (dynamic)prim.Value!;
                return new ManagedDouble(result);
            }
            case OpCode.Mul:
            {
                var result = PrimValue * (dynamic)prim.Value!;
                return new ManagedDouble(result);
            }
            case OpCode.Div:
            {
                var result = PrimValue / (dynamic)prim.Value!;
                return new ManagedDouble(result);
            }
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedDouble");
            }
        }
    }
}