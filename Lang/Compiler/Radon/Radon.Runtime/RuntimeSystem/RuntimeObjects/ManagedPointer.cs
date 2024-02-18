using System;
using System.Text;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedPointer : RuntimeObject
{
    public override RuntimeType Type { get; }
    public override int Size => sizeof(ulong);
    public override nuint Address { get; }
    public nuint Target { get; set; } // The pointer to the object this pointer points to.

    public ManagedPointer(RuntimeType type, nuint pointer, nuint target)
    {
        if (!type.TypeInfo.IsPointer)
        {
            throw new InvalidOperationException("The type of a pointer must be a pointer type.");
        }
        
        Type = type;
        Address = pointer;
        Target = target;
    }
    
    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (other is not ManagedPointer otherPointer)
        {
            throw new InvalidOperationException("Cannot perform an operation on a pointer and a non-pointer.");
        }
        
        switch (operation)
        {
            case OpCode.Ceq:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, Address == otherPointer.Address);
            }
            case OpCode.Cne:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, Address != otherPointer.Address);
            }
        }
        
        throw new InvalidOperationException($"Cannot perform operation {operation} on a pointer.");
    }

    public override RuntimeObject CopyTo(nuint address)
    {
        MemoryUtils.Copy(Address, address, Size);
        return new ManagedPointer(Type, address, Target);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Type);
        sb.Append(" -> ");
        // Get hex representation of the target.
        sb.Append("0x");
        sb.Append(Target.ToString("X"));
        return sb.ToString();
    }
}