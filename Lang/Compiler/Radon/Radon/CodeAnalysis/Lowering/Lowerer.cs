using System;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Types;

namespace Radon.CodeAnalysis.Lowering;

internal sealed class Lowerer
{
    private readonly BoundAssembly _assembly;

    private readonly DiagnosticBag _diagnostics;

    public Lowerer(BoundAssembly assembly)
    {
        _assembly = assembly;
        _diagnostics = new DiagnosticBag();
        _diagnostics.AddRange(assembly.Diagnostics);
    }
    
    public BoundAssembly Lower()
    {
        var loweredTypes = LowerTypes();
        return new BoundAssembly(_assembly.Syntax, _assembly.Assembly, loweredTypes, _diagnostics.ToImmutableArray(), _assembly.Scope);
    }
    
    private ImmutableArray<BoundType> LowerTypes()
    {
        var builder = ImmutableArray.CreateBuilder<BoundType>();
        foreach (var type in _assembly.Types)
        {
            builder.Add(LowerType(type));
        }
        
        return builder.ToImmutable();
    }
    
    private BoundType LowerType(BoundType node)
    {
        return node switch
        {
            BoundStruct boundStruct => LowerStruct(boundStruct),
            BoundEnum boundEnum => boundEnum,
            BoundErrorType boundType => boundType,
            BoundArray boundArray => boundArray,
            _ => throw new Exception($"Unexpected type {node.Kind}")
        };
    }

    private BoundStruct LowerStruct(BoundStruct node)
    {
        var structLowerer = new StructLowerer(node);
        var loweredStruct = structLowerer.Lower();
        _diagnostics.AddRange(structLowerer.Diagnostics);
        return loweredStruct;
    }
}