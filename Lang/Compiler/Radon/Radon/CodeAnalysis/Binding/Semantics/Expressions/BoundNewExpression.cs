using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundNewExpression : BoundExpression, ISymbolExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.NewExpression;
    public override TypeSymbol Type { get; }
    public ConstructorSymbol Constructor { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }
    public Symbol Symbol => Constructor;

    public BoundNewExpression(SyntaxNode syntax, TypeSymbol type, ConstructorSymbol constructor,
        ImmutableArray<BoundExpression> arguments)
        : base(syntax)
    {
        Type = type;
        Constructor = constructor;
        Arguments = arguments;
    }
}