using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundVariableDeclarationStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;
    public LocalVariableSymbol Variable { get; }
    public BoundExpression? Initializer { get; }
    
    public BoundVariableDeclarationStatement(SyntaxNode syntax, LocalVariableSymbol variable, BoundExpression? initializer)
        : base(syntax)
    {
        Variable = variable;
        Initializer = initializer;
    }
}