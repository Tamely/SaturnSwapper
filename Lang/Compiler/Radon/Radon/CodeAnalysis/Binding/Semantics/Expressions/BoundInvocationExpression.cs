using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundInvocationExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.InvocationExpression;
    public override TypeSymbol Type { get; }
    public MethodSymbol Method { get; }
    public BoundExpression Expression { get; }
    public TypeMap TypeMap { get; }
    public ImmutableDictionary<ParameterSymbol, BoundExpression> Arguments { get; }

    public BoundInvocationExpression(SyntaxNode syntax, MethodSymbol method, BoundExpression expression, TypeMap typeMap, 
                                     ImmutableDictionary<ParameterSymbol, BoundExpression> arguments, 
                                     TypeSymbol returnType)
        : base(syntax)
    {
        Method = method;
        Expression = expression;
        TypeMap = typeMap;
        Arguments = arguments;
        Type = returnType;
    }
}