using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Radon.CodeAnalysis.Disassembly;
using Radon.Common;
using Radon.Runtime.RuntimeSystem;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;

namespace Radon.Runtime.Memory;

public sealed class StackFrame
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
    public int ArgumentCount => _arguments.Count;
    public RuntimeObject? This { get; }
    public RuntimeObject? ReturnObject { get; set; }
    public ImmutableArray<RuntimeObject> Variables => _variables.Values.ToImmutableArray();

    public StackFrame(int stackSize, int maxStack, nuint pointer, RuntimeObject? instance, ImmutableArray<LocalInfo> locals, 
        ReadOnlyDictionary<ParameterInfo, RuntimeObject> arguments)
    {
        _freeBlocks = new LinkedList<FreeBlock>();
        _arguments = new Dictionary<ParameterInfo, nuint>(arguments.Count);
        _locals = new Dictionary<LocalInfo, nuint>(locals.Length);
        _variables = new Dictionary<nuint, RuntimeObject>(_arguments.Count + _locals.Count);
        _evaluationStack = new Stack<RuntimeObject>(maxStack);
        _end = pointer + (nuint)stackSize;
        _current = pointer;
        MaxStack = maxStack;
        This = instance;
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
        Logger.Log($"Pushing value at {value.Pointer} onto the stack.", LogLevel.Info);
        _evaluationStack.Push(value);
    }

    public RuntimeObject Pop()
    {
        var value = _evaluationStack.Pop();
        Logger.Log($"Popping value at {value.Pointer} from the stack.", LogLevel.Info);
        return value;
    }

    public void SetArgument(ParameterInfo parameter, RuntimeObject value)
    {
        if (!_arguments.ContainsKey(parameter))
        {
            throw new InvalidOperationException("Argument does not exist.");
        }

        var address = _arguments[parameter];
        var variable = value.CopyTo(address);
        _variables[address] = variable;
        Logger.Log($"Setting argument '{parameter.Name}'", LogLevel.Info);
        MemoryUtils.Copy(value.Pointer, address, value.Size);
    }

    public RuntimeObject GetArgument(ParameterInfo parameter)
    {
        if (!_arguments.ContainsKey(parameter))
        {
            throw new InvalidOperationException("Argument does not exist.");
        }

        var address = _arguments[parameter];
        Logger.Log($"Getting argument '{parameter.Name}'", LogLevel.Info);
        return _variables[address];
    }
    
    public RuntimeObject GetArgument(int index)
    {
        var parameter = _arguments.Keys.FirstOrDefault(param => param.Ordinal == index);
        if (parameter is null)
        {
            throw new InvalidOperationException("Argument does not exist.");
        }
        
        var address = _arguments[parameter];
        Logger.Log($"Getting argument '{parameter.Name}'", LogLevel.Info);
        return _variables[address];
    }
    
    public RuntimeObject GetArgumentAddress(ParameterInfo parameter, RuntimeType ptrType)
    {
        if (!_arguments.ContainsKey(parameter))
        {
            throw new InvalidOperationException("Argument does not exist.");
        }
        
        var address = _arguments[parameter];
        Logger.Log($"Getting address of argument '{parameter.Name}'", LogLevel.Info);
        return AllocatePointer(ptrType, address);
    }

    public void SetLocal(LocalInfo local, RuntimeObject value)
    {
        if (!_locals.ContainsKey(local))
        {
            throw new InvalidOperationException("Local does not exist.");
        }

        var address = _locals[local];
        var variable = value.CopyTo(address);
        _variables[address] = variable;
        Logger.Log($"Setting local '{local.Name}'", LogLevel.Info);
        MemoryUtils.Copy(value.Pointer, address, value.Size);
    }

    public RuntimeObject GetLocal(LocalInfo local)
    {
        if (!_locals.ContainsKey(local))
        {
            throw new InvalidOperationException("Local does not exist.");
        }

        var address = _locals[local];
        Logger.Log($"Getting local '{local.Name}'", LogLevel.Info);
        return _variables[address];
    }
    
    public RuntimeObject GetLocalAddress(LocalInfo local, RuntimeType ptrType)
    {
        if (!_locals.ContainsKey(local))
        {
            throw new InvalidOperationException("Local does not exist.");
        }
        
        var address = _locals[local];
        Logger.Log($"Getting address of local '{local.Name}'", LogLevel.Info);
        return AllocatePointer(ptrType, address);
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
    
    public unsafe RuntimeObject AllocatePointer(RuntimeType type, nuint target)
    {
        var address = Allocate(type.Size);
        if (!type.TypeInfo.IsPointer)
        {
            throw new InvalidOperationException("The type of a pointer must be a pointer type.");
        }
        
        *(ulong*)address = target;
        return new ManagedPointer(type, address, target);
    }

    public unsafe RuntimeObject AllocateObject(RuntimeType type, bool isDefault = false)
    {
        var address = Allocate(type.Size);
        if (isDefault)
        {
            return type.CreateDefault(address);
        }
        
        if (type.TypeInfo.IsValueType)
        {
            var runtimeObject = new ManagedObject(type, type.Size, address);
            return runtimeObject;
        }

        var obj = ManagedRuntime.HeapManager.AllocateObject(type);
        *(ulong*)address = obj.Pointer;
        return new ManagedReference(type, address, obj.Pointer);
    }

    public unsafe RuntimeObject AllocateArray(RuntimeType type, int length, bool isDefault = false)
    {
        if (!type.TypeInfo.IsArray)
        {
            throw new InvalidOperationException("The type must be an array type.");
        }

        var address = Allocate(type.Size); // 8 bytes
        if (isDefault)
        {
            return type.CreateDefault(address);
        }
        
        var array = ManagedRuntime.HeapManager.AllocateArray(type, length);
        *(ulong*)address = array.Pointer;
        return new ManagedReference(type, address, array.Pointer);
    }

    public nuint Allocate(int size)
    {
        Logger.Log($"Allocating {size} bytes on the stack.", LogLevel.Info);
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
        Logger.Log($"Deallocating object at {obj.Pointer}", LogLevel.Info);
        Free(obj);
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

    public void DeallocateIfDead(RuntimeObject obj)
    {
        Logger.Log($"Attempting to clean up object", LogLevel.Info);
        if (!obj.IsDeadObject())
        {
            return;
        }
        
        Free(obj);
        switch (obj)
        {
            case ManagedObject managedObject:
            {
                var fields = managedObject.Fields;
                foreach (var field in fields)
                {
                    DeallocateIfDead(field);
                }

                break;
            }
            case ManagedReference managedReference:
            {
                try
                {
                    if (managedReference.Target == nuint.Zero)
                    {
                        return;
                    }
                    
                    var heapObject = ManagedRuntime.HeapManager.GetObject(managedReference.Target);
                    ManagedRuntime.HeapManager.DeallocateIfDead(heapObject);
                }
                catch (InvalidOperationException)
                {
                    // If the object doesn't exist, an exception will be thrown
                    ManagedRuntime.StaticHeapManager.GetObject(managedReference.Target);
                    // We won't deallocate, because it's a static object
                }
                
                break;
            }
        }
    }

    private void Free(RuntimeObject obj)
    {
        var address = obj.Pointer;
        var size = obj.Size;
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