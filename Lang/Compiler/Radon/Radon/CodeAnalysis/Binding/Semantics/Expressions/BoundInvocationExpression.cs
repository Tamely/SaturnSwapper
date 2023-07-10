using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundInvocationExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.InvocationExpression;
    public override TypeSymbol Type { get; }
    public AbstractMethodSymbol Method { get; }
    public BoundExpression Expression { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    public BoundInvocationExpression(SyntaxNode syntax, AbstractMethodSymbol method, BoundExpression expression,
                                     ImmutableArray<BoundExpression> arguments, TypeSymbol returnType)
        : base(syntax)
    {
        Method = method;
        Expression = expression;
        Arguments = arguments;
        Type = returnType;
    }
}