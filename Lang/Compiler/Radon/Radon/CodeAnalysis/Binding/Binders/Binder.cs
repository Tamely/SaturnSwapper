using System;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;

namespace Radon.CodeAnalysis.Binding.Binders;

internal abstract class Binder
{
    private static readonly object Lock = new();
    protected Scope? Scope { get; set; }
    internal DiagnosticBag Diagnostics { get; }

    private protected Binder(Scope? scope)
    {
        if (scope is null)
        {
            Scope = new Scope(null);
        }
        else
        {
            Scope = scope.CreateChild();
        }

        Diagnostics = new DiagnosticBag();
    }

    private protected Binder(Binder binder)
    {
        if (binder.Scope is null)
        {
            Scope = new Scope(null);
        }
        else
        {
            Scope = binder.Scope.CreateChild();
        }

        Diagnostics = new DiagnosticBag();
    }
    
    public abstract BoundNode Bind(SyntaxNode node, params object[] args);

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

    protected void Reregister<TSymbol>(SemanticContext context, TSymbol symbol, TSymbol newSymbol)
        where TSymbol : Symbol
    {
        if (Scope is null)
        {
            context.Diagnostics.ReportNullScope(context.Location);
            return;
        }
        
        if (!Scope.TryLookupSymbol<TSymbol>(symbol.Name, out var existingSymbol))
        {
            Diagnostics.ReportUnresolvedSymbol(context.Location, symbol.Name);
        }

        if (existingSymbol is not null)
        {
            Scope.RemoveSymbol(existingSymbol);
            Register(context, newSymbol);
        }
        else
        {
            Diagnostics.ReportUnresolvedSymbol(context.Location, symbol.Name);
        }
    }

    protected bool TryResolve<TSymbol>(SemanticContext context, string name, out TSymbol? symbol, bool reportUnresolvedSymbol = true)
        where TSymbol : Symbol
    {
        try
        {
            if (Scope is null)
            {
                context.Diagnostics.ReportNullScope(context.Location);
                symbol = null;
                return false;
            }
        
            if (Scope.TryLookupSymbol(name, out symbol!))
            {
                return TryResolveSymbol(context, ref symbol);
            }

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
        if (typeof(TSymbol).IsAssignableTo(typeof(TypeSymbol)))
        {
            if (this is NamedTypeBinder ntb &&
                ntb.CurrentMember == SymbolKind.Constructor)
            {
                return true; // The ref symbol should be the parent type, which is the needed type, therefore, we do not need to do anything.
            }
            
            if (symbol is TemplateSymbol or PrimitiveTemplateSymbol)
            {
                ImmutableArray<TypeSymbol> typeArguments;
                if (symbol is TemplateSymbol template && context.Tag is ImmutableArray<TypeSymbol> typeArgs)
                {
                    typeArguments = typeArgs;
                }
                else if (symbol is PrimitiveTemplateSymbol primitiveTemplate)
                {
                    typeArguments = primitiveTemplate.TypeArguments;
                    template = primitiveTemplate.Template;
                }
                else
                {
                    throw new InvalidOperationException("The symbol is not a template symbol.");
                }

                var resolvedTypeArgs = new TypeSymbol[typeArguments.Length];
                for (var i = 0; i < typeArguments.Length; i++)
                {
                    var typeArg = typeArguments[i];
                    if (typeArg is TypeParameterSymbol) // We can't build the template if we have unresolved type parameters.
                    {
                        if (TryResolve<TypeSymbol>(context, typeArg.Name, out var resolvedArg,
                                false)) // We check if the type parameter has been resolved.
                        {
                            resolvedTypeArgs[i] = resolvedArg!;
                            continue;
                        }

                        // If the type can't be built on the spot, it will likely be built when the template method/class is called/constructed.
                        return true; // We return true because the type does exist, just we can't build it.
                    }

                    resolvedTypeArgs[i] = typeArg;
                }

                var templateSymbol = AssemblyBinder.Current.BuildTemplate(template, resolvedTypeArgs.ToImmutableArray());
                symbol = (TSymbol)(object)templateSymbol;
                return true;
            }

            if (symbol is TypeParameterSymbol typeParameter)
            {
                var boundTypeParameters = Scope!.GetSymbols<BoundTypeParameterSymbol>();
                foreach (var tp in boundTypeParameters)
                {
                    if (tp.TypeParameter == typeParameter)
                    {
                        symbol = (TSymbol)(object)tp.BoundType;
                        return true;
                    }
                }
            }

            if (symbol is BoundTypeParameterSymbol boundTypeParameter)
            {
                symbol = (TSymbol)(object)boundTypeParameter.BoundType;
                return true;
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