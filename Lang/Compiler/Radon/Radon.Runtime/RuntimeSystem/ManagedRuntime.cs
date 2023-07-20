using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;
using Radon.Utilities;

namespace Radon.Runtime.RuntimeSystem;

internal sealed class ManagedRuntime
{
    public static ManagedRuntime System { get; private set; } = null!;
    public AssemblyInfo AssemblyInfo { get; }
    public Dictionary<TypeInfo, RuntimeType> Types { get; }
    public ManagedRuntime(AssemblyInfo assemblyInfo)
    {
        System = this;
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

        foreach (var type in Types.Values)
        {
            type.Initialize();
        }
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