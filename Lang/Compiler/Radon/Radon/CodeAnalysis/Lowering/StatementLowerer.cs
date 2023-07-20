using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Statements;

namespace Radon.CodeAnalysis.Lowering;

internal sealed class StatementLowerer
{
    private readonly ExpressionLowerer _expressionLowerer;

    public StatementLowerer()
    {
        _expressionLowerer = new ExpressionLowerer();
    }
    
    public ImmutableArray<BoundStatement> LowerStatements(ImmutableArray<BoundStatement> statements)
    {
        var builder = ImmutableArray.CreateBuilder<BoundStatement>();
        foreach (var statement in statements)
        {
            var loweredStatement = LowerStatement(statement);
            builder.Add(loweredStatement);
        }
        
        return builder.ToImmutable();
    }

    private BoundStatement LowerStatement(BoundStatement node)
    {
        return node switch
        {
            BoundBlockStatement boundBlockStatement => LowerBlockStatement(boundBlockStatement),
            BoundErrorStatement boundErrorStatement => boundErrorStatement,
            BoundExpressionStatement boundExpressionStatement => LowerExpressionStatement(boundExpressionStatement),
            BoundReturnStatement boundReturnStatement => LowerReturnStatement(boundReturnStatement),
            BoundSignStatement boundSignStatement => boundSignStatement,
            BoundVariableDeclarationStatement boundVariableDeclarationStatement => LowerVariableDeclarationStatement(
                boundVariableDeclarationStatement),
            _ => throw new Exception($"Unexpected syntax {node.Kind}")
        };
    }
    
    private BoundStatement LowerBlockStatement(BoundBlockStatement node)
    {
        var loweredStatements = LowerStatements(node.Statements);
        return new BoundBlockStatement(node.Syntax, loweredStatements);
    }
    
    private BoundStatement LowerExpressionStatement(BoundExpressionStatement node)
    {
        var loweredExpression = _expressionLowerer.Lower(node.Expression);
        return new BoundExpressionStatement(node.Syntax, loweredExpression);
    }
    
    private BoundStatement LowerReturnStatement(BoundReturnStatement node)
    {
        if (node.Expression == null)
        {
            return node;
        }
        
        var loweredExpression = _expressionLowerer.Lower(node.Expression);
        return new BoundReturnStatement(node.Syntax, loweredExpression);
    }
    
    private BoundStatement LowerVariableDeclarationStatement(BoundVariableDeclarationStatement node)
    {
        if (node.Initializer == null)
        {
            var initializer = new BoundDefaultExpression(node.Syntax, node.Variable.Type);
            return new BoundVariableDeclarationStatement(node.Syntax, node.Variable, initializer);
        }
        
        var loweredInitializer = _expressionLowerer.Lower(node.Initializer);
        return new BoundVariableDeclarationStatement(node.Syntax, node.Variable, loweredInitializer);
    }
}
