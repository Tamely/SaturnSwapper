using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
    private readonly Dictionary<BoundField, BoundExpression> _initializers;
    private readonly DiagnosticBag _diagnosticBag;

    public ImmutableArray<Diagnostic> Diagnostics => _diagnosticBag.ToImmutableArray();

    public StructLowerer(BoundStruct boundStruct)
    {
        _boundStruct = boundStruct;
        _initializers = new Dictionary<BoundField, BoundExpression>();
        _diagnosticBag = new DiagnosticBag();
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
            var lowerer = new CodeLowerer();
            var loweredStatements = lowerer.LowerStatements(statements);
            _diagnosticBag.AddRange(lowerer.Diagnostics);
            // Add the lowered instance initializers to the start of the constructor
            var boundStatements = new List<BoundStatement>();
            foreach (var (field, initializer) in _initializers)
            {
                var isInitInConstructor = false;
                foreach (var statement in loweredStatements)
                {
                    if (statement is not BoundExpressionStatement
                        {
                            Expression: BoundAssignmentExpression init
                        })
                    {
                        continue;
                    }

                    if (init.Left is not BoundMemberAccessExpression { Member: FieldSymbol fieldSymbol } ||
                        fieldSymbol != field.Symbol)
                    {
                        continue;
                    }
                    
                    isInitInConstructor = true;
                    break;
                }
                
                if (isInitInConstructor)
                {
                    continue;
                }

                var syntax = initializer.Syntax;
                var loweredInitializer = lowerer.LowerExpression(initializer);
                BoundExpression thisOrType;
                if (constructor.Symbol.HasModifier(SyntaxKind.StaticKeyword))
                {
                    thisOrType = new BoundNameExpression(
                        syntax,
                        _boundStruct.TypeSymbol
                    );
                }
                else
                {
                    thisOrType = new BoundThisExpression(
                        syntax,
                        _boundStruct.TypeSymbol
                    );
                }
                
                var assignment = new BoundAssignmentExpression(
                    SyntaxNode.Empty,
                    new BoundMemberAccessExpression(
                        syntax,
                        thisOrType,
                        field.Symbol
                    ),
                    loweredInitializer,
                    SyntaxKind.EqualsToken
                );
                
                boundStatements.AddRange(lowerer.GeneratedStatements);
                boundStatements.Add(new BoundExpressionStatement(syntax, assignment));
            }

            boundStatements.AddRange(loweredStatements);
            var locals = constructor.Locals;
            if (lowerer.GeneratedLocals.Length > 0)
            {
                locals = locals.AddRange(lowerer.GeneratedLocals);
            }
            
            var loweredConstructor = new BoundConstructor(
                constructor.Syntax,
                constructor.Symbol,
                boundStatements.ToImmutableArray(),
                locals
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
        var lowerer = new CodeLowerer();
        var loweredStatements = lowerer.LowerStatements(node.Statements);
        _diagnosticBag.AddRange(lowerer.Diagnostics);
        var locals = node.Locals;
        if (lowerer.GeneratedLocals.Length > 0)
        {
            locals = locals.AddRange(lowerer.GeneratedLocals);
        }
        
        return new BoundMethod(node.Syntax, node.Symbol, loweredStatements, locals);
    }

    private BoundMember LowerField(BoundField node)
    {
        var loweredInitializer = node.Initializer ?? new BoundDefaultExpression(node.Syntax, node.Symbol.Type);
        var field = new BoundField(node.Syntax, node.Symbol, loweredInitializer);
        _initializers.Add(field, loweredInitializer);
        return field;
    }
}