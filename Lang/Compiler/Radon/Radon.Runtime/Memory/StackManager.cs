using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Radon.CodeAnalysis.Disassembly;
using Radon.Runtime.Memory.Exceptions;
using Radon.Runtime.Memory.Native;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;

namespace Radon.Runtime.Memory;

internal sealed class StackManager
{
    private readonly Stack<StackFrame> _stackFrames;
    private readonly nuint _stack; // The pointer to the beginning of the stack.
    private nuint _stackPointer; // The current stack pointer.

    public ImmutableArray<StackFrame> StackFrames => _stackFrames.ToImmutableArray();
    
    public StackManager()
    {
        _stack = (nuint)PInvoke.VirtualAlloc(nint.Zero, MemoryUtils.StackSize,
            AllocationType.COMMIT | AllocationType.RESERVE, MemoryProtection.READWRITE);
        _stackFrames = new Stack<StackFrame>();
        _stackPointer = _stack;
    }
    
    public void FreeStack()
    {
        if (!PInvoke.VirtualFree((nint)_stack, 0, FreeType.MEM_RELEASE))
        {
            throw new FailedToFreeMemoryException();
        }
    }

    public StackFrame AllocateStackFrame(int stackSize, int maxStack, RuntimeObject? instance,
        ImmutableArray<LocalInfo> locals, ReadOnlyDictionary<ParameterInfo, RuntimeObject> arguments)
    {
        if (_stackPointer + (nuint)stackSize >= _stack + MemoryUtils.StackSize)
        {
            throw new StackOverflowException();
        }
        
        var stackFrame = new StackFrame(stackSize, maxStack, _stackPointer, instance, locals, arguments);
        _stackFrames.Push(stackFrame);
        _stackPointer += (nuint)stackSize;
        return stackFrame;
    }

    public void DeallocateStackFrame()
    {
        var stackFrame = _stackFrames.Pop();
        _stackPointer -= (nuint)stackFrame.MaxStack;
        var variables = stackFrame.Variables;
        foreach (var variable in variables)
        {
            if (variable.IsDeadObject())
            {
                stackFrame.Deallocate(variable);
            }
        }
    }
}