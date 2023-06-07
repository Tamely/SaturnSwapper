using Radon.CodeAnalysis.Binding.Analyzers;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Members;

internal sealed class BoundErrorMember : BoundMember
{
    public override BoundNodeKind Kind => BoundNodeKind.ErrorMember;
    public SemanticContext Context { get; }
    public BoundErrorMember(SyntaxNode syntax, SemanticContext context) 
        : base(syntax)
    {
        Context = context;
    }
}