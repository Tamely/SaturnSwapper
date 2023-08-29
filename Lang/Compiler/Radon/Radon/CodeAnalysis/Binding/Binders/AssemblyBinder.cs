using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Lowering;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Members;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;

namespace Radon.CodeAnalysis.Binding.Binders;

internal sealed class AssemblyBinder : Binder
{
    private readonly List<TemplateBinder> _templateBinders;
    public static AssemblyBinder Current { get; private set; } = null!;
    public AssemblySymbol Assembly { get; }

    internal AssemblyBinder() 
        : base((Scope?)null)
    {
        _templateBinders = new List<TemplateBinder>();
        Assembly = new AssemblySymbol(string.Empty);
        Current = this;
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
        foreach (var syntaxTree in syntaxTrees)
        {
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

        var primitiveBinders = new List<TypeSymbolBinder>();
        foreach (var type in TypeSymbol.GetPrimitiveTypes())
        {
            var typeBinder = new TypeSymbolBinder(this, type);
            typeBinder.BindMembers();
            primitiveBinders.Add(typeBinder);
            Register(context, type);
        }

        var typeBinders = new List<NamedTypeBinder>();
        foreach (var codeUnit in codeUnits)
        {
            foreach (var typeDecl in codeUnit.DeclaredTypes)
            {
                var typeBinder = new NamedTypeBinder(this, typeDecl);
                var type = typeBinder.CreateType();
                var typeRegContext = new SemanticContext(typeDecl.Location, typeBinder, typeDecl, Diagnostics);
                typeBinders.Add(typeBinder);
                Register(typeRegContext, type);
            }
        }

        // There's a reason for multiple loops here
        // We first need to resolve all members of all types.
        // We do this so that all members will be resolved when we start binding the statements and expressions.
        //
        // Next, we bind all members of all types.
        // This is where we actually bind the statements and expressions.
        foreach (var typeBinder in typeBinders)
        {
            typeBinder.ResolveMembers();
        }

        foreach (var typeBinder in typeBinders)
        {
            typeBinder.BindMembers();
        }

        if (programUnit is not null)
        {
            var programBinder = new ProgramBinder(this, programUnit);
            var program = (BoundType)programBinder.Bind(null);
            boundTypes.Add(program);
            Diagnostics.AddRange(programBinder.Diagnostics);
        }
        
        foreach (var templateBinder in _templateBinders)
        {
            if (CheckForIncompleteTemplate(templateBinder))
            {
                continue;
            }
            
            var binder = templateBinder.GetBinder();
            switch (binder)
            {
                case NamedTypeBinder namedTypeBinder:
                    namedTypeBinder.BindMembers();
                    break;
                case TypeSymbolBinder primitiveTypeBinder:
                    primitiveTypeBinder.BindMembers();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var boundType = (BoundType)templateBinder.Bind(null);
            if (templateBinder.GetTypeArguments().Any(x => x is TypeParameterSymbol))
            {
                continue; // We do this because it's not a completed template
            }

            if (boundType is not BoundTemplate)
            {
                boundTypes.Add(boundType);
            }
            
            Diagnostics.AddRange(binder.Diagnostics);
        }
        
        foreach (var primitiveBinder in primitiveBinders)
        {
            var boundType = (BoundType)primitiveBinder.Bind(null);
            if (boundType is not BoundTemplate)
            {
                boundTypes.Add(boundType);
            }
            
            Diagnostics.AddRange(primitiveBinder.Diagnostics);
        }
        
        foreach (var typeBinder in typeBinders)
        {
            var boundType = (BoundType)typeBinder.Bind(null);
            if (boundType is BoundTemplate)
            {
                continue;
            }
            
            if (boundType is not BoundTemplate)
            {
                boundTypes.Add(boundType);
            }
            
            Diagnostics.AddRange(typeBinder.Diagnostics);
        }
        
        // Get all array type symbols
        var symbols = Scope?.GetAllSymbols<ArrayTypeSymbol>();
        if (symbols is not null)
        {
            foreach (var symbol in symbols)
            {
                bool HasUnresolvedTypeParameters(TypeSymbol type)
                {
                    if (type is TemplateSymbol or TypeParameterSymbol)
                    {
                        return true;
                    }
                    
                    if (type is ArrayTypeSymbol array)
                    {
                        return HasUnresolvedTypeParameters(array.ElementType);
                    }

                    if (type.TemplateBinder is { } binder)
                    {
                        return CheckForIncompleteTemplate(binder);
                    }

                    return false;
                }

                if (HasUnresolvedTypeParameters(symbol))
                {
                    continue;
                }
                
                var arrayBinder = new TypeSymbolBinder(this, symbol);
                arrayBinder.BindMembers();
                var boundType = (BoundType)arrayBinder.Bind(null);
                boundTypes.Add(boundType);
                Diagnostics.AddRange(arrayBinder.Diagnostics);
            }
        }

        // The following code is used to check for multiple entry points
        // This is necessary because having multiple entry points would lead to undefined behavior
        var foundEntryType = false;
        var foundEntryMethod = false;
        foreach (var boundType in boundTypes)
        {
            if (boundType is not BoundStruct boundStruct || 
                !boundType.TypeSymbol.HasModifier(SyntaxKind.EntryKeyword) ||
                boundType.Syntax is not StructDeclarationSyntax syntax)
            {
                continue;
            }

            if (foundEntryType)
            {
                Diagnostics.ReportMultipleEntryTypes(syntax.Modifiers
                    .First(x => x.Kind == SyntaxKind.EntryKeyword).Location);
            }
            
            foundEntryType = true;
            foreach (var member in boundStruct.Members)
            {
                if (member is not BoundMethod method ||
                    !method.Symbol.HasModifier(SyntaxKind.EntryKeyword) ||
                    member.Syntax is not MethodDeclarationSyntax methodSyntax)
                {
                    continue;
                }
                
                if (foundEntryMethod)
                {
                    Diagnostics.ReportMultipleEntryMethods(methodSyntax.Modifiers
                        .First(x => x.Kind == SyntaxKind.EntryKeyword).Location);
                }
                
                foundEntryMethod = true;
            }
        }

        var diagnostics = Diagnostics.ToImmutableArray();
        var assembly = new BoundAssembly(SyntaxNode.Empty, Assembly, boundTypes.ToImmutableArray(), diagnostics, Scope);
        var lowerer = new Lowerer(assembly);
        var loweredAssembly = lowerer.Lower();
        return loweredAssembly;
    }

    public TypeSymbol BuildTemplate(TemplateSymbol template, ImmutableArray<TypeSymbol> typeArguments)
    {
        var node = SyntaxNode.Empty;
        if (template.TypeBinder is NamedTypeBinder utb)
        {
            node = utb.Syntax;
        }
        
        var sb = new StringBuilder();
        sb.Append(template.Name);
        sb.Append('`');
        foreach (var typeArg in typeArguments)
        {
            sb.Append(typeArg.Name);
            sb.Append(',');
        }
        
        sb.Remove(sb.Length - 1, 1);
        var name = sb.ToString();
        var semanticContext = new SemanticContext(this, node, Diagnostics);
        if (TryResolve<StructSymbol>(semanticContext, name, out var type, false))
        {
            return type!;
        }
        
        var templateBinder = new TemplateBinder(this, node, template, typeArguments, name);
        var constructedType = templateBinder.GetTypeSymbol();
        Register(semanticContext, constructedType);
        
        templateBinder.SetBinder();
        _templateBinders.Add(templateBinder);
        return constructedType;
    }

    private static bool CheckForIncompleteTemplate(TemplateBinder? templateBinder)
    {
        if (templateBinder is null)
        {
            return false;
        }
        
        var typeArgs = templateBinder.GetTypeArguments();
        foreach (var typeArg in typeArgs)
        {
            if (typeArg is TypeParameterSymbol)
            {
                return true;
            }
            
            if (CheckForIncompleteTemplate(typeArg.TemplateBinder))
            {
                return true;
            }
        }

        return false;
    }
}