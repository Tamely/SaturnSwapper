using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.Runtime.RuntimeInfo;
using Radon.Runtime.RuntimeSystem.RuntimeObjects;

namespace Radon.Runtime.RuntimeSystem;

internal sealed class ManagedRuntime
{
    public static ManagedRuntime System { get; private set; } = null!;
    public AssemblyInfo AssemblyInfo { get; }
    public Dictionary<TypeInfo, RuntimeType> Types { get; }
    public ImmutableDictionary<TypeInfo, RuntimeType> PrimitiveTypes { get; }
    public ManagedRuntime(AssemblyInfo assemblyInfo)
    {
        System = this;
        AssemblyInfo = assemblyInfo;
        Types = new Dictionary<TypeInfo, RuntimeType>();
        var builder = ImmutableDictionary.CreateBuilder<TypeInfo, RuntimeType>();
        foreach (var type in assemblyInfo.Types)
        {
            if (Types.ContainsKey(type))
            {
                continue;
            }
            
            var runtimeType = new RuntimeType(type);
            Types.Add(type, runtimeType);
            if (type.IsPrimitive)
            {
                builder.Add(type, runtimeType);
            }
        }
        
        foreach (var type in Types.Values)
        {
            type.Initialize();
        }
        
        PrimitiveTypes = builder.ToImmutable();
    }
    
    public RuntimeType GetType(TypeInfo typeInfo)
    {
        if (Types.TryGetValue(typeInfo, out var type))
        {
            return type;
        }
        
        throw new InvalidOperationException($"Type {typeInfo.Name} does not exist.");
    }

    public RuntimeType GetType(string name)
    {
        var typeInfo = Types.Keys.FirstOrDefault(x => x.Name == name);
        if (typeInfo is null)
        {
            throw new InvalidOperationException($"Type {name} does not exist.");
        }
        
        return GetType(typeInfo);
    }

    public RuntimeType GetType(TypeDefinition definition)
    {
        var typeInfo = Types.Keys.FirstOrDefault(x => x.Definition == definition);
        if (typeInfo is null)
        {
            throw new InvalidOperationException($"Type {definition.Name} does not exist.");
        }
        
        return GetType(typeInfo);
    }
}