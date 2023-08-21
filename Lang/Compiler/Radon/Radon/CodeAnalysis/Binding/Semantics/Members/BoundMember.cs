using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Members;

internal abstract class BoundMember : BoundNode
{
    protected BoundMember(SyntaxNode syntax) 
        : base(syntax)
    {
    }

    public abstract override string ToString();
}