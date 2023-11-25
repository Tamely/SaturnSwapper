using System;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Binding.Binders;

internal abstract class Binder
{
    protected Scope? Scope { get; set; }
    public DiagnosticBag Diagnostics { get; }
    public TextLocation Location { get; }

    private protected Binder(Scope? scope, TextLocation location)
    {
        Scope = scope is null ? new Scope(null, location) : scope.CreateChild(location);
        Diagnostics = new DiagnosticBag();
    }

    private protected Binder(Binder binder, TextLocation location)
    {
        Scope = binder.Scope is null ? new Scope(null, location) : binder.Scope.CreateChild(location);
        Diagnostics = new DiagnosticBag();
    }
    
    public abstract BoundNode Bind(SyntaxNode node, params object[] args);

    public TextLocation GetDeclaration(Symbol symbol)
    {
        return Scope!.GetSymbolDeclaration(symbol);
    }

    /// <summary>
    /// Registers a symbol in the current scope.
    /// </summary>
    /// <param name="context">The semantic context</param>
    /// <param name="symbol">The symbol to register</param>
    /// <returns></returns>
    protected bool Register(SemanticContext context, Symbol symbol)
    {
        if (Scope is null)
        {
            context.Diagnostics.ReportNullScope(context.Location);
            return false;
        }
        
        if (!Scope.TryDeclareSymbol(symbol, context.Location))
        {
            context.Diagnostics.ReportSymbolAlreadyDeclared(context.Location, symbol.Name);
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Attempts to resolve a symbol in the current scope.
    /// </summary>
    /// <param name="context">The semantic context</param>
    /// <param name="name">The name of the symbol</param>
    /// <param name="symbol">The resolved symbol</param>
    /// <param name="reportUnresolvedSymbol">If true, the method will report an error</param>
    /// <typeparam name="TSymbol">Symbol Type</typeparam>
    /// <returns>False if the symbol does not exist in the current or parent scopes</returns>
    protected bool TryResolve<TSymbol>(SemanticContext context, string name, out TSymbol? symbol, bool reportUnresolvedSymbol = true)
        where TSymbol : Symbol
    {
        try
        {
            // We check if the symbol exists in the current scope.
            // This should never be null, but we check just in case.
            if (Scope is null)
            {
                context.Diagnostics.ReportNullScope(context.Location);
                symbol = null;
                return false;
            }
        
            // We check if the symbol exists in the current scope.
            if (Scope.TryLookupSymbol(name, out symbol!))
            {
                return TryResolveSymbol(context, ref symbol);
            }

            // We did not find the symbol. Report an error if specified.
            if (reportUnresolvedSymbol)
            {
                if (typeof(TSymbol).IsAssignableTo(typeof(TypeSymbol)))
                {
                    context.Diagnostics.ReportUndefinedType(context.Location, name);
                }
                else
                {
                    context.Diagnostics.ReportUnresolvedSymbol(context.Location, name);
                }
            }
        
            return false;
        }
        catch (Exception)
        {
            symbol = null;
            return false;
        }
    }

    public bool TryResolveSymbol<TSymbol>(SemanticContext context, ref TSymbol symbol) 
        where TSymbol : Symbol
    {
        // Check if the symbol is a TypeSymbol
        if (typeof(TSymbol).IsAssignableTo(typeof(TypeSymbol)))
        {
            // I don't remember the purpose of this, but I know it prevents an error.
            if (this is NamedTypeBinder { CurrentMember: SymbolKind.Constructor })
            {
                return true; // The ref symbol should be the parent type, which is the needed type, therefore, we do not need to do anything.
            }
            
            // Check the type of TypeSymbol
            switch (symbol)
            {
                case TemplateSymbol:
                {
                    ImmutableArray<TypeSymbol> typeArguments;
                    // Get the type arguments from the context.
                    if (symbol is TemplateSymbol template && context.Tag is ImmutableArray<TypeSymbol> typeArgs)
                    {
                        typeArguments = typeArgs;
                    }
                    else
                    {
                        throw new InvalidOperationException("The symbol is not a template symbol.");
                    }
                    
                    // Resolve the type arguments.
                    var resolvedTypeArgs = new TypeSymbol[typeArguments.Length];
                    for (var i = 0; i < typeArguments.Length; i++)
                    {
                        var typeArg = typeArguments[i];
                        // We can't build the template if we have unresolved type parameters.
                        if (typeArg is TypeParameterSymbol)
                        {
                            // We check if the type parameter has been resolved.
                            if (TryResolve<TypeSymbol>(context, typeArg.Name, out var resolvedArg,
                                    false))
                            {
                                resolvedTypeArgs[i] = resolvedArg!;
                                continue;
                            }

                            // If the type can't be built on the spot, it will likely be built when the template method/class is called/constructed.
                            return true; // We return true because the type does exist, just we can't build it.
                        }

                        resolvedTypeArgs[i] = typeArg;
                    }

                    // Build the template.
                    var templateSymbol = AssemblyBinder.Current.BuildTemplate(template, resolvedTypeArgs.ToImmutableArray());
                    symbol = (TSymbol)(object)templateSymbol;
                    return true;
                }
                case TypeParameterSymbol typeParameter:
                {
                    // We check if the type parameter has been resolved.
                    var boundTypeParameters = Scope!.GetSymbols<BoundTypeParameterSymbol>();
                    foreach (var tp in boundTypeParameters)
                    {
                        if (tp.TypeParameter == typeParameter)
                        {
                            symbol = (TSymbol)(object)tp.BoundType;
                            return true;
                        }
                    }

                    break;
                }
                case BoundTypeParameterSymbol boundTypeParameter:
                {
                    // Return the type that is bound to the type parameter.
                    symbol = (TSymbol)(object)boundTypeParameter.BoundType;
                    return true;
                }
            }
        }

        Scope?.AddSymbolReference(symbol, context.Location);
        return true;
    }

    protected bool TryResolveMethod<TMethodSymbol>(SemanticContext context, TypeSymbol parentType, string name, 
        ImmutableArray<TypeSymbol> typeArguments, ImmutableArray<BoundExpression> parameterTypes, SyntaxNode callSite, out TMethodSymbol? methodSymbol)
        where TMethodSymbol : AbstractMethodSymbol
    {
        if (Scope is null)
        {
            context.Diagnostics.ReportNullScope(context.Location);
            methodSymbol = default!;
            return false;
        }
        
        // The method was not found in the current scope, so we need to check the parent type.
        if (parentType.TryLookupMethod(this, name, typeArguments, parameterTypes, callSite, out var methodNotFound, 
                out var ambiguousCalls, out methodSymbol))
        {
            if (methodSymbol is null)
            {
                return false;
            }
            
            Scope.AddSymbolReference(methodSymbol, context.Location);
            return true;
        }
        
        if (methodNotFound)
        {
            context.Diagnostics.ReportUnresolvedMethod(context.Location, name, parameterTypes.Select(x => x.Type).ToImmutableArray());
        }

        if (ambiguousCalls.Length > 1)
        {
            context.Diagnostics.ReportAmbiguousMethodCall(context.Location, ambiguousCalls);
        }

        return false;
    }

    protected TypeSymbol BindTypeSyntax(TypeSyntax syntax)
    {
        if (syntax is ArrayTypeSyntax arraySyntax)
        {
            var type = BindTypeSyntax(arraySyntax.TypeSyntax);
            if (type.IsStatic)
            {
                Diagnostics.ReportElementTypeCannotBeStatic(arraySyntax.Location);
            }
            
            var name = $"{type.Name}[]";
            var context = new SemanticContext(syntax.Location, this, syntax, Diagnostics);
            if (TryResolve<ArrayTypeSymbol>(context, name, out var array, false))
            {
                return array!;
            }
            
            array = new ArrayTypeSymbol(type);
            AssemblyBinder.Current.Register(context, array);
            return array;
        }

        if (syntax is PointerTypeSyntax pointerType)
        {
            var type = BindTypeSyntax(pointerType.Type);
            if (type.IsStatic)
            {
                Diagnostics.ReportPointerTypeCannotBeStatic(pointerType.Location);
            }
            
            return BindPointerType(pointerType, type);
        }
        
        var typeArguments = ImmutableArray.CreateBuilder<TypeSymbol>();
        if (syntax.TypeArgumentList is not null)
        {
            foreach (var typeArgument in syntax.TypeArgumentList.Arguments)
            {
                var type = BindTypeSyntax(typeArgument);
                typeArguments.Add(type);
            }
        }

        var typeContext = new SemanticContext(syntax.Location, this, syntax, Diagnostics)
        {
            Tag = typeArguments.ToImmutable()
        };

        if (!TryResolve<TypeSymbol>(typeContext, syntax.Identifier.Text, out var typeSymbol))
        {
            return TypeSymbol.Error;
        }
        
        return typeSymbol!;
    }

    protected PointerTypeSymbol BindPointerType(SyntaxNode syntax, TypeSymbol type)
    {
        var name = $"{type.Name}*";
        var context = new SemanticContext(syntax.Location, this, syntax, Diagnostics);
        if (TryResolve<PointerTypeSymbol>(context, name, out var pointer, false))
        {
            return pointer!;
        }
            
        pointer = new PointerTypeSymbol(type);
        AssemblyBinder.Current.Register(context, pointer);
        return pointer;
    }
}