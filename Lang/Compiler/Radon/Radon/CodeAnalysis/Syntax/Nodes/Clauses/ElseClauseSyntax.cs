using Radon.CodeAnalysis.Syntax.Nodes.Statements;

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class ElseClauseSyntax : SyntaxNode
{
    public SyntaxToken ElseKeyword { get; }
    public StatementSyntax ElseStatement { get; }
    
    public ElseClauseSyntax(SyntaxTree syntaxTree, SyntaxToken elseKeyword, StatementSyntax elseStatement)
        : base(syntaxTree)
    {
        ElseKeyword = elseKeyword;
        ElseStatement = elseStatement;
    }
}