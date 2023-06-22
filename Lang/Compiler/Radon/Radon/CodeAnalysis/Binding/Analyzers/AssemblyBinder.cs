using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Lowering;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal sealed class AssemblyBinder : Binder
{
    public AssemblySymbol Assembly { get; }
    
    internal AssemblyBinder() 
        : base((Scope?)null)
    {
        Assembly = new AssemblySymbol(string.Empty);
    }

    public override BoundNode Bind(SyntaxNode node, params object[] args)
    {
        var context = SemanticContext.CreateEmpty(this, Diagnostics);
        // Args will be a list of syntax trees
        if (args.Length != 1)
        {
            return new BoundErrorNode(node, context);
        }

        var syntaxTrees = (ImmutableArray<SyntaxTree>)args[0];
        var codeUnits = new List<CodeCompilationUnitSyntax>();
        var boundTypes = new List<BoundType>();
        PluginCompilationUnitSyntax? programUnit = null;
        for (int i = 0; i < syntaxTrees.Length; i++)
        {
            var syntaxTree = syntaxTrees[i];
            if (syntaxTree.Root is CodeCompilationUnitSyntax codeUnit)
            {
                codeUnits.Add(codeUnit);
            }
            else if (syntaxTree.Root is PluginCompilationUnitSyntax program)
            {
                if (programUnit is not null)
                {
                    Diagnostics.ReportMultipleProgramUnits(context.Location);
                    return new BoundErrorNode(node, context);
                }

                programUnit = program;
            }
        }

        foreach (var type in TypeSymbol.GetPrimitiveTypes())
        {
            var typeBinder = new PrimitiveTypeBinder(this, type);
            var boundType = (BoundType)typeBinder.Bind(node);
            boundTypes.Add(boundType);
            Register(context, type);
        }

        var typeBinders = new Dictionary<TypeSymbol, TypeBinder>();
        foreach (var codeUnit in codeUnits)
        {
            foreach (var typeDecl in codeUnit.DeclaredTypes)
            {
                var typeBinder = new TypeBinder(this, typeDecl);
                var type = typeBinder.CreateType();
                var typeRegContext = new SemanticContext(typeDecl.Location, typeBinder, typeDecl, Diagnostics);
                typeBinders.Add(type, typeBinder);
                Register(typeRegContext, type);
            }
        }

        foreach (var (typeSymbol, typeBinder) in typeBinders)
        {
            var typeWithMembers = typeBinder.ResolveMembers();
            var typeRegContext = new SemanticContext(typeBinder, typeBinder.Syntax, Diagnostics);
            Reregister(typeRegContext, typeSymbol, typeWithMembers);
        }
        
        foreach (var (_, typeBinder) in typeBinders)
        {
            var boundType = (BoundType)typeBinder.Bind(null);
            boundTypes.Add(boundType);
            Diagnostics.AddRange(typeBinder.Diagnostics);
        }

        if (programUnit is not null)
        {
            var programBinder = new ProgramBinder(this, programUnit);
            var program = (BoundType)programBinder.Bind(null);
            boundTypes.Add(program);
            Diagnostics.AddRange(programBinder.Diagnostics);
        }

        var diagnostics = Diagnostics.ToImmutableArray();
        var assembly = new BoundAssembly(SyntaxNode.Empty, Assembly, boundTypes.ToImmutableArray(), diagnostics);
        var lowerer = new Lowerer(assembly);
        var loweredAssembly = lowerer.Lower();
        return loweredAssembly;
    }
}