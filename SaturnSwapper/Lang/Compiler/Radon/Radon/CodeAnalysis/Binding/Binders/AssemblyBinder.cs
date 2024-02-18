using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Formats.Asn1;
using System.IO;
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
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Binding.Binders;

internal sealed class AssemblyBinder : Binder
{
    private readonly List<TemplateBinder> _templateBinders;
    private readonly Dictionary<string, Scope> _assemblyScopes;
    public static AssemblyBinder Current { get; private set; } = null!;
    public AssemblySymbol Assembly { get; }

    internal AssemblyBinder() 
        : base((Scope?)null, TextLocation.Empty)
    {
        _templateBinders = new List<TemplateBinder>();
        _assemblyScopes = new Dictionary<string, Scope>();
        Assembly = new AssemblySymbol(string.Empty);
        Current = this;
    }

    public override BoundNode Bind(SyntaxNode node, params object[] args)
    {
        var context = SemanticContext.CreateEmpty(this, Diagnostics);
        // Args will be a list of syntax trees
        if (args is not [ImmutableArray<SyntaxTree> syntaxTrees])
        {
            return new BoundErrorNode(node, context);
        }

        // Get the syntax trees
        var codeUnits = new List<CodeCompilationUnitSyntax>();
        TopLevelStatementCompilationUnitSyntax? topLevelUnit = null;
        // Separate top-level-statement files from normal files
        foreach (var syntaxTree in syntaxTrees)
        {
            if (syntaxTree.Root is CodeCompilationUnitSyntax codeUnit)
            {
                codeUnits.Add(codeUnit);
            }
            else if (syntaxTree.Root is TopLevelStatementCompilationUnitSyntax program)
            {
                if (topLevelUnit is not null)
                {
                    // Only one top-level-statement file is allowed
                    Diagnostics.ReportMultipleProgramUnits(context.Location);
                    return new BoundErrorNode(node, context);
                }

                topLevelUnit = program;
            }
        }

        var primitiveBinders = new List<TypeSymbolBinder>();
        RegisterPrimitives(context, primitiveBinders);

        // Register all types
        var typeBinders = new List<NamedTypeBinder>();
        RegisterTypes(codeUnits, typeBinders);

        var boundTypes = new List<BoundType>();
        // If there is a top-level-statement file, it will be the last file to be bound
        if (topLevelUnit is not null)
        {
            var programBinder = new ProgramBinder(this, topLevelUnit);
            var program = (BoundType)programBinder.Bind(topLevelUnit);
            boundTypes.Add(program);
            Diagnostics.AddRange(programBinder.Diagnostics);
        }

        // Process all templates
        ProcessTemplates(boundTypes);

        // Bind all types
        BindTypes(primitiveBinders, boundTypes, typeBinders);

        // Check for incomplete arrays
        CheckForIncompleteArrays(boundTypes);
        
        // Check for multiple entry points
        CheckForMultipleEntries(boundTypes);

        // Create the assembly
        var diagnostics = Diagnostics.ToImmutableArray();
        var assembly = new BoundAssembly(SyntaxNode.Empty, Assembly, boundTypes.ToImmutableArray(), diagnostics, _assemblyScopes.ToImmutableDictionary());
        var lowerer = new Lowerer(assembly);
        var loweredAssembly = lowerer.Lower();
        return loweredAssembly;
    }

    public TypeSymbol BuildTemplate(TemplateSymbol template, ImmutableArray<TypeSymbol> typeArguments)
    {
        var node = SyntaxNode.Empty;
        if (template.TypeBinder is NamedTypeBinder ntb)
        {
            node = ntb.Syntax;
        }
        
        // Build the name of the constructed type
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
        
        // Make sure the template instance has not already been created
        var semanticContext = new SemanticContext(this, node, Diagnostics);
        if (TryResolve<StructSymbol>(semanticContext, name, out var type, false))
        {
            return type!;
        }
        
        // Create a new template binder, and register it
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

    private void RegisterPrimitives(SemanticContext context, List<TypeSymbolBinder> primitiveBinders)
    {
        // Register all primitive types
        var primitiveTypes = TypeSymbol.GetPrimitiveTypes();
        Register(context, new ArrayTypeSymbol(TypeSymbol.Char));
        foreach (var type in primitiveTypes)
        {
            Register(context, type);
        }
        
        // Initialize a binder for each primitive type
        foreach (var type in primitiveTypes)
        {
            var primitiveBinder = new TypeSymbolBinder(this, type);
            primitiveBinders.Add(primitiveBinder);
        }
        
        // Bind the members of each primitive type
        foreach (var primitiveBinder in primitiveBinders)
        {
            primitiveBinder.BindMembers();
        }
    }

    private void RegisterTypes(List<CodeCompilationUnitSyntax> codeUnits, List<NamedTypeBinder> typeBinders)
    {
        foreach (var codeUnit in codeUnits)
        {
            var scope = Scope?.CreateChild(codeUnit.Location);
            foreach (var typeDecl in codeUnit.DeclaredTypes)
            {
                var typeBinder = new NamedTypeBinder(this, typeDecl);
                var type = typeBinder.CreateType();
                var typeRegContext = new SemanticContext(typeDecl.Location, typeBinder, typeDecl, Diagnostics);
                typeBinders.Add(typeBinder);
                Register(typeRegContext, type);
            }
            
            var fileName = Path.GetFileName(codeUnit.SyntaxTree.Text.FileName);
            _assemblyScopes.Add(fileName, scope!);
        }
        
        // Resolve all members of each type
        foreach (var typeBinder in typeBinders)
        {
            typeBinder.ResolveMembers();
        }

        // Bind all members of each type
        foreach (var typeBinder in typeBinders)
        {
            typeBinder.BindMembers();
        }
    }

    private void ProcessTemplates(List<BoundType> boundTypes)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        // We do a for loop because we modify the list while iterating over it
        for (var i = 0; i < _templateBinders.Count; i++)
        {
            // Check if the bound template is incomplete
            // This means that there are still unresolved type parameters
            var templateBinder = _templateBinders[i];
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

            // Bind the template
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
    }
    
    private void CheckForIncompleteArrays(List<BoundType> boundTypes)
    {
        // Get all array type symbols
        var symbols = Scope?.GetAllSymbols<ArrayTypeSymbol>();
        if (symbols is not null)
        {
            foreach (var symbol in symbols)
            {
                if (HasUnresolvedTypeParameters(symbol))
                {
                    continue;
                }
                
                var arrayBinder = new TypeSymbolBinder(this, symbol);
                arrayBinder.BindMembers();
                var boundType = (BoundType)arrayBinder.Bind(null);
                boundTypes.Add(boundType);
                Diagnostics.AddRange(arrayBinder.Diagnostics);
                continue;
                
                bool HasUnresolvedTypeParameters(TypeSymbol type)
                {
                    switch (type)
                    {
                        case TemplateSymbol or TypeParameterSymbol:
                            return true;
                        case ArrayTypeSymbol array:
                            return HasUnresolvedTypeParameters(array.ElementType);
                    }

                    if (type.TemplateBinder is { } binder)
                    {
                        return CheckForIncompleteTemplate(binder);
                    }

                    return false;
                }
            }
        }
    }
    
    private void BindTypes(List<TypeSymbolBinder> primitiveBinders, List<BoundType> boundTypes, List<NamedTypeBinder> typeBinders)
    {
        // Bind all primitive types
        foreach (var primitiveBinder in primitiveBinders)
        {
            var boundType = (BoundType)primitiveBinder.Bind(null);
            if (boundType is not BoundTemplate)
            {
                boundTypes.Add(boundType);
            }

            Diagnostics.AddRange(primitiveBinder.Diagnostics);
        }

        // Bind all types
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
    }

    private void CheckForMultipleEntries(IEnumerable<BoundType> boundTypes)
    {
        // The following code is used to check for multiple entry points
        // This is necessary because having multiple entry points would lead to undefined behavior
        var foundEntryType = false;
        var foundEntryMethod = false;
        foreach (var boundType in boundTypes)
        {
            // Check if the type is an entry type
            if (boundType is not BoundStruct boundStruct || 
                !boundType.TypeSymbol.HasModifier(SyntaxKind.EntryKeyword) ||
                boundType.Syntax is not StructDeclarationSyntax syntax)
            {
                continue;
            }

            // Check if we already found an entry type
            if (foundEntryType)
            {
                Diagnostics.ReportMultipleEntryTypes(syntax.Modifiers
                    .First(x => x.Kind == SyntaxKind.EntryKeyword).Location);
            }
            
            // We found an entry type
            foundEntryType = true;
            // Find the entry method
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
    }
}