using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.Common;
using Radon.Runtime.Memory.Exceptions;
using Radon.Runtime.Memory.Native;
using Radon.Runtime.RuntimeSystem;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;

namespace Radon.Runtime.Memory;

public sealed class HeapManager
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
        Logger.Log("Allocating heap...", LogLevel.Info);
        var heap = (nuint)PInvoke.VirtualAlloc(nint.Zero, MemoryUtils.HeapSize,
            AllocationType.COMMIT | AllocationType.RESERVE, MemoryProtection.READWRITE);
        _end = heap + MemoryUtils.HeapSize;
        _current = heap;
        Logger.Log($"Allocated {MemoryUtils.HeapSize} bytes for the heap", LogLevel.Info);
    }

    public void FreeHeap()
    {
        if (!PInvoke.VirtualFree((nint)_current, 0, FreeType.MEM_RELEASE))
        {
            throw new FailedToFreeMemoryException();
        }
    }

    public RuntimeObject GetObject(nuint pointer)
    {
        Logger.Log($"Getting object at {pointer}", LogLevel.Info);
        if (!_allocatedObjects.TryGetValue(pointer, out var obj))
        {
            throw new InvalidOperationException("Object does not exist.");
        }

        return obj;
    }

    public void SetObject(nuint pointer, RuntimeObject obj)
    {
        Logger.Log($"Setting object at {pointer}", LogLevel.Info);
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
        Logger.Log($"Allocating {size} bytes on the heap...", LogLevel.Info);
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

    public void DeallocateIfDead(RuntimeObject obj)
    {
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
            case ManagedArray managedArray:
            {
                var elements = managedArray.Elements;
                foreach (var element in elements)
                {
                    DeallocateIfDead(element);
                }

                break;
            }
            case ManagedReference managedReference:
            {
                try
                {
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