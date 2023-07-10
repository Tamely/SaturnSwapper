using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Types;

internal sealed class BoundTemplate : BoundType
{
    public override BoundNodeKind Kind => BoundNodeKind.Template;
    public TemplateSymbol Symbol { get; }
    public ImmutableArray<BoundMember> Members { get; }

    public BoundTemplate(SyntaxNode syntax, TemplateSymbol symbol, ImmutableArray<BoundMember> members)
        : base(syntax, symbol)
    {
        Symbol = symbol;
        Members = members;
    }
}
