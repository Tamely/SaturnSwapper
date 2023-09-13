using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Radon.CodeAnalysis.Disassembly;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.Common;
using Radon.Runtime.Memory;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;

namespace Radon.Runtime.RuntimeSystem;

public sealed class ManagedRuntime
{
    private static ManagedRuntime? _system;
    
    public static StackManager StackManager { get; }
    public static HeapManager HeapManager { get;}
    public static HeapManager StaticHeapManager { get; }
    public static ManagedRuntime System
    {
        get
        {
            if (_system is null)
            {
                throw new InvalidOperationException("System is not initialized.");
            }

            return _system;
        }
    }
    
    public static RuntimeType Void => System.GetType("void");
    public static RuntimeType Boolean => System.GetType("bool");
    public static RuntimeType Int8 => System.GetType("sbyte");
    public static RuntimeType UInt8 => System.GetType("byte");
    public static RuntimeType Int16 => System.GetType("short");
    public static RuntimeType UInt16 => System.GetType("ushort");
    public static RuntimeType Int32 => System.GetType("int");
    public static RuntimeType UInt32 => System.GetType("uint");
    public static RuntimeType Int64 => System.GetType("long");
    public static RuntimeType UInt64 => System.GetType("ulong");
    public static RuntimeType Float32 => System.GetType("float");
    public static RuntimeType Float64 => System.GetType("double");
    public static RuntimeType Char => System.GetType("char");
    public static RuntimeType String => System.GetType("string");
    public static RuntimeType Archive => System.GetType("archive");
    public static RuntimeType CharArray => System.GetType("char[]");
    public static RuntimeType SoftObject => System.GetType("SoftObjectProperty");
    public static RuntimeType LinearColor => System.GetType("LinearColorProperty");
    public static RuntimeType ArrayProperty => System.GetType("ArrayProperty");

    static ManagedRuntime()
    {
        _system = null;
        StackManager = new StackManager();
        HeapManager = new HeapManager();
        StaticHeapManager = new HeapManager();
    }
    
    public AssemblyInfo AssemblyInfo { get; }
    public Dictionary<TypeInfo, RuntimeType> Types { get; }
    
    public ManagedRuntime(AssemblyInfo assemblyInfo)
    {
        _system = this;
        AssemblyInfo = assemblyInfo;
        Types = new Dictionary<TypeInfo, RuntimeType>();
        foreach (var type in assemblyInfo.Types)
        {
            if (Types.ContainsKey(type))
            {
                continue;
            }
            
            var runtimeType = new RuntimeType(type);
            Types.Add(type, runtimeType);
        }
    }

    public static ReadOnlyDictionary<TKey, TValue> EmptyDictionary<TKey, TValue>() 
        where TKey : notnull
    {
        return new(new Dictionary<TKey, TValue>());
    }

    public static ImmutableArray<RuntimeObject> GetRoots()
    {
        var builder = ImmutableArray.CreateBuilder<RuntimeObject>();
        foreach (var obj in StaticHeapManager.Objects)
        {
            builder.Add(obj);
        }
        
        var stacks = StackManager.StackFrames;
        foreach (var stackFrame in stacks)
        {
            foreach (var variable in stackFrame.Variables)
            {
                builder.Add(variable);
            }
            
            if (stackFrame.ReturnObject is not null)
            {
                builder.Add(stackFrame.ReturnObject);
            }

            if (stackFrame.This is not null)
            {
                builder.Add(stackFrame.This);
            }
        }
        
        return builder.ToImmutable();
    }
    
    public RuntimeType GetType(TypeInfo typeInfo)
    {
        if (Types.TryGetValue(typeInfo, out var type))
        {
            return type;
        }
        
        return CreateType(typeInfo);
    }

    public RuntimeType GetType(string name)
    {
        var typeInfo = Types.Keys.FirstOrDefault(x => x.Name == name);
        if (typeInfo is not null)
        {
            return GetType(typeInfo);
        }

        var typeDefinition = GetDefinitionByName(name);
        return GetType(typeDefinition);
    }

    public RuntimeType GetType(TypeDefinition definition)
    {
        var typeInfo = Types.Keys.FirstOrDefault(x => x.Definition == definition);
        if (typeInfo is null)
        {
            var type = AssemblyInfo.Types.FirstOrDefault(x => x.Definition == definition);
            typeInfo = type ?? throw new InvalidOperationException($"Type {definition.Name} does not exist.");
        }
        
        return GetType(typeInfo);
    }
    
    private RuntimeType CreateType(TypeInfo typeInfo)
    {
        if (Types.TryGetValue(typeInfo, out var type))
        {
            return type;
        }
        
        var runtimeType = new RuntimeType(typeInfo);
        Types.Add(typeInfo, runtimeType);
        return runtimeType;
    }

    private TypeDefinition GetDefinitionByName(string name)
    {
        var metadata = AssemblyInfo.Metadata;
        var nameIndex = metadata.Strings.Strings.IndexOf(name, StringComparer.Ordinal);
        if (nameIndex == -1)
        {
            throw new InvalidOperationException($"Type {name} does not exist.");
        }
        
        var typeDefinition = metadata.Types.Types.FirstOrDefault(x => x.Name == nameIndex);
        if (typeDefinition == default)
        {
            throw new InvalidOperationException($"Type {name} does not exist.");
        }
        
        return typeDefinition;
    }
}