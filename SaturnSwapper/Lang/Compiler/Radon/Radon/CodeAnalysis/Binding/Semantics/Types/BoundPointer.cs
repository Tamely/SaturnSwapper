using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Types;

internal sealed class BoundPointer : BoundType
{
    public override BoundNodeKind Kind => BoundNodeKind.Pointer;
    public PointerTypeSymbol Symbol { get; }

    public BoundPointer(SyntaxNode syntax, PointerTypeSymbol symbol)
        : base(syntax, symbol)
    {
        Symbol = symbol;
    }
}