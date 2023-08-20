using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Members;

namespace Radon.CodeAnalysis.Binding.Binders;

internal sealed class TemplateMethodBinder : Binder
{
    internal TemplateMethodBinder(Binder binder) 
        : base(binder)
    {
    }

    public override BoundNode Bind(SyntaxNode node, params object[] args)
    {
        if (args is not [TemplateMethodSymbol templateMethod, ImmutableArray<TypeSymbol> typeArguments, 
                         SyntaxNode callSite, string name])
        {
            throw new ArgumentException("Invalid arguments passed to template method binder.");
        }
        
        var methodContext = new SemanticContext(this, node, Diagnostics)
        {
            Tag = typeArguments
        };
        
        if (templateMethod.TypeParameters.Length != typeArguments.Length)
        {
            Diagnostics.ReportIncorrectNumberOfTypeArguments(callSite.Location, templateMethod.Name, templateMethod.TypeParameters.Length, typeArguments.Length);
            return new BoundErrorMember(node, methodContext);
        }
        
        for (var i = 0; i < templateMethod.TypeParameters.Length; i++)
        {
            var typeParameter = templateMethod.TypeParameters[i];
            var typeArgument = typeArguments[i];
            Register(methodContext, new BoundTypeParameterSymbol(typeParameter, typeArgument));
        }

        var newParams = ImmutableArray.CreateBuilder<ParameterSymbol>();
        foreach (var parameter in templateMethod.Parameters)
        {
            var paramType = parameter.Type;
            if (parameter.Type is TypeParameterSymbol)
            {
                if (!TryResolveSymbol<TypeSymbol>(methodContext, ref paramType))
                {
                    paramType = TypeSymbol.Error;
                }
            }
            
            var paramSymbol = new ParameterSymbol(parameter.Name, paramType, parameter.Ordinal);
            newParams.Add(paramSymbol);
            Register(methodContext, paramSymbol);
        }

        var returnType = templateMethod.Type;
        if (!TryResolveSymbol<TypeSymbol>(methodContext, ref returnType))
        {
            returnType = TypeSymbol.Error;
        }

        var methodSymbol = new MethodSymbol(templateMethod.ParentType, name, returnType, newParams.ToImmutable(),
            templateMethod.Modifiers);
        if (node is TemplateMethodDeclarationSyntax templateMethodDeclaration)
        {
            var body = templateMethodDeclaration.Body;
            var statements = body.Statements;
            var statementBinder = new StatementBinder(this);
            var boundStatements = new List<BoundStatement>();
            foreach (var statement in statements)
            {
                var boundStatement = (BoundStatement)statementBinder.Bind(statement, templateMethod);
                boundStatements.Add(boundStatement);
            }
            
            Diagnostics.AddRange(statementBinder.Diagnostics);
            return new BoundMethod(templateMethodDeclaration, methodSymbol, boundStatements.ToImmutableArray(), statementBinder.Locals);
        }
        
        return new BoundMethod(SyntaxNode.Empty, methodSymbol, ImmutableArray<BoundStatement>.Empty, ImmutableArray<LocalVariableSymbol>.Empty);
    }
}