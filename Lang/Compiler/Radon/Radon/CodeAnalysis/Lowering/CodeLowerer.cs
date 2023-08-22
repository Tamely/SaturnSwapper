using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Operators;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Lowering;

internal sealed class CodeLowerer
{
    private readonly List<LocalVariableSymbol> _generatedLocals;
    private readonly List<BoundStatement> _generatedStatements;
    private readonly DiagnosticBag _diagnostics;
    private int _labelCount;

    public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();
    public ImmutableArray<LocalVariableSymbol> GeneratedLocals => _generatedLocals.ToImmutableArray();
    public ImmutableArray<BoundStatement> GeneratedStatements => _generatedStatements.ToImmutableArray();

    public CodeLowerer()
    {
        _generatedLocals = new List<LocalVariableSymbol>();
        _generatedStatements = new List<BoundStatement>();
        _diagnostics = new DiagnosticBag();
    }

    private BoundLabel GenerateLabel()
    {
        var label = new BoundLabel($"Label{_labelCount++}");
        return label;
    }
    
    private LocalVariableSymbol GenerateLocal(TypeSymbol type)
    {
        var name = $"<V_{_generatedLocals.Count}>";
        if (type == TypeSymbol.Bool)
        {
            name = $"flag{_generatedLocals.Count}";
        }
        else if (TypeSymbol.GetNumericTypes().Contains(type))
        {
            name = $"num{_generatedLocals.Count}";
        }
        
        var local = new LocalVariableSymbol(name, type);
        _generatedLocals.Add(local);
        return local;
    }
    
    public ImmutableArray<BoundStatement> LowerStatements(ImmutableArray<BoundStatement> nodes)
    {
        var loweredStatements = new List<BoundStatement>();
        foreach (var node in nodes)
        {
            var loweredStatement = LowerStatement(node);
            loweredStatements.Add(loweredStatement);
        }
        
        return loweredStatements.ToImmutableArray();
    }

    public BoundStatement LowerStatement(BoundStatement node)
    {
        var result = node switch
        {
            BoundBlockStatement boundBlockStatement => LowerBlockStatement(boundBlockStatement),
            BoundErrorStatement boundErrorStatement => boundErrorStatement,
            BoundExpressionStatement boundExpressionStatement => LowerExpressionStatement(boundExpressionStatement),
            BoundReturnStatement boundReturnStatement => LowerReturnStatement(boundReturnStatement),
            BoundSignStatement boundSignStatement => boundSignStatement,
            BoundVariableDeclarationStatement boundVariableDeclarationStatement =>
                LowerVariableDeclarationStatement(boundVariableDeclarationStatement),
            BoundIfStatement boundIfStatement => LowerIfStatement(boundIfStatement),
            BoundWhileStatement boundWhileStatement => LowerWhileStatement(boundWhileStatement),
            BoundForStatement boundForStatement => LowerForStatement(boundForStatement),
            BoundConditionalGotoStatement boundConditionalGotoStatement => LowerConditionalGotoStatement(boundConditionalGotoStatement),
            BoundGotoStatement boundGotoStatement => boundGotoStatement,
            BoundLabelStatement boundLabelStatement => boundLabelStatement,
            _ => throw new Exception($"Unexpected syntax {node.Kind}")
        };

        return result;
    }

    private BoundStatement LowerWithGenerated(BoundStatement node)
    {
        if (_generatedStatements.Count <= 0)
        {
            return node;
        }
        
        var statements = _generatedStatements.ToImmutableArray();
        _generatedStatements.Clear();
        return new BoundBlockStatement(node.Syntax, statements.Add(node));
    }
    
    private BoundStatement LowerBlockStatement(BoundBlockStatement node)
    {
        var statements = new List<BoundStatement>();
        foreach (var statement in node.Statements)
        {
            var loweredStatement = LowerStatement(statement);
            statements.Add(loweredStatement);
        }

        return new BoundBlockStatement(node.Syntax, statements.ToImmutableArray());
    }
    
    private BoundStatement LowerExpressionStatement(BoundExpressionStatement node)
    {
        var loweredExpression = LowerExpression(node.Expression);
        var expressionStatement = new BoundExpressionStatement(node.Syntax, loweredExpression);
        return LowerWithGenerated(expressionStatement);
    }
    
    private BoundStatement LowerReturnStatement(BoundReturnStatement node)
    {
        if (node.Expression is null)
        {
            return node;
        }
        
        var loweredExpression = LowerExpression(node.Expression);
        var returnStatement = new BoundReturnStatement(node.Syntax, loweredExpression);
        return LowerWithGenerated(returnStatement);
    }
    
    private BoundStatement LowerVariableDeclarationStatement(BoundVariableDeclarationStatement node)
    {
        if (node.Initializer is null)
        {
            var initializer = new BoundDefaultExpression(node.Syntax, node.Variable.Type);
            return new BoundVariableDeclarationStatement(node.Syntax, node.Variable, initializer);
        }
        
        var loweredInitializer = LowerExpression(node.Initializer);
        var variableDeclarationStatement = new BoundVariableDeclarationStatement(node.Syntax, node.Variable, loweredInitializer);
        return LowerWithGenerated(variableDeclarationStatement);
    }

    private BoundStatement LowerIfStatement(BoundIfStatement node)
    {
        if (node.ElseStatement == null)
        {
            // if <condition>
            //      <then>
            //
            // ----> Lowered
            //
            // gotoFalse <condition> end
            // <then>
            // end:
            //
            // ----> IL
            //
            // <condition>
            // brfalse end
            // <then>
            // end:
            
            var endLabel = GenerateLabel();
            var result = new BoundBlockStatement(
                node.Syntax,
                ImmutableArray.Create(
                    new BoundConditionalGotoStatement(
                        node.Syntax,
                        endLabel,
                        node.Condition,
                        false
                    ),
                    node.ThenStatement,
                    new BoundLabelStatement(
                        node.Syntax,
                        endLabel
                    )
                )
            );
            
            return LowerStatement(result);
        }
        else
        {
            // if <condition>
            //      <then>
            // else
            //      <else>
            //
            // ---->
            //
            // gotoFalse <condition> else
            // <then>
            // goto end
            // else:
            // <else>
            // end:
            
            var elseLabel = GenerateLabel();
            var endLabel = GenerateLabel();
            var result = new BoundBlockStatement(
                node.Syntax,
                ImmutableArray.Create(
                    new BoundConditionalGotoStatement(
                        node.Syntax, 
                        elseLabel, 
                        node.Condition, 
                        false
                    ),
                    node.ThenStatement,
                    new BoundGotoStatement(
                        node.Syntax,
                        endLabel
                    ),
                    new BoundLabelStatement(node.Syntax,
                        elseLabel
                    ),
                    node.ElseStatement,
                    new BoundLabelStatement(node.Syntax,
                        endLabel
                    )
                )
            );
            
            return LowerStatement(result);
        }
    }
    
    private BoundStatement LowerWhileStatement(BoundWhileStatement node)
    {
        // while <condition>
        //      <body>
        //
        // ----->
        //
        // goto continue
        // body:
        // <body>
        // continue:
        // gotoTrue <condition> body
        // break:
        
        var bodyLabel = GenerateLabel();
        var result = new BoundBlockStatement(
            node.Syntax,
            ImmutableArray.Create(
                new BoundGotoStatement(
                    node.Syntax,
                    node.ContinueLabel
                ),
                new BoundLabelStatement(
                    node.Syntax,
                    bodyLabel
                ),
                node.Body,
                new BoundLabelStatement(
                    node.Syntax,
                    node.ContinueLabel
                ),
                new BoundConditionalGotoStatement(
                    node.Syntax,
                    bodyLabel,
                    node.Condition,
                    true
                ),
                new BoundLabelStatement(
                    node.Syntax,
                    node.BreakLabel
                )
            )
        );
        
        return LowerStatement(result);
    }
    
    private BoundStatement LowerForStatement(BoundForStatement node)
    {
        // for <initializer; condition; increment/action>
        //      <body>
        //
        // ----->
        //
        // <initializer>
        // gotoTrue <condition> body
        // break:
        // goto end
        // body:
        // <body>
        // continue:
        // <increment/action>
        // gotoTrue <condition> body
        // end:
        
        var bodyLabel = GenerateLabel();
        var endLabel = GenerateLabel();
        var result = new BoundBlockStatement(
            node.Syntax,
            ImmutableArray.Create(
                node.Initializer,
                new BoundConditionalGotoStatement(
                    node.Syntax,
                    bodyLabel,
                    node.Condition,
                    true
                ),
                new BoundLabelStatement(
                    node.Syntax,
                    node.BreakLabel
                ),
                new BoundGotoStatement(
                    node.Syntax,
                    endLabel
                ),
                new BoundLabelStatement(
                    node.Syntax,
                    bodyLabel
                ),
                node.Body,
                new BoundLabelStatement(
                    node.Syntax,
                    node.ContinueLabel
                ),
                new BoundExpressionStatement(
                    node.Syntax,
                    node.Action
                ),
                new BoundConditionalGotoStatement(
                    node.Syntax,
                    bodyLabel,
                    node.Condition,
                    true
                ),
                new BoundLabelStatement(
                    node.Syntax,
                    endLabel
                )
            )
        );
        
        return LowerStatement(result);
    }
    
    private BoundStatement LowerConditionalGotoStatement(BoundConditionalGotoStatement node)
    {
        var condition = LowerExpression(node.Condition);
        var conditionalGotoStatement = new BoundConditionalGotoStatement(node.Syntax, node.Label, condition, node.JumpIfTrue);
        return LowerWithGenerated(conditionalGotoStatement);
    }
    
    public BoundExpression LowerExpression(BoundNode node)
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
        var loweredLeft = LowerExpression(node.Left);
        var loweredRight = LowerExpression(node.Right);
        if (node.AssignmentOperatorKind.HasAttribute(SKAttributes.CompoundAssignment))
        {
            var text = node.AssignmentOperatorKind.Text;
            var operatorText = text?[..^1];
            if (operatorText is null)
            {
                throw new Exception($"Unexpected assignment operator {node.AssignmentOperatorKind}");
            }

            var operatorToken = SyntaxKind.GetKind(operatorText);
            var boundOperator = BoundBinaryOperator.Bind(operatorToken, loweredLeft.Type, loweredRight.Type);
            if (boundOperator is null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(node.Syntax.Location, operatorText, loweredLeft.Type, loweredRight.Type);
                return node;
            }
            
            var result = new BoundAssignmentExpression(
                node.Syntax,
                loweredLeft,
                new BoundBinaryExpression(
                    node.Syntax,
                    loweredLeft,
                    boundOperator,
                    loweredRight
                ),
                SyntaxKind.EqualsToken
            );

            return LowerExpression(result);
        }
        
        return new BoundAssignmentExpression(node.Syntax, loweredLeft, loweredRight, node.AssignmentOperatorKind);
    }
    
    private BoundStatement GenerateShortCircuiting(BoundNode node, BoundExpression loweredLeft, 
        BoundExpression loweredRight, bool jumpIfTrue, out LocalVariableSymbol resultLocal)
    {
        resultLocal = GenerateLocal(TypeSymbol.Bool);
        var endLabel = GenerateLabel();
        var result = new BoundBlockStatement(
            node.Syntax,
            ImmutableArray.Create<BoundStatement>(
                new BoundVariableDeclarationStatement(
                    node.Syntax,
                    resultLocal,
                    loweredLeft
                ),
                new BoundConditionalGotoStatement(
                    node.Syntax,
                    endLabel,
                    new BoundNameExpression(
                        node.Syntax,
                        resultLocal
                    ),
                    jumpIfTrue
                ),
                new BoundExpressionStatement(
                    node.Syntax,
                    new BoundAssignmentExpression(
                        node.Syntax,
                        new BoundNameExpression(
                            node.Syntax,
                            resultLocal
                        ),
                        loweredRight,
                        SyntaxKind.EqualsToken
                    )
                ),
                new BoundLabelStatement(
                    node.Syntax,
                    endLabel
                )
            )
        );

        return result;
    }
    
    private BoundExpression LowerBinaryExpression(BoundBinaryExpression node)
    {
        var loweredLeft = LowerExpression(node.Left);
        var loweredRight = LowerExpression(node.Right);
        if (node.Op.Kind is BoundBinaryOperatorKind.LogicalOr or BoundBinaryOperatorKind.LogicalAnd)
        {
            // bool x = <left> || <right>
            //
            // ----->
            //
            // bool x;
            // bool result = <left>
            // gotoTrue <left> end
            // x = <right>
            // end:
            // x = result
            //
            // ----->
            //
            // bool x = <left> && <right>
            //
            // ----->
            //
            // bool x;
            // bool result = <left>
            // gotoFalse <left> end
            // x = <right>
            // end:
            // x = result
            
            var jumpIfTrue = node.Op.Kind == BoundBinaryOperatorKind.LogicalOr;
            var result = GenerateShortCircuiting(node, loweredLeft, loweredRight, jumpIfTrue, out var resultLocal);
            var statement = LowerStatement(result);
            _generatedStatements.Add(statement);
            return new BoundNameExpression(node.Syntax, resultLocal);
        }
        
        return new BoundBinaryExpression(node.Syntax, loweredLeft, node.Op, loweredRight);
    }
    
    private BoundExpression LowerConversionExpression(BoundConversionExpression node)
    {
        var loweredExpression = LowerExpression(node.Expression);
        return new BoundConversionExpression(node.Syntax, node.Type, loweredExpression);
    }
    
    private BoundExpression LowerElementAccessExpression(BoundElementAccessExpression node)
    {
        var loweredExpression = LowerExpression(node.Expression);
        var loweredIndex = LowerExpression(node.IndexExpression);
        return new BoundElementAccessExpression(node.Syntax, node.Type, loweredExpression, loweredIndex);
    }
    
    private BoundExpression LowerImportExpression(BoundImportExpression node)
    {
        // from_ar = import "ar"
        // Becomes:
        // from_ar = archive.Import("ar")
        var loweredPath = LowerExpression(node.Path);
        var importMethod = TypeSymbol.Archive.GetMember("Import");
        if (importMethod is not MethodSymbol symbol)
        {
            throw new Exception("archive.Import method not found");
        }
        
        var result = new BoundInvocationExpression(
            node.Syntax,
            symbol,
            new BoundMemberAccessExpression(
                node.Syntax,
                new BoundNameExpression(
                    node.Syntax,
                    TypeSymbol.Archive
                ),
                importMethod
            ),
            ImmutableArray.Create(loweredPath),
            TypeSymbol.Archive
        );
        
        return LowerExpression(result);
    }
    
    private BoundExpression LowerInvocationExpression(BoundInvocationExpression node)
    {
        var loweredArguments = LowerArguments(node.Arguments);
        BoundExpression result;
        if (node.Expression is BoundNameExpression)
        {
            // We know it's calling a method in the same type
            var symbol = node.Method;
            if (symbol.HasModifier(SyntaxKind.StaticKeyword))
            {
                result = new BoundMemberAccessExpression(
                    node.Expression.Syntax, 
                    new BoundNameExpression(
                        node.Expression.Syntax,
                        symbol.ParentType
                    ),
                    symbol
                );
                
                result = LowerExpression(result);
            }
            else
            {
                result = new BoundMemberAccessExpression(
                    node.Expression.Syntax, 
                    new BoundThisExpression(
                        node.Expression.Syntax,
                        symbol.ParentType
                    ),
                    symbol
                );
            }
            
            result = LowerExpression(result);
        }
        else
        {
            result = LowerExpression(node.Expression);
        }
        
        return new BoundInvocationExpression(node.Syntax, node.Method, result, loweredArguments, node.Type);
    }
    
    private BoundExpression LowerMemberAccessExpression(BoundMemberAccessExpression node)
    {
        var result = node.Expression;
        var loweredExpression = LowerExpression(result);
        return new BoundMemberAccessExpression(node.Syntax, loweredExpression, node.Member);
    }
    
    private BoundExpression LowerNameExpression(BoundNameExpression node)
    {
        if (node.Symbol is not MemberSymbol member)
        {
            return node;
        }

        BoundExpression result;
        if (member.HasModifier(SyntaxKind.StaticKeyword))
        {
            result = new BoundMemberAccessExpression(
                node.Syntax, 
                new BoundNameExpression(
                    node.Syntax, 
                    member.ParentType
                ), 
                member
            );
        }
        else
        {
            result = new BoundMemberAccessExpression(
                node.Syntax, 
                new BoundThisExpression(
                    node.Syntax, 
                    member.ParentType
                ), 
                member
            );
        }
        
        return LowerExpression(result);
    }
    
    private BoundExpression LowerNewArrayExpression(BoundNewArrayExpression node)
    {
        var loweredSize = LowerExpression(node.SizeExpression);
        return new BoundNewArrayExpression(node.Syntax, node.Type, loweredSize);
    }
    
    private BoundExpression LowerNewExpression(BoundNewExpression node)
    {
        var loweredArguments = LowerArguments(node.Arguments);
        return new BoundNewExpression(node.Syntax, node.Type, node.Constructor, loweredArguments);
    }
    
    private BoundExpression LowerUnaryExpression(BoundUnaryExpression node)
    {
        var loweredOperand = LowerExpression(node.Operand);
        if (node.Op.Kind is BoundUnaryOperatorKind.Decrement or BoundUnaryOperatorKind.Increment)
        {
            var opToken = node.Op.Kind == BoundUnaryOperatorKind.Decrement 
                ? SyntaxKind.MinusEqualsToken 
                : SyntaxKind.PlusEqualsToken;
            var op = BoundBinaryOperator.Bind(opToken, loweredOperand.Type, loweredOperand.Type);
            if (op == null)
            {
                var syntax = node.Syntax as UnaryExpressionSyntax;
                var location = syntax!.OperatorToken.Location;
                _diagnostics.ReportUndefinedUnaryOperator(location, opToken.Text!, loweredOperand.Type);
                return node;
            }
            
            var result = new BoundAssignmentExpression(
                node.Syntax,
                loweredOperand,
                new BoundLiteralExpression(
                    node.Syntax,
                    loweredOperand.Type,
                    1
                ),
                opToken
            );
            
            return LowerExpression(result);
        }

        if (node.Op.Kind is BoundUnaryOperatorKind.LogicalNot)
        {
            // bool x = !<operand>
            //
            // ----->
            //
            // bool x;
            // bool result = <operand>
            // result = (result == false)
            // x = result
            
            var resultLocal = GenerateLocal(TypeSymbol.Bool);
            var equalsOperator = BoundBinaryOperator.Bind(SyntaxKind.EqualsEqualsToken, TypeSymbol.Bool, TypeSymbol.Bool)!;
            var result = new BoundBlockStatement(
                node.Syntax,
                ImmutableArray.Create<BoundStatement>(
                    new BoundVariableDeclarationStatement(
                        node.Syntax,
                        resultLocal,
                        loweredOperand
                    ),
                    new BoundExpressionStatement(
                        node.Syntax,
                        new BoundAssignmentExpression(
                            node.Syntax,
                            new BoundNameExpression(
                                node.Syntax,
                                resultLocal
                            ),
                            new BoundBinaryExpression(
                                node.Syntax,
                                new BoundNameExpression(
                                    node.Syntax,
                                    resultLocal
                                ),
                                equalsOperator,
                                new BoundLiteralExpression(
                                    node.Syntax,
                                    TypeSymbol.Bool,
                                    false
                                )
                            ),
                            SyntaxKind.EqualsToken
                        )
                    )
                )
            );
            
            var statement = LowerStatement(result);
            _generatedStatements.Add(statement);
            return new BoundNameExpression(node.Syntax, resultLocal);
        }
        
        return new BoundUnaryExpression(node.Syntax, node.Op, loweredOperand);
    }
    
    private ImmutableArray<BoundExpression> LowerArguments(ImmutableArray<BoundExpression> arguments)
    {
        var builder = ImmutableArray.CreateBuilder<BoundExpression>();
        foreach (var argument in arguments)
        {
            var loweredArgument = LowerExpression(argument);
            builder.Add(loweredArgument);
        }
        
        return builder.ToImmutable();
    }
}
