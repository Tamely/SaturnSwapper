using System;
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
        foreach (var type in _assembly.Types)
        {
            builder.Add(LowerType(type));
        }
        
        return builder.ToImmutable();
    }
    
    private BoundType LowerType(BoundType type)
    {
        if (type is BoundStruct boundStruct)
        {
            return LowerStruct(boundStruct);
        }
        
        if (type is BoundEnum boundEnum)
        {
            return LowerEnum(boundEnum);
        }

        if (type is BoundErrorType boundType)
        {
            return boundType;
        }

        throw new Exception($"Unexpected type {type.Kind}");
    }

    private BoundStruct LowerStruct(BoundStruct boundStruct)
    {
        var members = ImmutableArray.CreateBuilder<BoundMember>();
        var containsStaticConstructor = false;
        foreach (var member in boundStruct.Members)
        {
            if (member is BoundConstructor constructor &&
                constructor.Symbol.Modifiers.Contains(SyntaxKind.StaticKeyword))
            {
                containsStaticConstructor = true;
            }
            
            members.Add(LowerMember(member));
        }
        
        var syntax = SyntaxNode.Empty;
        ConstructorSymbol staticConstructorSymbol;
        if (!containsStaticConstructor)
        {
            staticConstructorSymbol = new ConstructorSymbol(
                boundStruct.Symbol,
                ImmutableArray<ParameterSymbol>.Empty,
                ImmutableArray.Create(SyntaxKind.StaticKeyword)
            );
        }
        else
        {
            staticConstructorSymbol = boundStruct.Members.OfType<BoundConstructor>()
                .First(c => c.Symbol.Modifiers.Contains(SyntaxKind.StaticKeyword))
                .Symbol;
        }
        
        var staticConstructorStatements = ImmutableArray.CreateBuilder<BoundStatement>();
        var fields = boundStruct.Members.OfType<BoundField>().Where(f => f.Symbol.Modifiers.Contains(SyntaxKind.StaticKeyword));
        foreach (var field in fields)
        {
            var initializer = field.Initializer ?? new BoundDefaultExpression(SyntaxNode.Empty, field.Symbol.Type);
            var assignment = new BoundExpressionStatement(
                syntax,
                new BoundAssignmentExpression(
                    syntax,
                    new BoundMemberAccessExpression(
                        syntax,
                        new BoundNameExpression(
                            syntax,
                            field.Symbol
                        ),
                        field.Symbol
                    ),
                    initializer
                )
            );
                
            staticConstructorStatements.Add(assignment);
        }

        var staticConstructor = new BoundConstructor(
            syntax,
            staticConstructorSymbol,
            staticConstructorStatements.ToImmutable()
        );
            
        members.Add(staticConstructor);

        return new BoundStruct(boundStruct.Syntax, boundStruct.Symbol, members.ToImmutable());
    }
    
    private BoundEnum LowerEnum(BoundEnum boundEnum)
    {
        var members = ImmutableArray.CreateBuilder<BoundEnumMember>();
        foreach (var member in boundEnum.Members)
        {
            members.Add((BoundEnumMember)LowerMember(member));
        }
        
        return new BoundEnum(boundEnum.Syntax, boundEnum.Symbol, members.ToImmutable());
    }
    
    private BoundMember LowerMember(BoundMember member)
    {
        if (member is BoundMethod boundMethod)
        {
            return LowerMethod(boundMethod);
        }
        
        if (member is BoundConstructor boundConstructor)
        {
            return LowerConstructor(boundConstructor);
        }
        
        if (member is BoundField boundField)
        {
            return LowerField(boundField);
        }
        
        if (member is BoundEnumMember boundEnumMember)
        {
            return boundEnumMember;
        }

        if (member is BoundErrorMember boundErrorMember)
        {
            return boundErrorMember;
        }
        
        throw new Exception($"Unexpected member {member.Kind}");
    }
    
    private BoundMethod LowerMethod(BoundMethod boundMethod)
    {
        var loweredStatements = LowerStatements(boundMethod.Statements);
        return new BoundMethod(boundMethod.Syntax, boundMethod.Symbol, loweredStatements, boundMethod.Locals);
    }
    
    private BoundConstructor LowerConstructor(BoundConstructor boundConstructor)
    {
        var loweredStatements = LowerStatements(boundConstructor.Statements);
        return new BoundConstructor(boundConstructor.Syntax, boundConstructor.Symbol, loweredStatements);
    }
    
    private BoundField LowerField(BoundField boundField)
    {
        if (boundField.Initializer is not null)
        {
            var loweredInitializer = LowerExpression(boundField.Initializer);
            return new BoundField(boundField.Syntax, boundField.Symbol, loweredInitializer);
        }
        
        return boundField;
    }
    
    private ImmutableArray<BoundStatement> LowerStatements(ImmutableArray<BoundStatement> statements)
    {
        var builder = ImmutableArray.CreateBuilder<BoundStatement>();
        foreach (var statement in statements)
        {
            builder.Add(LowerStatement(statement));
        }
        
        return builder.ToImmutable();
    }

    private BoundStatement LowerStatement(BoundStatement node)
    {
        if (node is BoundBlockStatement boundBlockStatement)
        {
            return LowerBlockStatement(boundBlockStatement);
        }
        
        if (node is BoundExpressionStatement boundExpressionStatement)
        {
            return LowerExpressionStatement(boundExpressionStatement);
        }
        
        if (node is BoundSignStatement boundSignStatement)
        {
            return LowerSignStatement(boundSignStatement);
        }
        
        if (node is BoundVariableDeclarationStatement boundVariableDeclarationStatement)
        {
            return LowerVariableDeclarationStatement(boundVariableDeclarationStatement);
        }

        if (node is BoundErrorStatement boundErrorStatement)
        {
            return boundErrorStatement;
        }
        
        throw new Exception($"Unexpected statement {node.Kind}");
    }
    
    private BoundStatement LowerBlockStatement(BoundBlockStatement node)
    {
        var loweredStatements = LowerStatements(node.Statements);
        return new BoundBlockStatement(node.Syntax, loweredStatements);
    }
    
    private BoundStatement LowerExpressionStatement(BoundExpressionStatement node)
    {
        var loweredExpression = LowerExpression(node.Expression);
        return new BoundExpressionStatement(node.Syntax, loweredExpression);
    }
    
    private BoundStatement LowerSignStatement(BoundSignStatement node)
    {
        return node;
    }
    
    private BoundStatement LowerVariableDeclarationStatement(BoundVariableDeclarationStatement node)
    {
        if (node.Initializer is null)
        {
            var initializer = new BoundDefaultExpression(node.Syntax, node.Variable.Type);
            return new BoundVariableDeclarationStatement(node.Syntax, node.Variable, initializer);
        }
        
        var loweredInitializer = LowerExpression(node.Initializer);
        return new BoundVariableDeclarationStatement(node.Syntax, node.Variable, loweredInitializer);
    }
    
    private BoundExpression LowerExpression(BoundExpression node)
    {
        if (node is BoundAssignmentExpression boundAssignmentExpression)
        {
            return LowerAssignmentExpression(boundAssignmentExpression);
        }
        
        if (node is BoundBinaryExpression boundBinaryExpression)
        {
            return LowerBinaryExpression(boundBinaryExpression);
        }
        
        if (node is BoundImportExpression boundImportExpression)
        {
            return LowerImportExpression(boundImportExpression);
        }
        
        if (node is BoundInvocationExpression boundInvocationExpression)
        {
            return LowerInvocationExpression(boundInvocationExpression);
        }
        
        if (node is BoundLiteralExpression boundLiteralExpression)
        {
            return LowerLiteralExpression(boundLiteralExpression);
        }
        
        if (node is BoundMemberAccessExpression boundMemberAccessExpression)
        {
            return LowerMemberAccessExpression(boundMemberAccessExpression);
        }
        
        if (node is BoundNameExpression boundNameExpression)
        {
            return LowerNameExpression(boundNameExpression);
        }
        
        if (node is BoundNewExpression boundNewExpression)
        {
            return LowerNewExpression(boundNewExpression);
        }
        
        if (node is BoundThisExpression boundThisExpression)
        {
            return LowerThisExpression(boundThisExpression);
        }
        
        if (node is BoundUnaryExpression boundUnaryExpression)
        {
            return LowerUnaryExpression(boundUnaryExpression);
        }
        
        if (node is BoundErrorExpression boundErrorExpression)
        {
            return boundErrorExpression;
        }
        
        throw new Exception($"Unexpected expression {node.Kind}");
    }
    
    private BoundExpression LowerAssignmentExpression(BoundAssignmentExpression node)
    {
        var loweredLeft = LowerExpression(node.Left);
        var loweredRight = LowerExpression(node.Right);
        return new BoundAssignmentExpression(node.Syntax, loweredLeft, loweredRight);
    }
    
    private BoundExpression LowerBinaryExpression(BoundBinaryExpression node)
    {
        var loweredLeft = LowerExpression(node.Left);
        var loweredRight = LowerExpression(node.Right);
        return new BoundBinaryExpression(node.Syntax, loweredLeft, node.Op, loweredRight);
    }
    
    private BoundExpression LowerImportExpression(BoundImportExpression node)
    {
        return node;
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
            newExpression = LowerExpression(node.Expression);
        }
        
        return new BoundInvocationExpression(node.Syntax, node.Method, newExpression, node.TypeMap, loweredArguments, node.Type);
    }
    
    private ImmutableDictionary<ParameterSymbol, BoundExpression> LowerArguments(ImmutableDictionary<ParameterSymbol, BoundExpression> arguments)
    {
        var builder = ImmutableDictionary.CreateBuilder<ParameterSymbol, BoundExpression>();
        foreach (var argument in arguments)
        {
            builder.Add(argument.Key, LowerExpression(argument.Value));
        }
        
        return builder.ToImmutable();
    }
    
    private BoundExpression LowerLiteralExpression(BoundLiteralExpression node)
    {
        return node;
    }
    
    private BoundExpression LowerMemberAccessExpression(BoundMemberAccessExpression node)
    {
        // Get the token before the first token of the member access expression
        var tokens = node.Syntax.Dissolve();
        var firstToken = tokens.First();
        var previousToken = firstToken.GetPreviousToken();
        if (node.Expression is BoundNameExpression name &&
            previousToken != null &&
            previousToken.Kind != SyntaxKind.DotToken &&
            previousToken.Kind != SyntaxKind.CloseParenthesisToken)
        {
            var symbol = name.Symbol;
            if (symbol is MemberSymbol member)
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
        }
        
        var loweredExpression = LowerExpression(node.Expression);
        return new BoundMemberAccessExpression(node.Syntax, loweredExpression, node.Member);
    }
    
    private BoundExpression LowerNameExpression(BoundNameExpression node)
    {
        return node;
    }
    
    private BoundExpression LowerNewExpression(BoundNewExpression node)
    {
        var loweredArguments = LowerArguments(node.Arguments);
        return new BoundNewExpression(node.Syntax, node.Type, node.TypeMap, node.Constructor, loweredArguments);
    }
    
    private BoundExpression LowerThisExpression(BoundThisExpression node)
    {
        return node;
    }

    private BoundExpression LowerUnaryExpression(BoundUnaryExpression node)
    {
        var loweredOperand = LowerExpression(node.Operand);
        return new BoundUnaryExpression(node.Syntax, node.Op, loweredOperand);
    }
}