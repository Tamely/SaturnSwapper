using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundNewExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.NewExpression;
    public override TypeSymbol Type { get; }
    public TypeMap TypeMap { get; }
    public ConstructorSymbol Constructor { get; }
    public ImmutableDictionary<ParameterSymbol, BoundExpression> Arguments { get; }

    public BoundNewExpression(SyntaxNode syntax, TypeSymbol type, TypeMap typeMap, ConstructorSymbol constructor,
        ImmutableDictionary<ParameterSymbol, BoundExpression> arguments)
        : base(syntax)
    {
        Type = type;
        TypeMap = typeMap;
        Constructor = constructor;
        Arguments = arguments;
    }
}