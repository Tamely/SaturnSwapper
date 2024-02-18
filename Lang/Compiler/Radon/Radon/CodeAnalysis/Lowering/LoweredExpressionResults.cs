using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Statements;

namespace Radon.CodeAnalysis.Lowering;

internal sealed class LoweredExpressionResults
{
    public BoundExpression Expression { get; }
    public ImmutableArray<BoundStatement> Statements { get; }
        
    public LoweredExpressionResults(BoundExpression expression, ImmutableArray<BoundStatement> statements)
    {
        Expression = expression;
        Statements = statements;
    }
}