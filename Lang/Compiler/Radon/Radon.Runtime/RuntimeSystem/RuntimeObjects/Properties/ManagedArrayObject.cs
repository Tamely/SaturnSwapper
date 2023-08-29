using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;
using UAssetAPI.PropertyTypes.Objects;
using System;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects.Properties;

internal sealed class ManagedArrayObject : RuntimeObject
{
    public override RuntimeType Type { get; }
    public override int Size { get; } // The size in bytes of the array on the heap. This includes the 4 bytes for the length.
    public override UIntPtr Pointer { get; } // The address of the array on the heap.
    public ArrayPropertyData ArrayPropertyData { get; }

    public ManagedArrayObject(ArrayPropertyData array, UIntPtr pointer)
    {
        ArrayPropertyData = array;
        Pointer = pointer;
        Type = ManagedRuntime.System.GetType("ArrayProperty");
        Size = Type.Size;
    }

    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (other is not ManagedSoftObject otherArchive)
        {
            throw new InvalidOperationException("Cannot perform an operation on a array object and a non-array object.");
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

        throw new InvalidOperationException($"Cannot perform operation {operation} on a array object.");
    }
    

    public override string ToString()
    {
        return ArrayPropertyData.ArrayType.Value.Value;
    }
}