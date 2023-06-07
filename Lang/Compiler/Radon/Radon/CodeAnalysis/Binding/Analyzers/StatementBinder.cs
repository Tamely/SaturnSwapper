using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal sealed class StatementBinder : Binder
{
    internal bool IsStaticMethod;
    internal AbstractMethodSymbol? MethodSymbol;
    private bool _hasRun;
    internal StatementBinder(Binder binder) 
        : base(binder)
    {
    }
    
    public ImmutableArray<LocalVariableSymbol> Locals => Scope!.Symbols.OfType<LocalVariableSymbol>().ToImmutableArray();

    public override BoundNode Bind(SyntaxNode node, params object[] args)
    {
        var statementContext = new SemanticContext(this, node, Diagnostics);
        if (!_hasRun)
        {
            if (args.Length != 1 || 
                args[0] is not AbstractMethodSymbol abstractMethodSymbol)
            {
                return new BoundErrorStatement(node, statementContext);
            }
            
            MethodSymbol = abstractMethodSymbol;
            IsStaticMethod = MethodSymbol.HasModifier(SyntaxKind.StaticKeyword);
        }

        _hasRun = true;
        if (node.Kind == SyntaxKind.BlockStatement)
        {
            return BindBlockStatement((BlockStatementSyntax)node);
        }
        
        if (node.Kind == SyntaxKind.ExpressionStatement)
        {
            return BindExpressionStatement((ExpressionStatementSyntax)node);
        }

        if (node.Kind == SyntaxKind.SignStatement)
        {
            return BindSignStatement((SignStatementSyntax)node);
        }

        if (node.Kind == SyntaxKind.VariableDeclaration)
        {
            return BindVariableDeclaration((VariableDeclarationSyntax)node);
        }

        return new BoundErrorStatement(node, statementContext);
    }
    
    private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
    {
        var statements = syntax.Statements;
        var boundStatements = new List<BoundStatement>();
        foreach (var statement in statements)
        {
            var boundStatement = (BoundStatement)Bind(statement);
            boundStatements.Add(boundStatement);
        }
        
        return new BoundBlockStatement(syntax, boundStatements.ToImmutableArray());
    }
    
    private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
    {
        var expression = syntax.Expression;
        var expressionBinder = new ExpressionBinder(this);
        var boundExpression = (BoundExpression)expressionBinder.Bind(expression);
        Diagnostics.AddRange(expressionBinder.Diagnostics);
        return new BoundExpressionStatement(syntax, boundExpression);
    }
    
    private BoundStatement BindSignStatement(SignStatementSyntax syntax)
    {
        var keyToken = syntax.KeyStringToken;
        var valueToken = syntax.ValueStringToken;
        var key = keyToken.Text;
        var value = valueToken.Text;
        return new BoundSignStatement(syntax, key, value);
    }
    
    private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
    {
        var type = syntax.Type;
        var declarator = syntax.Declarator;
        var typeContext = new SemanticContext(this, type, Diagnostics);
        if (!TryResolve<TypeSymbol>(typeContext, type.Identifier.Text, out var typeSymbol))
        {
            typeSymbol = TypeSymbol.Error;
        }
        
        var variable = declarator.Identifier.Text;
        var initializer = declarator.Initializer;
        var variableSymbol = new LocalVariableSymbol(variable, typeSymbol!);
        var variableContext = new SemanticContext(this, declarator.Identifier, Diagnostics);
        if (!Register(variableContext, variableSymbol))
        {
            var context = new SemanticContext(this, syntax, Diagnostics);
            return new BoundErrorStatement(syntax, context);
        }
        
        if (initializer == null)
        {
            return new BoundVariableDeclarationStatement(syntax, variableSymbol, null);
        }
        
        var expressionBinder = new ExpressionBinder(this);
        var boundInitializer = (BoundExpression)expressionBinder.Bind(initializer);
        var boundConversion = expressionBinder.BindConversion(boundInitializer, typeSymbol!, true);
        Diagnostics.AddRange(expressionBinder.Diagnostics);
        return new BoundVariableDeclarationStatement(syntax, variableSymbol, boundConversion);
    }
}