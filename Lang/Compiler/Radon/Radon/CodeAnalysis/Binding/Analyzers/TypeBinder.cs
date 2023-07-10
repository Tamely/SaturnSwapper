using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal abstract class TypeBinder : Binder
{
    protected TypeBinder(Binder binder) 
        : base(binder)
    {
    }

    public abstract MethodSymbol BuildTemplateMethod(TemplateMethodSymbol templateMethod, ImmutableArray<TypeSymbol> typeArguments, SyntaxNode callSite);
}