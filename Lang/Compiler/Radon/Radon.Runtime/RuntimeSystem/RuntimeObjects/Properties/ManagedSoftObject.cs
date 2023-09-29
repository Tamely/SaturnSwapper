using System;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;
using UAssetAPI.PropertyTypes.Objects;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects.Properties;

internal sealed class ManagedSoftObject : RuntimeObject
{
    public override RuntimeType Type { get; }
    public override int Size { get; } // The size in bytes of the array on the heap. This includes the 4 bytes for the length.
    public override nuint Address { get; } // The address of the array on the heap.
    public SoftObjectPropertyData SoftObjectPropertyData { get; }

    public ManagedSoftObject(SoftObjectPropertyData? softObject, nuint pointer)
    {
        SoftObjectPropertyData = softObject ?? new SoftObjectPropertyData();
        Address = pointer;
        Type = ManagedRuntime.System.GetType("SoftObjectProperty");
        Size = Type.Size;
    }

    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (other is not ManagedSoftObject otherArchive)
        {
            throw new InvalidOperationException("Cannot perform an operation on a soft object and a non-soft object.");
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

        throw new InvalidOperationException($"Cannot perform operation {operation} on a soft object.");
    }

    public override RuntimeObject CopyTo(nuint address)
    {
        MemoryUtils.Copy(Address, address, Size);
        return new ManagedSoftObject(SoftObjectPropertyData, address);
    }


    public override string ToString()
    {
        return SoftObjectPropertyData.Value.AssetPath.AssetName + "." + SoftObjectPropertyData.Value.AssetPath.PackageName;
    }
}