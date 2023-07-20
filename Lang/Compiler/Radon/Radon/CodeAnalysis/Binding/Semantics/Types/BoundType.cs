using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Types;

internal abstract class BoundType : BoundNode
{
    public TypeSymbol TypeSymbol { get; }
    protected BoundType(SyntaxNode syntax, TypeSymbol typeSymbol) 
        : base(syntax)
    {
        TypeSymbol = typeSymbol;
    }
    
    public override string ToString() => TypeSymbol.ToString();
}