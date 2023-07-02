using System;
using System.Collections.Generic;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedArray : RuntimeObject
{
    // <index, element>
    private readonly Dictionary<int, IRuntimeObject> _elements;
    public int Length { get; } // Length of the array
    public override int Size { get; }
    public override RuntimeType Type { get; }
    public override object? Value => null;

    public ManagedArray(RuntimeType type, int length)
    {
        var typeInfo = type.TypeInfo;
        if (!typeInfo.IsArray || typeInfo.UnderlyingType is null)
        {
            throw new ArgumentException("Type must be an array type", nameof(type));
        }
        
        if (typeInfo.UnderlyingType is null)
        {
            throw new InvalidOperationException("Cannot resolve size of array with no underlying type");
        }
        
        Size = -1;
        Type = type;
        Length = length;
        var elements = new Dictionary<int, IRuntimeObject>();
        for (var i = 0; i < length; i++)
        {
            var defaultElement = IRuntimeObject.CreateDefault(ManagedRuntime.System.GetType(typeInfo.UnderlyingType));
            elements.Add(i, defaultElement);
        }
        
        _elements = elements;
    }

    public override int ResolveSize()
    {
        if (Size != -1)
        {
            return Size;
        }

        var size = 0;
        for (var i = 0; i < Length; i++)
        {
            size += _elements[i].ResolveSize();
        }
        
        return size;
    }

    public override byte[] Serialize()
    {
        var bytes = new List<byte>();
        var lengthBytes = BitConverter.GetBytes(Length);
        bytes.AddRange(lengthBytes);
        foreach (var element in _elements)
        {
            bytes.AddRange(element.Value.Serialize());
        }
        
        return bytes.ToArray();
    }

    public override IRuntimeObject? ComputeOperation(OpCode operation, IRuntimeObject? other)
    {
        return null;
    }

    public override IRuntimeObject? ConvertTo(RuntimeType type)
    {
        return null;
    }

    public void SetElement(int index, IRuntimeObject value)
    {
        if (_elements.ContainsKey(index))
        {
            throw new IndexOutOfRangeException($"Index {index} is out of range for array of length {Length}");
        }
        
        _elements[index] = value;
    }
    
    public IRuntimeObject GetElement(int index)
    {
        if (!_elements.ContainsKey(index))
        {
            throw new IndexOutOfRangeException($"Index {index} is out of range for array of length {Length}");
        }
        
        return _elements[index];
    }
}