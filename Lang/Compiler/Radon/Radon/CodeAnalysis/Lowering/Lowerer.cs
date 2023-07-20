using System;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Types;

namespace Radon.CodeAnalysis.Lowering;

internal sealed class Lowerer
{
    private readonly BoundAssembly _assembly;
    public Lowerer(BoundAssembly assembly)
    {
        _assembly = assembly;
    }
    
    public BoundAssembly Lower()
    {
        var loweredTypes = LowerTypes();
        return new BoundAssembly(_assembly.Syntax, _assembly.Assembly, loweredTypes, _assembly.Diagnostics, _assembly.Scope);
    }
    
    private ImmutableArray<BoundType> LowerTypes()
    {
        var builder = ImmutableArray.CreateBuilder<BoundType>();
#if DEBUG
        // ReSharper disable once NotAccessedVariable
        var counter = 0;
#endif
        foreach (var type in _assembly.Types)
        {
            builder.Add(LowerType(type));
            
#if DEBUG
            counter++;
#endif
        }
        
        return builder.ToImmutable();
    }
    
    private BoundType LowerType(BoundType node)
    {
        return node switch
        {
            BoundStruct boundStruct => LowerStruct(boundStruct),
            BoundEnum boundEnum => LowerEnum(boundEnum),
            BoundErrorType boundType => boundType,
            _ => throw new Exception($"Unexpected type {node.Kind}")
        };
    }

    private BoundStruct LowerStruct(BoundStruct node)
    {
        var structLowerer = new StructLowerer(node);
        return structLowerer.Lower();
    }
    
    private BoundEnum LowerEnum(BoundEnum node)
    {
        return node;
    }
}
