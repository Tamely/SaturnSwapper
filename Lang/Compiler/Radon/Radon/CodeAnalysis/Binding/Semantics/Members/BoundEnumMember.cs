using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Members;

internal sealed class BoundEnumMember : BoundMember
{
    public override BoundNodeKind Kind => BoundNodeKind.EnumMember;
    public EnumMemberSymbol Symbol { get; }
    public BoundEnumMember(SyntaxNode syntax, EnumMemberSymbol symbol) 
        : base(syntax)
    {
        Symbol = symbol;
    }
}