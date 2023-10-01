using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundNameExpression : BoundExpression, ISymbolExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.NameExpression;
    public override TypeSymbol Type { get; }
    public Symbol Symbol { get; }

    internal BoundNameExpression(SyntaxNode syntax, Symbol symbol) 
        : base(syntax)
    {
        Symbol = symbol;
        Type = symbol.Type;
    }
}