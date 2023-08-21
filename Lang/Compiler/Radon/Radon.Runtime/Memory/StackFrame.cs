using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Radon.CodeAnalysis.Disassembly;
using Radon.Runtime.RuntimeSystem;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;

namespace Radon.Runtime.Memory;

internal sealed class StackFrame
{
    private readonly LinkedList<FreeBlock> _freeBlocks;
    private readonly Dictionary<ParameterInfo, nuint> _arguments;
    private readonly Dictionary<LocalInfo, nuint> _locals;
    private readonly Dictionary<nuint, RuntimeObject> _variables;
    private readonly Stack<RuntimeObject> _evaluationStack;
    private readonly nuint _end;
    private nuint _current;

    public int EvaluationStackSize => _evaluationStack.Count;
    public int MaxStack { get; }
    public RuntimeObject? ReturnObject { get; set; }

    public ImmutableArray<RuntimeObject> Variables => _variables.Values.ToImmutableArray();

    public StackFrame(int stackSize, int maxStack, nuint pointer, RuntimeObject? instance,
        ImmutableArray<LocalInfo> locals, ReadOnlyDictionary<ParameterInfo, RuntimeObject> arguments)
    {
        _freeBlocks = new LinkedList<FreeBlock>();
        _arguments = new Dictionary<ParameterInfo, nuint>(arguments.Count);
        _locals = new Dictionary<LocalInfo, nuint>(locals.Length);
        _variables = new Dictionary<nuint, RuntimeObject>(_arguments.Count + _locals.Count);
        _evaluationStack = new Stack<RuntimeObject>(maxStack);
        _end = pointer + (nuint)stackSize;
        _current = pointer;
        MaxStack = maxStack;
        foreach (var (parameter, value) in arguments)
        {
            var type = ManagedRuntime.System.GetType(parameter.Type);
            var address = Allocate(type.Size);
            var copy = value.Type.CreateDefault(address);
            MemoryUtils.Copy(value.Pointer, address, value.Size);
            _arguments.Add(parameter, address);
            _variables.Add(address, copy);
        }

        foreach (var local in locals)
        {
            var type = ManagedRuntime.System.GetType(local.Type);
            var address = Allocate(type.Size);
            _locals.Add(local, address);
            var value = type.CreateDefault(address);
            _variables.Add(address, value);
        }
    }

    public void Push(RuntimeObject value)
    {
        _evaluationStack.Push(value);
    }

    public RuntimeObject Pop()
    {
        var value = _evaluationStack.Pop();
        return value;
    }

    public void SetArgument(ParameterInfo parameter, RuntimeObject value)
    {
        if (!_arguments.ContainsKey(parameter))
        {
            throw new InvalidOperationException("Argument does not exist.");
        }

        var address = _arguments[parameter];
        _variables[address] = value;
        MemoryUtils.Copy(value.Pointer, address, value.Size);
    }

    public RuntimeObject GetArgument(ParameterInfo parameter)
    {
        if (!_arguments.ContainsKey(parameter))
        {
            throw new InvalidOperationException("Argument does not exist.");
        }

        var address = _arguments[parameter];
        return _variables[address];
    }

    public void SetLocal(LocalInfo local, RuntimeObject value)
    {
        if (!_locals.ContainsKey(local))
        {
            throw new InvalidOperationException("Local does not exist.");
        }

        var address = _locals[local];
        _variables[address] = value;
        MemoryUtils.Copy(value.Pointer, address, value.Size);
    }

    public RuntimeObject GetLocal(LocalInfo local)
    {
        if (!_locals.ContainsKey(local))
        {
            throw new InvalidOperationException("Local does not exist.");
        }

        var address = _locals[local];
        return _variables[address];
    }

    public unsafe RuntimeObject AllocateConstant(RuntimeType type, byte[] value)
    {
        var address = Allocate(type.Size);
        if (!type.TypeInfo.IsValueType)
        {
            throw new InvalidOperationException("The type of a constant must be a value type.");
        }

        if (value.Length > type.Size)
        {
            throw new InvalidOperationException("The size of the value is greater than the size of the type.");
        }

        fixed (byte* ptr = value)
        {
            MemoryUtils.Copy((nuint)ptr, address, value.Length);
        }

        return new ManagedObject(type, type.Size, address);
    }

    public unsafe RuntimeObject AllocatePrimitive<T>(RuntimeType type, T value)
        where T : unmanaged
    {
        if (type.Size < sizeof(T))
        {
            throw new InvalidOperationException("The size of the type is less than the size of the value.");
        }

        var address = Allocate(type.Size);
        if (!type.TypeInfo.IsValueType)
        {
            throw new InvalidOperationException("The type of a constant must be a value type.");
        }

        *(T*)address = value;
        return new ManagedObject(type, type.Size, address);
        ;
    }

    public unsafe RuntimeObject AllocateString(string str)
    {
        var type = ManagedRuntime.String;
        var address = Allocate(type.Size);
        // In the case of the Radon Runtime, the string type itself is not a reference type
        // However, the character array that it contains is a reference type
        var array = (ManagedReference)AllocateArray(ManagedRuntime.CharArray, str.Length);
        var firstElement = array.Target + sizeof(int);
        for (var i = 0; i < str.Length; i++)
        {
            var c = str[i];
            *(byte*)(firstElement + (nuint)i) = (byte)c;
        }

        *(ulong*)address = array.Pointer;
        return new ManagedString(type, address);
    }

    public unsafe RuntimeObject AllocateObject(RuntimeType type)
    {
        var address = Allocate(type.Size);
        if (type.TypeInfo.IsValueType)
        {
            var runtimeObject = new ManagedObject(type, type.Size, address);
            return runtimeObject;
        }

        var obj = ManagedRuntime.HeapManager.AllocateObject(type);
        *(ulong*)address = obj.Pointer;
        return new ManagedReference(type, address, obj.Pointer);
    }

    public unsafe RuntimeObject AllocateArray(RuntimeType type, int length)
    {
        if (!type.TypeInfo.IsArray)
        {
            throw new InvalidOperationException("The type must be an array type.");
        }

        var address = Allocate(type.Size); // 8 bytes
        var array = ManagedRuntime.HeapManager.AllocateArray(type, length);
        *(ulong*)address = array.Pointer;
        return new ManagedReference(type, address, array.Pointer);
    }

    private nuint Allocate(int size)
    {
        var current = _freeBlocks.First;
        while (current is not null)
        {
            var block = current.Value;
            if (block.Size >= size)
            {
                var pointer = block.Pointer;
                if (block.Size == size)
                {
                    _freeBlocks.Remove(current);
                }
                else
                {
                    // we didn't consume the entire block, so we need to split it
                    var newPointer = pointer + (nuint)size;
                    var newSize = block.Size - size;
                    _freeBlocks.AddAfter(current, new FreeBlock(newPointer, newSize));
                    // we need to remove the old block
                    _freeBlocks.Remove(current);
                }
            }

            current = current.Next;
        }

        if (_current + (nuint)size > _end)
        {
            throw new StackOverflowException();
        }

        var address = _current;
        _current += (nuint)size;
        return address;
    }

    public void Deallocate(RuntimeObject obj)
    {
        Free(obj.Pointer, obj.Size);
        switch (obj)
        {
            case ManagedObject managedObject:
            {
                var fields = managedObject.Fields;
                foreach (var field in fields)
                {
                    Deallocate(field);
                }

                break;
            }
            case ManagedReference managedReference:
            {
                var heapObject = ManagedRuntime.HeapManager.GetObject(managedReference.Target);
                ManagedRuntime.HeapManager.Deallocate(heapObject);
                break;
            }
        }
    }

    private void Free(nuint address, int size)
    {
        var block = new FreeBlock(address, size);
        var current = _freeBlocks.First;
        var merged = false;
        while (current is not null)
        {
            var next = current.Next;
            if (current.Value.Pointer + (nuint)current.Value.Size == block.Pointer && next is not null)
            {
                // We can merge the blocks
                var newBlock = new FreeBlock(current.Value.Pointer, current.Value.Size + block.Size);
                _freeBlocks.AddBefore(next, newBlock);
                _freeBlocks.Remove(current);
                _freeBlocks.Remove(next);
                merged = true;
            }

            current = next;
        }

        if (!merged)
        {
            _freeBlocks.AddLast(block);
        }
    }
}