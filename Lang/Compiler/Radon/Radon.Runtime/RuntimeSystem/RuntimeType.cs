using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Radon.CodeAnalysis.Disassembly;
using Radon.Common;
using Radon.Runtime.Memory;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;

namespace Radon.Runtime.RuntimeSystem;

public sealed class RuntimeType
{
    private readonly Dictionary<FieldInfo, nuint> _staticFields;
    private bool _initialized;
    
    public TypeInfo TypeInfo { get; }
    public int Size { get; }
    public IReadOnlyDictionary<FieldInfo, nuint> StaticFields => _staticFields.ToImmutableDictionary();

    public RuntimeType(TypeInfo type)
    {
        TypeInfo = type;
        Size = type.Size;
        _staticFields = new Dictionary<FieldInfo, nuint>();
        var staticFields = type.Fields.Where(f => f.IsStatic);
        foreach (var field in staticFields)
        {
            var fieldType = ManagedRuntime.System.GetType(field.Type);
            var obj = ManagedRuntime.HeapManager.AllocateObject(fieldType);
            _staticFields.Add(field, obj.Pointer);
        }
    }

    public void StaticInitialization()
    {
        _initialized = true;
        var staticConstructor = TypeInfo.StaticConstructor;
        if (staticConstructor is null)
        {
            Logger.Log("No static constructor found.", LogLevel.Warning);
            return;
        }

        var runtime = new MethodRuntime(ManagedRuntime.System.AssemblyInfo, null, staticConstructor,
            new ReadOnlyDictionary<ParameterInfo, RuntimeObject>(new Dictionary<ParameterInfo, RuntimeObject>()));
        runtime.Invoke();
    }
    
    public RuntimeObject GetStaticField(FieldInfo field)
    {
        var objPtr = _staticFields[field];
        return ManagedRuntime.StaticHeapManager.GetObject(objPtr);
    }
    
    public void SetStaticField(FieldInfo field, RuntimeObject value)
    {
        var objPtr = _staticFields[field];
        var type = ManagedRuntime.System.GetType(field.Type);
        if (value.Type != type)
        {
            throw new InvalidOperationException("Type mismatch.");
        }
        
        ManagedRuntime.StaticHeapManager.SetObject(objPtr, value);
    }

    public nuint GetStaticFieldAddress(FieldInfo field)
    {
        return _staticFields[field];
    }

    public StackFrame Construct(AssemblyInfo assembly, StackFrame stackFrame, 
        ReadOnlyDictionary<ParameterInfo, RuntimeObject> arguments, MethodInfo constructor)
    {
        if (!_initialized)
        {
            StaticInitialization();
        }
        
        var instance = stackFrame.AllocateObject(this);
        var runtime = new MethodRuntime(assembly, instance, constructor, arguments);
        var objStackFrame = runtime.Invoke();
        objStackFrame.ReturnObject = instance;
        return objStackFrame;
    }
    
    public StackFrame InvokeStatic(AssemblyInfo assembly, MethodInfo method, 
        ReadOnlyDictionary<ParameterInfo, RuntimeObject> arguments)
    {
        if (!_initialized)
        {
            StaticInitialization();
        }
        
        var runtime = new MethodRuntime(assembly, null, method, arguments);
        return runtime.Invoke();
    }
    
    public RuntimeObject CreateDefault(nuint address)
    {
        if (TypeInfo.IsReferenceType)
        {
            return new ManagedReference(this, address, 0);
        }
        
        return new ManagedObject(this, Size, address);
    }
    
    public override string ToString()
    {
        return TypeInfo.ToString();
    }
}