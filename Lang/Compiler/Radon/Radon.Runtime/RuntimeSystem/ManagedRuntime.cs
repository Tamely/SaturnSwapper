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
    
    public static StackManager StackManager { get; private set; }
    public static HeapManager HeapManager { get; private set; }
    public static HeapManager StaticHeapManager { get; private set; }
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
    
    public static RuntimeType Void => System.GetTypeFromVersion("void", RadonVersion.RADON_LATEST);
    public static RuntimeType Boolean => System.GetTypeFromVersion("bool", RadonVersion.RADON_LATEST);
    public static RuntimeType Int8 => System.GetTypeFromVersion("sbyte", RadonVersion.RADON_LATEST);
    public static RuntimeType UInt8 => System.GetTypeFromVersion("byte", RadonVersion.RADON_LATEST);
    public static RuntimeType Int16 => System.GetTypeFromVersion("short", RadonVersion.RADON_LATEST);
    public static RuntimeType UInt16 => System.GetTypeFromVersion("ushort", RadonVersion.RADON_LATEST);
    public static RuntimeType Int32 => System.GetTypeFromVersion("int", RadonVersion.RADON_LATEST);
    public static RuntimeType UInt32 => System.GetTypeFromVersion("uint", RadonVersion.RADON_LATEST);
    public static RuntimeType Int64 => System.GetTypeFromVersion("long", RadonVersion.RADON_LATEST);
    public static RuntimeType UInt64 => System.GetTypeFromVersion("ulong", RadonVersion.RADON_LATEST);
    public static RuntimeType Float32 => System.GetTypeFromVersion("float", RadonVersion.RADON_LATEST);
    public static RuntimeType Float64 => System.GetTypeFromVersion("double", RadonVersion.RADON_LATEST);
    public static RuntimeType Char => System.GetTypeFromVersion("char", RadonVersion.RADON_LATEST);
    public static RuntimeType String => System.GetTypeFromVersion("string", RadonVersion.RADON_LATEST);
    public static RuntimeType Archive => System.GetTypeFromVersion("archive", RadonVersion.RADON_LATEST);
    public static RuntimeType CharArray => System.GetTypeFromVersion("char[]", RadonVersion.RADON_LATEST);
    public static RuntimeType SoftObject => System.GetTypeFromVersion("SoftObjectProperty", RadonVersion.RADON_LATEST);
    public static RuntimeType LinearColor => System.GetTypeFromVersion("LinearColorProperty", RadonVersion.RADON_LATEST);
    public static RuntimeType ArrayProperty => System.GetTypeFromVersion("ArrayProperty", RadonVersion.RADON_LATEST);
    public static RuntimeType ByteArrayProperty => System.GetTypeFromVersion("ByteArrayProperty", RadonVersion.RADON_LATEST);
    public static RuntimeType IntProperty => System.GetTypeFromVersion("IntProperty", RadonVersion.RADON_LATEST);
    public static RuntimeType FloatProperty => System.GetTypeFromVersion("FloatProperty", RadonVersion.RADON_LATEST);
    public static RuntimeType DoubleProperty => System.GetTypeFromVersion("DoubleProperty", RadonVersion.RADON_LATEST);

    static ManagedRuntime()
    {
        _system = null;
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
        
        StackManager = new StackManager();
        HeapManager = new HeapManager();
        StaticHeapManager = new HeapManager();
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

    public RuntimeType GetTypeFromVersion(string name, RadonVersion endVersion)
    {
        return GetTypeFromVersion(name, RadonVersion.RADON_1_0_0, endVersion);
    }
    
    public RuntimeType GetTypeFromVersion(string name, RadonVersion begin, RadonVersion end)
    {
        var version = AssemblyInfo.Version;
        // If the end version is 0, then it is the latest version.
        if (end == 0)
        {
            end = RadonVersion.RADON_LATEST;
        }
        
        if (version < begin.GetVersion() || version > end.GetVersion())
        {
            return null!;
        }
        
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