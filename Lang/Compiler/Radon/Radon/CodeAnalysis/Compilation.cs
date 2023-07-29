using System;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding;
using Radon.CodeAnalysis.Binding.Analyzers;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Emit;
using Radon.CodeAnalysis.Emit.Builders;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis;

public sealed class Compilation
{
    private readonly BoundAssembly _boundTree;
    private readonly bool _proceedWithCompilation = true;
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public Scope AssemblyScope => _boundTree.Scope ?? new Scope(null);
    public Compilation(SyntaxTree syntaxTree)
    {
        var syntaxTrees = ImmutableArray.CreateBuilder<SyntaxTree>();
        syntaxTrees.Add(syntaxTree);
        syntaxTrees.AddRange(syntaxTree.Included);
        SyntaxTrees = syntaxTrees.ToImmutable();
        var syntaxDiagnostics = SyntaxTrees.SelectMany(s => s.Diagnostics);
        var diagnostics = syntaxDiagnostics as Diagnostic[] ?? syntaxDiagnostics.ToArray();
        if (diagnostics.Any())
        {
            _proceedWithCompilation = false;
        }
        
        _boundTree = GetTree();
        Diagnostics = _proceedWithCompilation ? _boundTree.Diagnostics.AddRange(diagnostics) : diagnostics.ToImmutableArray();
    }

    private BoundAssembly GetTree()
    {
        if (!_proceedWithCompilation)
        {
            return new BoundAssembly(SyntaxNode.Empty, null, ImmutableArray<BoundType>.Empty, Diagnostics, null);
        }
        
        try
        {
            var assemblyBinder = new AssemblyBinder();
            var assembly = assemblyBinder.Bind(SyntaxNode.Empty, SyntaxTrees);
            return (BoundAssembly)assembly;
        }
        catch (Exception e)
        {
            var diagnosticBag = new DiagnosticBag();
            diagnosticBag.ReportInternalCompilerError(e);
            return new BoundAssembly(SyntaxNode.Empty, null, ImmutableArray<BoundType>.Empty, diagnosticBag.ToImmutableArray(), null);
        }
    }
    
    public byte[]? Compile(out ImmutableArray<Diagnostic> diagnostics)
    {
        try
        {
            var emissionBuilder = new AssemblyBuilder(_boundTree);
            var emission = emissionBuilder.Build();
            var emitter = new Emitter(emission);
            diagnostics = ImmutableArray<Diagnostic>.Empty;
            return emitter.Emit();
        }
        catch (Exception e)
        {
            var diagnosticBag = new DiagnosticBag();
            diagnosticBag.ReportInternalCompilerError(e);
            diagnostics = diagnosticBag.ToImmutableArray();
            return null;
        }
    }
}