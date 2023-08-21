using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;

namespace Radon.CodeAnalysis.Binding.Binders;

internal sealed class StatementBinder : Binder
{
    private readonly Stack<LoopConditions> _loopStack;
    private readonly List<LocalVariableSymbol> _locals;
    private int _labelCounter;
    internal bool IsStaticMethod;
    internal AbstractMethodSymbol? MethodSymbol;
    private bool _hasRun;
    internal StatementBinder(Binder binder) 
        : base(binder)
    {
        _loopStack = new Stack<LoopConditions>();
        _locals = new List<LocalVariableSymbol>();
    }
    
    public ImmutableArray<LocalVariableSymbol> Locals => _locals.ToImmutableArray();

    public override BoundNode Bind(SyntaxNode node, params object[] args)
    {
        var statementContext = new SemanticContext(this, node, Diagnostics);
        if (!_hasRun)
        {
            if (args is not [AbstractMethodSymbol abstractMethodSymbol])
            {
                return new BoundErrorStatement(node, statementContext);
            }
            
            MethodSymbol = abstractMethodSymbol;
            IsStaticMethod = MethodSymbol.HasModifier(SyntaxKind.StaticKeyword);
        }

        _hasRun = true;

        return node switch
        {
            BlockStatementSyntax blockStatement => BindBlockStatement(blockStatement),
            ExpressionStatementSyntax expressionStatement => BindExpressionStatement(expressionStatement),
            SignStatementSyntax signStatement => BindSignStatement(signStatement),
            VariableDeclarationSyntax variableDeclaration => BindVariableDeclaration(variableDeclaration),
            ReturnStatementSyntax returnStatement => BindReturnStatement(returnStatement),
            IfStatementSyntax ifStatement => BindIfStatement(ifStatement),
            WhileStatementSyntax whileStatement => BindWhileStatement(whileStatement),
            ForStatementSyntax forStatement => BindForStatement(forStatement),
            BreakStatementSyntax breakStatement => BindBreakStatement(breakStatement),
            ContinueStatementSyntax continueStatement => BindContinueStatement(continueStatement),
            _ => new BoundErrorStatement(node, statementContext)
        };
    }
    
    private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
    {
        Scope = Scope?.CreateChild();
        var statements = syntax.Statements;
        var boundStatements = new List<BoundStatement>();
        foreach (var statement in statements)
        {
            var boundStatement = (BoundStatement)Bind(statement);
            boundStatements.Add(boundStatement);
        }
        
        Scope = Scope?.Parent;
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
        var expressionBinder = new ExpressionBinder(this);
        var boundKey = (BoundExpression)expressionBinder.Bind(syntax.KeyExpression);
        var boundValue = (BoundExpression)expressionBinder.Bind(syntax.ValueExpression);
        Diagnostics.AddRange(expressionBinder.Diagnostics);
        if (boundKey.Type != TypeSymbol.String)
        {
            Diagnostics.ReportSignKeyMustBeString(syntax.KeyExpression.Location);
            return new BoundErrorStatement(syntax, new SemanticContext(this, syntax, Diagnostics));
        }
        
        if (boundValue.Type != TypeSymbol.String &&
            boundValue.Type != TypeSymbol.Bool &&
            !TypeSymbol.GetNumericTypes().Contains(boundValue.Type))
        {
            Diagnostics.ReportSignValueMustBeStringBoolOrNumeric(syntax.ValueExpression.Location);
            return new BoundErrorStatement(syntax, new SemanticContext(this, syntax, Diagnostics));
        }
        
        if (boundKey.ConstantValue is null ||
            boundValue.ConstantValue is null)
        {
            Diagnostics.ReportNullConstantValue(syntax.Location);
            return new BoundErrorStatement(syntax, new SemanticContext(this, syntax, Diagnostics));
        }

        var key = boundKey.ConstantValue.Value.ToString();
        var value = boundValue.ConstantValue.Value.ToString();
        if (key is null ||
            value is null)
        {
            Diagnostics.ReportNullConstantValue(syntax.Location);
            return new BoundErrorStatement(syntax, new SemanticContext(this, syntax, Diagnostics));
        }
        
        return new BoundSignStatement(syntax, key, value);
    }
    
    private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
    {
        var type = BindTypeSyntax(syntax.Type);
        var declarator = syntax.Declarator;
        var variable = declarator.Identifier.Text;
        var initializer = declarator.Initializer;
        var variableSymbol = new LocalVariableSymbol(variable, type);
        var variableContext = new SemanticContext(this, declarator.Identifier, Diagnostics);
        if (!Register(variableContext, variableSymbol))
        {
            var context = new SemanticContext(this, syntax, Diagnostics);
            return new BoundErrorStatement(syntax, context);
        }
        
        _locals.Add(variableSymbol);
        if (initializer == null)
        {
            return new BoundVariableDeclarationStatement(syntax, variableSymbol, null);
        }
        
        var expressionBinder = new ExpressionBinder(this);
        var boundInitializer = (BoundExpression)expressionBinder.Bind(initializer);
        var boundConversion = expressionBinder.BindConversion(boundInitializer, type, ImmutableArray<TypeSymbol>.Empty);
        Diagnostics.AddRange(expressionBinder.Diagnostics);
        return new BoundVariableDeclarationStatement(syntax, variableSymbol, boundConversion);
    }

    private BoundStatement BindReturnStatement(ReturnStatementSyntax syntax)
    {
        var expression = syntax.Expression;
        if (MethodSymbol?.Type == TypeSymbol.Void &&
            expression is not null)
        {
            Diagnostics.ReportCannotReturnExpressionFromVoidMethod(syntax.Expression!.Location);
            return new BoundErrorStatement(syntax, new SemanticContext(this, syntax, Diagnostics));
        }
        
        if (MethodSymbol?.Type != TypeSymbol.Void &&
            expression is null)
        {
            Diagnostics.ReportMustReturnExpressionFromNonVoidMethod(syntax.ReturnKeyword.Location);
            return new BoundErrorStatement(syntax, new SemanticContext(this, syntax, Diagnostics));
        }
        
        if (expression is null)
        {
            return new BoundReturnStatement(syntax, null);
        }
        
        var expressionBinder = new ExpressionBinder(this);
        var boundExpression = (BoundExpression)expressionBinder.Bind(expression);
        var boundConversion = expressionBinder.BindConversion(boundExpression, MethodSymbol!.Type, ImmutableArray<TypeSymbol>.Empty);
        Diagnostics.AddRange(expressionBinder.Diagnostics);
        return new BoundReturnStatement(syntax, boundConversion);
    }

    private BoundStatement BindIfStatement(IfStatementSyntax syntax)
    {
        var expressionBinder = new ExpressionBinder(this);
        var boundCondition = (BoundExpression)expressionBinder.Bind(syntax.Condition);
        var boundConversion = expressionBinder.BindConversion(boundCondition, TypeSymbol.Bool, ImmutableArray<TypeSymbol>.Empty);
        Diagnostics.AddRange(expressionBinder.Diagnostics);
        Scope = Scope?.CreateChild();
        var boundThenStatement = (BoundStatement)Bind(syntax.ThenStatement);
        Scope = Scope?.Parent!;
        BoundStatement? boundElseStatement = null;
        if (syntax.ElseClause is not null)
        {
            Scope = Scope?.CreateChild();
            boundElseStatement = (BoundStatement)Bind(syntax.ElseClause.ElseStatement);
            Scope = Scope?.Parent!;
        }
        
        return new BoundIfStatement(syntax, boundConversion, boundThenStatement, boundElseStatement);
    }

    private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
    {
        var expressionBinder = new ExpressionBinder(this);
        var boundCondition = (BoundExpression)expressionBinder.Bind(syntax.Condition);
        var boundConversion = expressionBinder.BindConversion(boundCondition, TypeSymbol.Bool, ImmutableArray<TypeSymbol>.Empty);
        Diagnostics.AddRange(expressionBinder.Diagnostics);
        var boundBody = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
        return new BoundWhileStatement(syntax, boundConversion, boundBody, breakLabel, continueLabel);
    }

    private BoundStatement BindForStatement(ForStatementSyntax syntax)
    {
        Scope = Scope?.CreateChild();
        var boundInitializer = (BoundStatement)Bind(syntax.Initializer);
        var expressionBinder = new ExpressionBinder(this);
        var boundCondition = (BoundExpression)expressionBinder.Bind(syntax.Condition);
        var boundConversion = expressionBinder.BindConversion(boundCondition, TypeSymbol.Bool, ImmutableArray<TypeSymbol>.Empty);
        Diagnostics.AddRange(expressionBinder.Diagnostics);
        var boundIncrementor = (BoundExpression)expressionBinder.Bind(syntax.Incrementor);
        var boundBody = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
        Scope = Scope?.Parent!;
        return new BoundForStatement(syntax, boundInitializer, boundConversion, boundIncrementor, boundBody, breakLabel, continueLabel);
    }

    private BoundStatement BindLoopBody(SyntaxNode body, out BoundLabel breakLabel, out BoundLabel continueLabel)
    {
        _labelCounter++;
        breakLabel = new BoundLabel($"break{_labelCounter}");
        continueLabel = new BoundLabel($"continue{_labelCounter}");
        _loopStack.Push(new LoopConditions(breakLabel, continueLabel));
        var boundBody = (BoundStatement)Bind(body);
        _loopStack.Pop();
        return boundBody;
    }

    private BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
    {
        if (_loopStack.Count == 0)
        {
            Diagnostics.ReportInvalidBreakOrContinue(syntax.Location, syntax.BreakKeyword.Text);
            return new BoundErrorStatement(syntax, new SemanticContext(this, syntax, Diagnostics));
        }
        
        var breakLabel = _loopStack.Peek().BreakLabel;
        return new BoundGotoStatement(syntax, breakLabel);
    }

    private BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
    {
        if (_loopStack.Count == 0)
        {
            Diagnostics.ReportInvalidBreakOrContinue(syntax.Location, syntax.ContinueKeyword.Text);
            return new BoundErrorStatement(syntax, new SemanticContext(this, syntax, Diagnostics));
        }
        
        var continueLabel = _loopStack.Peek().ContinueLabel;
        return new BoundGotoStatement(syntax, continueLabel);
    }

    private sealed class LoopConditions
    {
        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
        public LoopConditions(BoundLabel breakLabel, BoundLabel continueLabel)
        {
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }
    }
}