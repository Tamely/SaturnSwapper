using Radon.CodeAnalysis.Binding.Analyzers;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundErrorStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ErrorStatement;
    public SemanticContext Context { get; }
    public BoundErrorStatement(SyntaxNode syntax, SemanticContext context) 
        : base(syntax)
    {
        Context = context;
    }
}