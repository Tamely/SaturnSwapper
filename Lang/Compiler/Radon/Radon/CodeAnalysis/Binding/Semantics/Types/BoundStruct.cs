using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Types;

internal sealed class BoundStruct : BoundType
{
    public override BoundNodeKind Kind => BoundNodeKind.Struct;
    public StructSymbol Symbol { get; }
    public ImmutableArray<BoundMember> Members { get; }

    public BoundStruct(SyntaxNode syntax, StructSymbol symbol, ImmutableArray<BoundMember> members)
        : base(syntax, symbol)
    {
        Symbol = symbol;
        Members = members;
    }
}
