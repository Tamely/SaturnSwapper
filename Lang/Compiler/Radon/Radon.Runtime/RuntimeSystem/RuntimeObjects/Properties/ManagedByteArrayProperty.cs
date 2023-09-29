using System;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;
using UAssetAPI.PropertyFactories;
using UAssetAPI.PropertyTypes.Objects;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects.Properties;

internal sealed class ManagedByteArrayProperty : RuntimeObject
{
    public override RuntimeType Type { get; }
    public override int Size { get; } // The size in bytes of the array on the heap. This includes the 4 bytes for the length.
    public override nuint Address { get; } // The address of the array on the heap.
    public ByteArrayPropertyData ByteArrayPropertyData { get; }

    public ManagedByteArrayProperty(ByteArrayPropertyData? value, nuint pointer)
    {
        ByteArrayPropertyData = value ?? new ByteArrayPropertyData();
        Address = pointer;
        Type = ManagedRuntime.System.GetType("ByteArrayProperty");
        Size = Type.Size;
    }

    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (other is not ManagedByteArrayProperty otherArchive)
        {
            throw new InvalidOperationException("Cannot perform an operation on a byte array and a non-byte array.");
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

        throw new InvalidOperationException($"Cannot perform operation {operation} on a byte array.");
    }

    public override RuntimeObject CopyTo(nuint address)
    {
        return new ManagedByteArrayProperty(ByteArrayPropertyData, address);
    }


    public override string ToString()
    {
        return ByteArrayPropertyData.Value.ToString() ?? "null";
    }
}