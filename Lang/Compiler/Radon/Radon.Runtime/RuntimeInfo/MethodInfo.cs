using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

namespace Radon.Runtime.RuntimeInfo;

internal sealed record MethodInfo : IMemberInfo
{
    public bool IsStatic { get; }
    public bool IsConstructor { get; }
    public bool IsRuntimeMethod { get; }
    public string Name { get; }
    public TypeInfo Parent { get; }
    public TypeInfo ReturnType { get; }
    public ImmutableArray<ParameterInfo> Parameters { get; }
    public ImmutableArray<LocalInfo> Locals { get; }
    public int InstructionCount { get; }
    public int FirstInstruction { get; }
    public MethodInfo(Method method, Metadata metadata, TypeInfo parent)
    {
        var name = metadata.Strings.Strings[method.Name];
        IsStatic = method.Flags.HasFlag(BindingFlags.Static);
        IsConstructor = name == ".ctor";
        IsRuntimeMethod = method.Flags.HasFlag(BindingFlags.RuntimeInternal);
        Name = name;
        Parent = parent;
        ReturnType = TypeTracker.Add(metadata.Types.Types[method.ReturnType], metadata, parent);
        var parameters = ImmutableArray.CreateBuilder<ParameterInfo>();
        var parameterCount = method.ParameterCount;
        var firstParameter = method.FirstParameter;
        for (var i = 0; i < parameterCount; i++)
        {
            var parameter = metadata.Parameters.Parameters[firstParameter + i];
            parameters.Add(new ParameterInfo(parameter, metadata, this, parent));
        }
        
        var locals = ImmutableArray.CreateBuilder<LocalInfo>();
        var localCount = method.LocalCount;
        var firstLocal = method.FirstLocal;
        for (var i = 0; i < localCount; i++)
        {
            var local = metadata.Locals.Locals[firstLocal + i];
            locals.Add(new LocalInfo(local, metadata, parent));
        }
        
        Parameters = parameters.ToImmutable();
        Locals = locals.ToImmutable();
        InstructionCount = method.InstructionCount;
        FirstInstruction = method.FirstInstruction;
    }
    
    public override string ToString()
    {
        var parameterString = string.Join(", ", Parameters.Select(p => p.ToString()));
        return $"{Parent.ToString(false)}.{Name}({parameterString})";
    }
}