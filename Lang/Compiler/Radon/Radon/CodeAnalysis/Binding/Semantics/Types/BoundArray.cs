using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Types;

internal sealed class BoundArray : BoundType
{
    public override BoundNodeKind Kind => BoundNodeKind.Array;
    public ArrayTypeSymbol Symbol { get; }
    public ImmutableArray<BoundMember> Members { get; }

    public BoundArray(SyntaxNode syntax, ArrayTypeSymbol symbol, ImmutableArray<BoundMember> members)
        : base(syntax, symbol)
    {
        Symbol = symbol;
        Members = members;
    }
}