using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Lowering;

internal sealed class Lowerer
{
    private readonly BoundAssembly _assembly;
    public Lowerer(BoundAssembly assembly)
    {
        _assembly = assembly;
    }
    
    public BoundAssembly Lower()
    {
        var loweredTypes = LowerTypes();
        return new BoundAssembly(_assembly.Syntax, _assembly.Assembly, loweredTypes, _assembly.Diagnostics);
    }
    
    private ImmutableArray<BoundType> LowerTypes()
    {
        var builder = ImmutableArray.CreateBuilder<BoundType>();
#if DEBUG
        // ReSharper disable once NotAccessedVariable
        var counter = 0;
#endif
        foreach (var type in _assembly.Types)
        {
            builder.Add(LowerType(type));
            
#if DEBUG
            counter++;
#endif
        }
        
        return builder.ToImmutable();
    }
    
    private BoundType LowerType(BoundType node)
    {
        return node switch
        {
            BoundStruct boundStruct => LowerStruct(boundStruct),
            BoundEnum boundEnum => LowerEnum(boundEnum),
            BoundErrorType boundType => boundType,
            _ => throw new Exception($"Unexpected type {node.Kind}")
        };
    }

    private BoundStruct LowerStruct(BoundStruct node)
    {
        var structLowerer = new StructLowerer(node);
        return structLowerer.Lower();
    }
    
    private BoundEnum LowerEnum(BoundEnum node)
    {
        return node;
    }
}

internal sealed class StructLowerer
{
    private readonly BoundStruct _boundStruct;
    private readonly Dictionary<BoundField, BoundExpression> _loweredInitializers;
    private readonly StatementLowerer _statementsLowerer;

    public StructLowerer(BoundStruct boundStruct)
    {
        _boundStruct = boundStruct;
        _loweredInitializers = new Dictionary<BoundField, BoundExpression>();
        _statementsLowerer = new StatementLowerer();
    }

    public BoundStruct Lower()
    {
        var loweredMembers = new List<BoundMember>();
        foreach (var member in _boundStruct.Members)
        {
            var loweredMember = LowerMember(member);
            loweredMembers.Add(loweredMember);
        }

        var constructors = _boundStruct.Members.OfType<BoundConstructor>();
        foreach (var constructor in constructors)
        {
            var statements = constructor.Statements;
            // Add the lowered instance initializers to the start of the constructor
            var boundStatements = new List<BoundStatement>();
            foreach (var initializer in _loweredInitializers)
            {
                BoundExpression thisOrType;
                if (constructor.Symbol.HasModifier(SyntaxKind.StaticKeyword))
                {
                    thisOrType = new BoundNameExpression(
                        SyntaxNode.Empty,
                        _boundStruct.TypeSymbol
                    );
                }
                else
                {
                    thisOrType = new BoundThisExpression(
                        SyntaxNode.Empty,
                        _boundStruct.TypeSymbol
                    );
                }
                
                var assignment = new BoundAssignmentExpression(
                    SyntaxNode.Empty,
                    new BoundMemberAccessExpression(
                        SyntaxNode.Empty,
                        thisOrType,
                        initializer.Key.Symbol
                    ),
                    initializer.Value
                );
                
                boundStatements.Add(new BoundExpressionStatement(SyntaxNode.Empty, assignment));
            }
            
            var statementsLowered = _statementsLowerer.LowerStatements(statements);
            boundStatements.AddRange(statementsLowered);
            var loweredConstructor = new BoundConstructor(
                constructor.Syntax,
                constructor.Symbol,
                boundStatements.ToImmutableArray()
            );
            
            loweredMembers.Add(loweredConstructor);
        }
        
        return new BoundStruct(
            _boundStruct.Syntax,
            _boundStruct.Symbol,
            loweredMembers.ToImmutableArray()
        );
    }
    
    private BoundMember LowerMember(BoundMember node)
    {
        return node switch
        {
            BoundMethod boundMethod => LowerMethod(boundMethod),
            BoundConstructor boundConstructor => boundConstructor,
            BoundField boundField => LowerField(boundField),
            BoundErrorMember boundErrorMember => boundErrorMember,
            _ => throw new Exception($"Unexpected member {node.Kind}")
        };
    }
    
    private BoundMember LowerMethod(BoundMethod node)
    {
        var loweredStatements = _statementsLowerer.LowerStatements(node.Statements);
        return new BoundMethod(node.Syntax, node.Symbol, loweredStatements, node.Locals);
    }

    private BoundMember LowerField(BoundField node)
    {
        var expressionLowerer = new ExpressionLowerer();
        var loweredInitializer = node.Initializer == null ? 
            new BoundDefaultExpression(SyntaxNode.Empty, node.Symbol.Type) 
            : expressionLowerer.Lower(node.Initializer);
        _loweredInitializers.Add(node, loweredInitializer);
        return new BoundField(node.Syntax, node.Symbol, loweredInitializer);
    }
}

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
