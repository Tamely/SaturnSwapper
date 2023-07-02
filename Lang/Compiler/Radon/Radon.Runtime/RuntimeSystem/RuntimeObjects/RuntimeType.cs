using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.Utilities;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed record RuntimeType
{
    private readonly Dictionary<FieldInfo, IRuntimeObject> _staticFields;
    private readonly Dictionary<EnumMemberInfo, IRuntimeObject> _enumValues;
    private bool _initialized;
    
    public TypeInfo TypeInfo { get; }

    public RuntimeType(TypeInfo typeInfo)
    {
        TypeInfo = typeInfo;
        _staticFields = new Dictionary<FieldInfo, IRuntimeObject>();
        var staticFields = typeInfo.Fields.Where(f => f.IsStatic);
        foreach (var field in staticFields)
        {
            var defaultObject = IRuntimeObject.CreateDefault(ManagedRuntime.System.GetType(field.Type));
            _staticFields.Add(field, defaultObject);
        }

        _enumValues = new Dictionary<EnumMemberInfo, IRuntimeObject>();
        if (typeInfo.IsEnum)
        {
            var enumValues = typeInfo.EnumMembers;
            foreach (var enumValue in enumValues)
            {
                var defaultObject = IRuntimeObject.CreateDefault(ManagedRuntime.System.GetType(enumValue.Type));
                _enumValues.Add(enumValue, defaultObject);
            }
        }
    }
    
    public void Initialize()
    {
        foreach (var (enumMember, _) in _enumValues)
        {
            _enumValues[enumMember] = IRuntimeObject.Deserialize(ManagedRuntime.System.GetType(enumMember.Type), enumMember.Value);
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

        var runtime = new MethodRuntime(ManagedRuntime.System.AssemblyInfo, IRuntimeObject.Null(this), staticConstructor, ImmutableArray<IRuntimeObject>.Empty);
        runtime.Invoke();
    }
    
    public IRuntimeObject GetStaticField(MemberReferenceInfo reference)
    {
        if (reference.MemberInfo is not FieldInfo field)
        {
            throw new ArgumentException("Member reference must be a field.", nameof(reference));
        }
        
        if (!_initialized)
        {
            StaticInitialization();
        }
        
        return _staticFields[field];
    }
    
    public void SetStaticField(MemberReferenceInfo reference, IRuntimeObject value)
    {
        if (reference.MemberInfo is not FieldInfo field)
        {
            throw new ArgumentException("Member reference must be a field.", nameof(reference));
        }
        
        if (!_initialized)
        {
            StaticInitialization();
        }

        _staticFields[field] = value;
    }
    
    public IRuntimeObject GetEnumValue(MemberReferenceInfo reference)
    {
        if (reference.MemberInfo is not EnumMemberInfo enumMember)
        {
            throw new ArgumentException("Member reference must be an enum value.", nameof(reference));
        }
        
        if (!_initialized)
        {
            StaticInitialization();
        }

        return _enumValues[enumMember];
    }

    public IRuntimeObject InvokeStaticMethod(AssemblyInfo assembly, MethodInfo method, ImmutableArray<IRuntimeObject> arguments)
    {
        if (!_initialized)
        {
            StaticInitialization();
        }

        var runtime = new MethodRuntime(assembly, IRuntimeObject.Null(this), method, arguments);
        return runtime.Invoke();
    }
    
    public ManagedArray CreateArray(int length)
    {
        return new ManagedArray(this, length);
    }

    public IRuntimeObject CreateInstance(AssemblyInfo assembly, MemberReferenceInfo constructorReference, ImmutableArray<IRuntimeObject> arguments)
    {
        if (constructorReference.MemberInfo is not MethodInfo constructor)
        {
            throw new ArgumentException("Member reference must be a constructor.", nameof(constructorReference));
        }
        
        var managedObject = new ManagedObject(this);
        var runtime = new MethodRuntime(assembly, managedObject, constructor, arguments);
        runtime.Invoke();
        return managedObject;
    }
}