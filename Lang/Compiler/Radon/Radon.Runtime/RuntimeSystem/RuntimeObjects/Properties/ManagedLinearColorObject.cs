using System;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;
using UAssetAPI.PropertyTypes.Structs;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects.Properties;

internal sealed class ManagedLinearColorObject : RuntimeObject
{
    public override RuntimeType Type { get; }
    public override int Size { get; } // The size in bytes of the array on the heap. This includes the 4 bytes for the length.
    public override nuint Address { get; } // The address of the array on the heap.
    public LinearColorPropertyData LinearColorPropertyData { get; }

    public ManagedLinearColorObject(LinearColorPropertyData? linearColorPropertyData, nuint pointer)
    {
        LinearColorPropertyData = linearColorPropertyData ?? new LinearColorPropertyData();
        Address = pointer;
        Type = ManagedRuntime.LinearColor;
        Size = Type.Size;
    }

    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (other is not ManagedSoftObject otherArchive)
        {
            throw new InvalidOperationException("Cannot perform an operation on a color object and a non-color object.");
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

        throw new InvalidOperationException($"Cannot perform operation {operation} on a color object.");
    }

    public override RuntimeObject CopyTo(nuint address)
    {
        MemoryUtils.Copy(Address, address, Size);
        return new ManagedLinearColorObject(LinearColorPropertyData, address);
    }


    public override string ToString()
    {
        return LinearColorPropertyData.Value.ToString() ?? "null";
    }
}