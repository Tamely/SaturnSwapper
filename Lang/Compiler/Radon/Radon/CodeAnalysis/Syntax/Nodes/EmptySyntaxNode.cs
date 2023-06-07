using System.Collections.Generic;
using System.Linq;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Syntax.Nodes;

public sealed class EmptySyntaxNode : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.Empty;
    public EmptySyntaxNode() 
        : base(Syntax.SyntaxTree.Parse(SourceText.Empty))
    {
    }
    
    public override IEnumerable<SyntaxNode> GetChildren() => Enumerable.Empty<SyntaxNode>();
}