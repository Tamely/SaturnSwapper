using System;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;
using UAssetAPI.PropertyTypes.Objects;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects.Properties;

internal sealed class ManagedSoftObject : RuntimeObject
{
    public override RuntimeType Type { get; }
    public override int Size { get; } // The size in bytes of the array on the heap. This includes the 4 bytes for the length.
    public override nuint Pointer { get; } // The address of the array on the heap.
    public SoftObjectPropertyData SoftObjectPropertyData { get; }

    public ManagedSoftObject(SoftObjectPropertyData softObject, nuint pointer)
    {
        SoftObjectPropertyData = softObject;
        Pointer = pointer;
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
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, Pointer == otherArchive.Pointer);
            }
            case OpCode.Cne:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, Pointer != otherArchive.Pointer);
            }
        }

        throw new InvalidOperationException($"Cannot perform operation {operation} on a soft object.");
    }


    public override string ToString()
    {
        return SoftObjectPropertyData.Value.AssetPath.AssetName + "." + SoftObjectPropertyData.Value.AssetPath.PackageName;
    }
}