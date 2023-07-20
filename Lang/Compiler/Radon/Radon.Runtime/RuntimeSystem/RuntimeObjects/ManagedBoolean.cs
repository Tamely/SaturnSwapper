using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedBoolean : ManagedPrimitive<bool>
{
    public override RuntimeType Type { get; }
    public override bool PrimValue { get; protected set; }
    
    public ManagedBoolean()
    {
        Type = ManagedRuntime.System.GetType("bool");
        PrimValue = default;
    }
    
    public ManagedBoolean(bool value)
    {
        Type = ManagedRuntime.System.GetType("bool");
        PrimValue = value;
    }
    
    public unsafe ManagedBoolean(byte[] value)
    {
        Type = ManagedRuntime.System.GetType("bool");
        if (value.Length != Size)
        {
            throw new ArgumentException($"Value must be {Size} bytes long", nameof(value));
        }
        
        fixed (byte* ptr = value)
        {
            PrimValue = *(bool*)ptr;
        }
    }
    
    public override IRuntimeObject ComputeOperation(OpCode operation, IRuntimeObject? other)
    {
        if (other is not ManagedBoolean)
        {
            throw new ArgumentException("Other must be a ManagedBool", nameof(other));
        }
        
        switch (operation)
        {
            default:
            {
                throw new InvalidOperationException($"Operation {operation} is not supported for ManagedBool");
            }
        }
    }
}