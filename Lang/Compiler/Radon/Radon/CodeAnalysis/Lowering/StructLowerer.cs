using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Lowering;

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

        var constructors = loweredMembers.OfType<BoundConstructor>();
        var ctorsToRemove = new List<BoundConstructor>();
        var ctorsToAdd = new List<BoundConstructor>();
        foreach (var constructor in constructors)
        {
            var statements = constructor.Statements;
            var statementsLowered = _statementsLowerer.LowerStatements(statements);
            // Add the lowered instance initializers to the start of the constructor
            var boundStatements = new List<BoundStatement>();
            foreach (var (field, initializer) in _loweredInitializers)
            {
                var isInitInConstructor = false;
                foreach (var statement in statementsLowered)
                {
                    if (statement is not BoundExpressionStatement
                        {
                            Expression: BoundAssignmentExpression init
                        })
                    {
                        continue;
                    }
                    
                    if (init.Left is BoundMemberAccessExpression { Member: FieldSymbol fieldSymbol } &&
                        fieldSymbol == field.Symbol)
                    {
                        isInitInConstructor = true;
                        break;
                    }
                }
                
                if (isInitInConstructor)
                {
                    continue;
                }
                
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
                        field.Symbol
                    ),
                    initializer
                );
                
                boundStatements.Add(new BoundExpressionStatement(SyntaxNode.Empty, assignment));
            }

            boundStatements.AddRange(statementsLowered);
            var loweredConstructor = new BoundConstructor(
                constructor.Syntax,
                constructor.Symbol,
                boundStatements.ToImmutableArray(),
                constructor.Locals
            );
            
            ctorsToAdd.Add(loweredConstructor);
            ctorsToRemove.Add(constructor);
        }
        
        foreach (var ctor in ctorsToRemove)
        {
            loweredMembers.Remove(ctor);
        }
        
        loweredMembers.AddRange(ctorsToAdd);
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
