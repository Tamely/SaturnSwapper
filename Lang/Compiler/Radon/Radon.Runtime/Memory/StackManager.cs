using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Radon.CodeAnalysis.Disassembly;
using Radon.Common;
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
        Logger.Log("Allocating stack...", LogLevel.Info);
        _stack = (nuint)PInvoke.VirtualAlloc(nint.Zero, MemoryUtils.StackSize,
            AllocationType.COMMIT | AllocationType.RESERVE, MemoryProtection.READWRITE);
        _stackFrames = new Stack<StackFrame>();
        _stackPointer = _stack;
        Logger.Log($"Allocated {MemoryUtils.StackSize} bytes for the stack", LogLevel.Info);
    }
    
    public void FreeStack()
    {
        if (!PInvoke.VirtualFree((nint)_stack, 0, FreeType.MEM_RELEASE))
        {
            throw new FailedToFreeMemoryException();
        }
    }

    public StackFrame AllocateStackFrame(int stackSize, int maxStack,ImmutableArray<LocalInfo> locals, 
        ReadOnlyDictionary<ParameterInfo, RuntimeObject> arguments)
    {
        Logger.Log($"Allocating stack frame of size {stackSize} with {locals.Length} locals and {arguments.Count} arguments", LogLevel.Info);
        if (_stackPointer + (nuint)stackSize >= _stack + MemoryUtils.StackSize)
        {
            throw new StackOverflowException();
        }
        
        var stackFrame = new StackFrame(stackSize, maxStack, _stackPointer, locals, arguments);
        _stackFrames.Push(stackFrame);
        _stackPointer += (nuint)stackSize;
        return stackFrame;
    }

    public void DeallocateStackFrame()
    {
        Logger.Log("Deallocating stack frame...", LogLevel.Info);
        var stackFrame = _stackFrames.Pop();
        _stackPointer -= (nuint)stackFrame.MaxStack;
        var variables = stackFrame.Variables;
        Logger.Log("Attempting to deallocate allocated objects", LogLevel.Info);
        foreach (var variable in variables)
        {
            Logger.Log("Checking if object is dead...", LogLevel.Info);
            if (variable.IsDeadObject())
            {
                Logger.Log("Object is dead, deallocating...", LogLevel.Info);
                stackFrame.Deallocate(variable);
            }
        }
    }
}