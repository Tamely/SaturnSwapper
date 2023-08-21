using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundErrorExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
    public override TypeSymbol Type => TypeSymbol.Error;
    public SemanticContext Context { get; }
    internal BoundErrorExpression(SyntaxNode syntax, SemanticContext context) 
        : base(syntax)
    {
        Context = context;
    }
}