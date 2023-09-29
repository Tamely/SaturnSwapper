using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.CodeAnalysis.Disassembly;

public sealed record MethodInfo : IMemberInfo
{
    public bool IsStatic { get; }
    public bool IsEntry { get; }
    public bool IsConstructor { get; }
    public bool IsRuntimeMethod { get; }
    public string Name { get; }
    public string Fullname => ToString();
    public TypeInfo Parent { get; }
    public TypeInfo Type { get; }
    public ReadOnlyDictionary<Parameter, ParameterInfo> Parameters { get; }
    public ReadOnlyDictionary<Local, LocalInfo> Locals { get; }
    public int InstructionCount { get; }
    public int FirstInstruction { get; }
    public Method Definition { get; }
    public MethodInfo(Method method, Metadata metadata, TypeInfo parent)
    {
        IsStatic = method.Flags.HasFlag(BindingFlags.Static);
        IsEntry = method.Flags.HasFlag(BindingFlags.Entry);
        var name = metadata.Strings.Strings[method.Name];
        IsConstructor = name == ".ctor";
        IsRuntimeMethod = method.Flags.HasFlag(BindingFlags.RuntimeInternal);
        Name = name;
        Parent = TypeTracker.Add(metadata.Types.Types[method.Parent], metadata, parent);
        if (Parent != parent)
        {
            throw new InvalidOperationException("Parent type does not match the parent type of the field.");
        }
        
        Type = TypeTracker.Add(metadata.Types.Types[method.ReturnType], metadata, parent);
        var parameters = new Dictionary<Parameter, ParameterInfo>();
        var parameterCount = method.ParameterCount;
        var firstParameter = method.FirstParameter;
        for (var i = 0; i < parameterCount; i++)
        {
            var parameter = metadata.Parameters.Parameters[firstParameter + i];
            parameters.Add(parameter, new ParameterInfo(parameter, metadata, this, parent));
        }
        
        var locals = new Dictionary<Local, LocalInfo>();
        var localCount = method.LocalCount;
        var firstLocal = method.FirstLocal;
        for (var i = 0; i < localCount; i++)
        {
            var local = metadata.Locals.Locals[firstLocal + i];
            locals.Add(local, new LocalInfo(local, metadata, parent));
        }
        
        // Order the parameters and locals by their ordinal from least to greatest.
        Parameters = new ReadOnlyDictionary<Parameter, ParameterInfo>(parameters);
        Locals = new ReadOnlyDictionary<Local, LocalInfo>(locals);
        InstructionCount = method.InstructionCount;
        FirstInstruction = method.FirstInstruction;
        Definition = method;
    }
    
    public override string ToString()
    {
        var parameterString = string.Join(", ", Parameters.Select(p => p.ToString()));
        return $"{Parent}.{Name}({parameterString})";
    }
}