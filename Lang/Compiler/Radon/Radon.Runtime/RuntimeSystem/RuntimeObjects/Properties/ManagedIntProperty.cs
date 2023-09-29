using System;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;
using UAssetAPI.PropertyTypes.Objects;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects.Properties;

internal sealed class ManagedIntProperty : RuntimeObject
{
    public override RuntimeType Type { get; }
    public override int Size { get; } // The size in bytes of the array on the heap. This includes the 4 bytes for the length.
    public override nuint Address { get; } // The address of the array on the heap.
    public IntPropertyData IntPropertyData { get; }

    public ManagedIntProperty(IntPropertyData? value, nuint pointer)
    {
        IntPropertyData = value ?? new IntPropertyData();
        Address = pointer;
        Type = ManagedRuntime.System.GetType("IntProperty");
        Size = Type.Size;
    }

    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (other is not ManagedIntProperty otherArchive)
        {
            throw new InvalidOperationException("Cannot perform an operation on a int and a non-int.");
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

        throw new InvalidOperationException($"Cannot perform operation {operation} on a int.");
    }

    public override RuntimeObject CopyTo(nuint address)
    {
        return new ManagedIntProperty(IntPropertyData, address);
    }


    public override string ToString()
    {
        return IntPropertyData.Value.ToString();
    }
}