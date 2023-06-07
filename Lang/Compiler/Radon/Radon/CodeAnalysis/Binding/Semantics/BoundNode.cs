using Radon.CodeAnalysis.Binding.Analyzers;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics;

internal abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }
    public SyntaxNode Syntax { get; }
    
    private protected BoundNode(SyntaxNode syntax)
    {
        Syntax = syntax;
    }
}

internal sealed class BoundErrorNode : BoundNode
{
    public override BoundNodeKind Kind => BoundNodeKind.ErrorNode;
    public SemanticContext Context { get; }
    public BoundErrorNode(SyntaxNode syntax, SemanticContext context) 
        : base(syntax)
    {
        Context = context;
    }
}
