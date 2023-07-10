using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.RuntimeInfo;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedObject : RuntimeObject
{
    // <index, field>
    private readonly Dictionary<FieldInfo, IRuntimeObject> _instanceFields;
    public override int Size { get; }
    public override RuntimeType Type { get; }
    public override object? Value => null;

    public ManagedObject(RuntimeType type)
    {
        var typeInfo = type.TypeInfo;
        if (typeInfo.IsArray)
        {
            throw new ArgumentException("Cannot create a managed object from an array type.");
        }
        
        Size = typeInfo.Size;
        Type = type;
        var fields = new Dictionary<FieldInfo, IRuntimeObject>();
        foreach (var field in typeInfo.Fields)
        {
            var defaultObject = IRuntimeObject.CreateDefault(ManagedRuntime.System.GetType(field.Type));
            fields.Add(field, defaultObject);
        }
        
        _instanceFields = fields;
    }

    public override int ResolveSize()
    {
        return Size != -1 ? Size : _instanceFields.Values.Sum(field => field.ResolveSize());
    }

    public override byte[] Serialize()
    {
        var bytes = new List<byte>();
        foreach (var field in _instanceFields)
        {
            bytes.AddRange(field.Value.Serialize());
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

    public IRuntimeObject GetField(MemberReferenceInfo reference)
    {
        if (reference.MemberInfo is not FieldInfo field)
        {
            throw new Exception("The member reference is not a field.");
        }
        
        if (!_instanceFields.ContainsKey(field))
        {
            throw new InvalidOperationException($"Field {field.Name} does not exist.");
        }
        
        return _instanceFields[field];
    }

    public void SetField(MemberReferenceInfo reference, IRuntimeObject runtimeObject)
    {
        if (reference.MemberInfo is not FieldInfo field)
        {
            throw new Exception("The member reference is not a field.");
        }

        if (!_instanceFields.ContainsKey(field))
        {
            throw new InvalidOperationException($"Field {field.Name} does not exist.");
        }
        
        _instanceFields[field] = runtimeObject;
    }
    
    public void SetField(FieldInfo field, IRuntimeObject runtimeObject)
    {
        if (!_instanceFields.ContainsKey(field))
        {
            throw new InvalidOperationException($"Field {field.Name} does not exist.");
        }
        
        _instanceFields[field] = runtimeObject;
    }

    public IRuntimeObject InvokeMethod(AssemblyInfo assembly, MethodInfo method, ImmutableArray<IRuntimeObject> arguments)
    {
        var methodRuntime = new MethodRuntime(assembly, this, method, arguments);
        return methodRuntime.Invoke();
    }
}