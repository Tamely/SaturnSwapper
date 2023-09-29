using System;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;
using UAssetAPI.PropertyTypes.Objects;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects.Properties;

internal sealed class ManagedFloatProperty : RuntimeObject
{
    public override RuntimeType Type { get; }
    public override int Size { get; } // The size in bytes of the array on the heap. This includes the 4 bytes for the length.
    public override nuint Address { get; } // The address of the array on the heap.
    public FloatPropertyData FloatPropertyData { get; }

    public ManagedFloatProperty(FloatPropertyData? value, nuint pointer)
    {
        FloatPropertyData = value ?? new FloatPropertyData();
        Address = pointer;
        Type = ManagedRuntime.System.GetType("FloatProperty");
        Size = Type.Size;
    }

    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (other is not ManagedFloatProperty otherArchive)
        {
            throw new InvalidOperationException("Cannot perform an operation on a float and a non-float.");
        }

        switch (operation)
        {
            case OpCode.Ceq:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, Address == otherArchive.Address);
            }
            case OpCode.Cne:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, Address != otherArchive.Address);
            }
        }

        throw new InvalidOperationException($"Cannot perform operation {operation} on a float.");
    }

    public override RuntimeObject CopyTo(nuint address)
    {
        return new ManagedFloatProperty(FloatPropertyData, address);
    }


    public override string ToString()
    {
        return FloatPropertyData.Value.ToString();
    }
}