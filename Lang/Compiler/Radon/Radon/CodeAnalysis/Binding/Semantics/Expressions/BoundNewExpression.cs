using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundNewExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.NewExpression;
    public override TypeSymbol Type { get; }
    public ConstructorSymbol Constructor { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    public BoundNewExpression(SyntaxNode syntax, TypeSymbol type, ConstructorSymbol constructor,
        ImmutableArray<BoundExpression> arguments)
        : base(syntax)
    {
        Type = type;
        Constructor = constructor;
        Arguments = arguments;
    }
}