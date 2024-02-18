using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Types;

internal sealed class BoundEnum : BoundType
{
    public override BoundNodeKind Kind => BoundNodeKind.Enum;
    public EnumSymbol Symbol { get; }
    public ImmutableArray<BoundEnumMember> Members { get; }
    
    public BoundEnum(SyntaxNode syntax, EnumSymbol symbol, ImmutableArray<BoundEnumMember> members)
        : base(syntax, symbol)
    {
        Symbol = symbol;
        Members = members;
    }
}