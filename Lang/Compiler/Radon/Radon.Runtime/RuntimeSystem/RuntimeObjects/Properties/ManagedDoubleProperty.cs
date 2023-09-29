using System;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;
using UAssetAPI.PropertyTypes.Objects;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects.Properties;

internal sealed class ManagedDoubleProperty : RuntimeObject
{
    public override RuntimeType Type { get; }
    public override int Size { get; } // The size in bytes of the array on the heap. This includes the 4 bytes for the length.
    public override nuint Address { get; } // The address of the array on the heap.
    public DoublePropertyData DoublePropertyData { get; }

    public ManagedDoubleProperty(DoublePropertyData? value, nuint pointer)
    {
        DoublePropertyData = value ?? new DoublePropertyData();
        Address = pointer;
        Type = ManagedRuntime.System.GetType("DoubleProperty");
        Size = Type.Size;
    }

    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (other is not ManagedDoubleProperty otherArchive)
        {
            throw new InvalidOperationException("Cannot perform an operation on a double and a non-double.");
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

        throw new InvalidOperationException($"Cannot perform operation {operation} on a double.");
    }

    public override RuntimeObject CopyTo(nuint address)
    {
        return new ManagedDoubleProperty(DoublePropertyData, address);
    }


    public override string ToString()
    {
        return DoublePropertyData.Value.ToString();
    }
}