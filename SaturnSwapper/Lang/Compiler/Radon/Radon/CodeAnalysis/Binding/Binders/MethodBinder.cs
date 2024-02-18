using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Members;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Binding.Binders;

internal sealed class MethodBinder : Binder
{
    internal MethodBinder(Scope? scope, TextLocation location) 
        : base(scope, location)
    {
    }

    internal MethodBinder(Binder binder, TextLocation location) 
        : base(binder, location)
    {
    }

    private BoundMember BindMember(SemanticContext context, SyntaxNode node, AbstractMethodSymbol abstractMethodSymbol)
    {
        switch (abstractMethodSymbol)
        {
            case MethodSymbol methodSymbol:
                return BindMethod(context, (MethodDeclarationSyntax)node, methodSymbol);
            case TemplateMethodSymbol templateMethodSymbol:
                return BindTemplateMethod(context, (TemplateMethodDeclarationSyntax)node, templateMethodSymbol);
            case ConstructorSymbol constructorSymbol:
                return BindConstructor(context, (ConstructorDeclarationSyntax)node, constructorSymbol);
            default:
                throw new ArgumentException("The provided symbol is not supported");
        }
    }
    
    public override BoundNode Bind(SyntaxNode node, params object[] args)
    {
        var methodContext = new SemanticContext(this, node, Diagnostics);
        if (args is not [AbstractMethodSymbol abstractMethodSymbol])
        {
            throw new ArgumentException("The arguments passed to the method binder must be of type AbstractMethodSymbol.");
        }

        return BindMember(methodContext, node, abstractMethodSymbol);
    }
    
    private (ImmutableArray<BoundStatement> Statements, ImmutableArray<LocalVariableSymbol> Locals) BindStatements(SyntaxNode node, IEnumerable<StatementSyntax> statements, AbstractMethodSymbol symbol)
    {
        var statementBinder = new StatementBinder(this, node.Location);
        var boundStatements = new List<BoundStatement>();
        foreach (var statement in statements)
        {
            var boundStatement = (BoundStatement)statementBinder.Bind(statement, symbol);
            boundStatements.Add(boundStatement);
        }

        Diagnostics.AddRange(statementBinder.Diagnostics);
        var locals = statementBinder.Locals;
        foreach (var local in locals)
        {
            if (!local.IsInitialized)
            {
                var declaration = statementBinder.GetDeclaration(local);
                Diagnostics.ReportVariableNotInitialized(declaration, local);
            }
        }
        
        return (boundStatements.ToImmutableArray(), statementBinder.Locals);
    }
    
    private BoundMethod BindMethod(SemanticContext context, MethodDeclarationSyntax syntax, MethodSymbol symbol)
    {
        foreach (var parameter in symbol.Parameters)
        {
            Register(context, parameter);
        }

        BindTypeSyntax(syntax.ReturnType);
        var (boundStatements, locals) = BindStatements(syntax.Body, syntax.Body.Statements, symbol);
        return new BoundMethod(syntax, symbol, boundStatements, locals);
    }
    
    private BoundTemplateMethod BindTemplateMethod(SemanticContext context, TemplateMethodDeclarationSyntax syntax,
        TemplateMethodSymbol symbol)
    {
        foreach (var typeParameter in symbol.TypeParameters)
        {
            Register(context, typeParameter);
        }
        
        foreach (var parameter in symbol.Parameters)
        {
            Register(context, parameter);
        }
        
        BindTypeSyntax(syntax.ReturnType);
        var (boundStatements, locals) = BindStatements(syntax.Body, syntax.Body.Statements, symbol);
        return new BoundTemplateMethod(syntax, symbol, boundStatements, locals);
    }
    
    private BoundConstructor BindConstructor(SemanticContext context, ConstructorDeclarationSyntax syntax, ConstructorSymbol symbol)
    {
        foreach (var parameter in symbol.Parameters)
        {
            Register(context, parameter);
        }
        
        var (boundStatements, locals) = BindStatements(syntax.Body, syntax.Body.Statements, symbol);
        return new BoundConstructor(syntax, symbol, boundStatements, locals);
    }
}