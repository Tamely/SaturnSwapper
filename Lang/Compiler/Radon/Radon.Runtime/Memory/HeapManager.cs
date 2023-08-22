﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.Runtime.Memory.Exceptions;
using Radon.Runtime.Memory.Native;
using Radon.Runtime.RuntimeSystem;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;

namespace Radon.Runtime.Memory;

internal sealed class HeapManager
{
    private readonly LinkedList<FreeBlock> _freeBlocks;
    private readonly Dictionary<nuint, RuntimeObject> _allocatedObjects;
    private readonly nuint _end;
    private nuint _current;

    public ImmutableArray<RuntimeObject> Objects => _allocatedObjects.Values.ToImmutableArray();
    
    public HeapManager()
    {
        _freeBlocks = new LinkedList<FreeBlock>();
        _allocatedObjects = new Dictionary<nuint, RuntimeObject>();
        var heap = (nuint)PInvoke.VirtualAlloc(nint.Zero, MemoryUtils.HeapSize,
            AllocationType.COMMIT | AllocationType.RESERVE, MemoryProtection.READWRITE);
        _end = heap + MemoryUtils.HeapSize;
        _current = heap;
    }
    
    public void FreeHeap()
    {
        if (!PInvoke.VirtualFree((nint)_current,0, FreeType.MEM_RELEASE))
        {
            throw new FailedToFreeMemoryException();
        }
    }

    public RuntimeObject GetObject(nuint pointer)
    {
        if (!_allocatedObjects.TryGetValue(pointer, out var obj))
        {
            throw new InvalidOperationException("Object does not exist.");
        }

        return obj;
    }
    
    public void SetObject(nuint pointer, RuntimeObject obj)
    {
        if (!_allocatedObjects.ContainsKey(pointer))
        {
            throw new InvalidOperationException("Object does not exist.");
        }

        _allocatedObjects[pointer] = obj;
        MemoryUtils.Copy(obj.Pointer, pointer, obj.Size);
    }
    
    public RuntimeObject AllocateObject(RuntimeType type)
    {
        var size = type.Size;
        var pointer = Allocate(size);
        var obj = type.CreateDefault(pointer);
        _allocatedObjects.Add(pointer, obj);
        return obj;
    }

    public RuntimeObject AllocateArray(RuntimeType type, int length)
    {
        if (!type.TypeInfo.IsArray)
        {
            throw new InvalidOperationException("Type is not an array.");
        }

        var underlyingType = ManagedRuntime.System.GetType(type.TypeInfo.UnderlyingType!);
        var size = underlyingType.Size * length + sizeof(int);
        var pointer = Allocate(size);
        var elements = new List<RuntimeObject>(length);
        for (var i = 0; i < length; i++)
        {
            var elementPointer = pointer + (nuint)(i * underlyingType.Size + sizeof(int));  
            var element = underlyingType.CreateDefault(elementPointer);
            elements.Add(element);
        }
        
        var obj = new ManagedArray(type, pointer, elements);
        _allocatedObjects.Add(pointer, obj);
        return obj;
    }
    
    public nuint Allocate(int size)
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
            case ManagedArray managedArray:
            {
                var elements = managedArray.Elements;
                foreach (var element in elements)
                {
                    Deallocate(element);
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

        _allocatedObjects.Remove(obj.Pointer);
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