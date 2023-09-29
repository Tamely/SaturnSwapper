using Radon.CodeAnalysis.Syntax.Nodes.Statements;

namespace Radon.CodeAnalysis.Syntax.Nodes;

public sealed partial class TopLevelStatementCompilationUnitSyntax : CompilationUnitSyntax
{
    public ImmutableSyntaxList<StatementSyntax> Statements { get; }
    
    public TopLevelStatementCompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<StatementSyntax> statements,
                                 SyntaxToken endOfFileToken)
        : base(syntaxTree, endOfFileToken)
    {
        Statements = statements;
    }
}