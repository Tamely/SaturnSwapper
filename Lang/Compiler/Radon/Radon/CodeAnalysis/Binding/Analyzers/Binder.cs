using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal abstract class Binder
{
    protected Scope? Scope { get; }
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

    public bool Register(SemanticContext context, Symbol symbol)
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

    public void Reregister<TSymbol>(SemanticContext context, TSymbol symbol, TSymbol newSymbol)
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

    public bool TryResolve<TSymbol>(SemanticContext context, string name, out TSymbol? symbol)
        where TSymbol : Symbol
    {
        if (Scope is null)
        {
            context.Diagnostics.ReportNullScope(context.Location);
            symbol = null;
            return false;
        }
        
        if (Scope.TryLookupSymbol(name, out symbol!))
        {
            Scope.AddSymbolReference(symbol, context.Location);
            return true;
        }

        if (typeof(TSymbol) == typeof(TypeSymbol))
        {
            context.Diagnostics.ReportUndefinedType(context.Location, name);
        }
        else
        {
            context.Diagnostics.ReportUnresolvedSymbol(context.Location, name);
        }
        
        return false;
    }
    
    public bool TryResolveMethod<TMethodSymbol>(SemanticContext context, TypeSymbol parentType, string name, 
        ImmutableArray<BoundExpression> parameterTypes, out TMethodSymbol methodSymbol)
        where TMethodSymbol : AbstractMethodSymbol
    {
        if (Scope is null)
        {
            context.Diagnostics.ReportNullScope(context.Location);
            methodSymbol = default!;
            return false;
        }
        
        // The method was not found in the current scope, so we need to check the parent type.
        if (parentType.TryLookupMethod(name, parameterTypes, out var methodNotFound, out var cannotConvertType,
                out var ambiguousCall, out var from, out var to, out var ambiguousCalls,
                out methodSymbol))
        {
            Scope.AddSymbolReference(methodSymbol, context.Location);
            return true;
        }
        
        if (methodNotFound)
        {
            context.Diagnostics.ReportUnresolvedMethod(context.Location, name, parameterTypes.Select(x => x.Type).ToImmutableArray());
        }

        if (cannotConvertType)
        {
            context.Diagnostics.ReportCannotConvert(context.Location, from!, to!);
        }

        if (ambiguousCall)
        {
            context.Diagnostics.ReportAmbiguousMethodCall(context.Location, ambiguousCalls);
        }
        
        return false;
    }
}