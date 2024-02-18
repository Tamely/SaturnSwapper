using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundBlockStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
    public ImmutableArray<BoundStatement> Statements { get; }
    
    public BoundBlockStatement(SyntaxNode syntax, ImmutableArray<BoundStatement> statements)
        : base(syntax)
    {
        Statements = statements;
    }
}