using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Lowering;

internal sealed class ExpressionLowerer
{
    public BoundExpression Lower(BoundExpression node)
    {
        return node switch
        {
            BoundAssignmentExpression boundAssignmentExpression => LowerAssignmentExpression(boundAssignmentExpression),
            BoundBinaryExpression boundBinaryExpression => LowerBinaryExpression(boundBinaryExpression),
            BoundConversionExpression boundConversionExpression => LowerConversionExpression(boundConversionExpression),
            BoundDefaultExpression boundDefaultExpression => boundDefaultExpression,
            BoundElementAccessExpression boundElementAccessExpression => LowerElementAccessExpression(boundElementAccessExpression),
            BoundErrorExpression boundErrorExpression => boundErrorExpression,
            BoundImportExpression boundImportExpression => LowerImportExpression(boundImportExpression),
            BoundInvocationExpression boundInvocationExpression => LowerInvocationExpression(boundInvocationExpression),
            BoundLiteralExpression boundLiteralExpression => boundLiteralExpression,
            BoundMemberAccessExpression boundMemberAccessExpression => LowerMemberAccessExpression(boundMemberAccessExpression),
            BoundNameExpression boundNameExpression => LowerNameExpression(boundNameExpression),
            BoundNewArrayExpression boundNewArrayExpression => LowerNewArrayExpression(boundNewArrayExpression),
            BoundNewExpression boundNewExpression => LowerNewExpression(boundNewExpression),
            BoundThisExpression boundThisExpression => boundThisExpression,
            BoundUnaryExpression boundUnaryExpression => LowerUnaryExpression(boundUnaryExpression),
            _ => throw new Exception($"Unexpected syntax {node.Kind}")
        };
    }
    
    private BoundExpression LowerAssignmentExpression(BoundAssignmentExpression node)
    {
        var loweredLeft = Lower(node.Left);
        var loweredRight = Lower(node.Right);
        return new BoundAssignmentExpression(node.Syntax, loweredLeft, loweredRight);
    }
    
    private BoundExpression LowerBinaryExpression(BoundBinaryExpression node)
    {
        var loweredLeft = Lower(node.Left);
        var loweredRight = Lower(node.Right);
        return new BoundBinaryExpression(node.Syntax, loweredLeft, node.Op, loweredRight);
    }
    
    private BoundExpression LowerConversionExpression(BoundConversionExpression node)
    {
        var loweredExpression = Lower(node.Expression);
        return new BoundConversionExpression(node.Syntax, node.Type, loweredExpression);
    }
    
    private BoundExpression LowerElementAccessExpression(BoundElementAccessExpression node)
    {
        var loweredExpression = Lower(node.Expression);
        var loweredIndex = Lower(node.IndexExpression);
        return new BoundElementAccessExpression(node.Syntax, node.Type, loweredExpression, loweredIndex);
    }
    
    private BoundExpression LowerImportExpression(BoundImportExpression node)
    {
        var loweredPath = Lower(node.Path);
        return new BoundImportExpression(node.Syntax, loweredPath);
    }
    
    private BoundExpression LowerInvocationExpression(BoundInvocationExpression node)
    {
        // We need to convert something like: "bar()" to "this.bar()" or "foo.bar()"
        var loweredArguments = LowerArguments(node.Arguments);
        BoundExpression newExpression;
        if (node.Expression is BoundNameExpression)
        {
            // We know it's calling a method in the same type
            var symbol = node.Method;
            if (symbol.HasModifier(SyntaxKind.StaticKeyword))
            {
                newExpression = new BoundMemberAccessExpression(
                    node.Expression.Syntax, 
                    new BoundNameExpression(
                        node.Expression.Syntax,
                        symbol.ParentType
                    ),
                    symbol
                );
            }
            else
            {
                newExpression = new BoundMemberAccessExpression(
                    node.Expression.Syntax, 
                    new BoundThisExpression(
                        node.Expression.Syntax,
                        symbol.ParentType
                    ),
                    symbol
                );
            }
        }
        else
        {
            newExpression = Lower(node.Expression);
        }
        
        return new BoundInvocationExpression(node.Syntax, node.Method, newExpression, loweredArguments, node.Type);
    }
    
    private BoundExpression LowerMemberAccessExpression(BoundMemberAccessExpression node)
    {
        if (node.Expression is BoundNameExpression { Symbol: MemberSymbol member })
        {
            if (member.HasModifier(SyntaxKind.StaticKeyword))
            {
                return new BoundMemberAccessExpression(
                    node.Syntax, 
                    new BoundNameExpression(
                        node.Syntax, 
                        member.ParentType
                    ), 
                    node.Member
                );
            }

            return new BoundMemberAccessExpression(
                node.Syntax, 
                new BoundThisExpression(
                    node.Syntax, 
                    member.ParentType
                ), 
                node.Member
            );
        }
        
        var loweredExpression = Lower(node.Expression);
        return new BoundMemberAccessExpression(node.Syntax, loweredExpression, node.Member);
    }
    
    private BoundExpression LowerNameExpression(BoundNameExpression node)
    {
        if (node.Symbol is not MemberSymbol m)
        {
            return node;
        }
        
        if (m.HasModifier(SyntaxKind.StaticKeyword))
        {
            return new BoundMemberAccessExpression(
                node.Syntax, 
                new BoundNameExpression(
                    node.Syntax, 
                    m.ParentType
                ), 
                m
            );
        }
            
        return new BoundMemberAccessExpression(
            node.Syntax, 
            new BoundThisExpression(
                node.Syntax, 
                m.ParentType
            ), 
            m
        );
    }
    
    private BoundExpression LowerNewArrayExpression(BoundNewArrayExpression node)
    {
        var loweredSize = Lower(node.SizeExpression);
        return new BoundNewArrayExpression(node.Syntax, node.Type, loweredSize);
    }
    
    private BoundExpression LowerNewExpression(BoundNewExpression node)
    {
        var loweredArguments = LowerArguments(node.Arguments);
        return new BoundNewExpression(node.Syntax, node.Type, node.Constructor, loweredArguments);
    }
    
    private BoundExpression LowerUnaryExpression(BoundUnaryExpression node)
    {
        var loweredOperand = Lower(node.Operand);
        return new BoundUnaryExpression(node.Syntax, node.Op, loweredOperand);
    }
    
    private ImmutableArray<BoundExpression> LowerArguments(ImmutableArray<BoundExpression> arguments)
    {
        var builder = ImmutableArray.CreateBuilder<BoundExpression>();
        foreach (var argument in arguments)
        {
            var loweredArgument = Lower(argument);
            builder.Add(loweredArgument);
        }
        
        return builder.ToImmutable();
    }
}
