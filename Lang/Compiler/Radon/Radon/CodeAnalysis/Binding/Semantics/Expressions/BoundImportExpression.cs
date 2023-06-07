using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundImportExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.ImportExpression;
    public BoundExpression Path { get; }
    public override TypeSymbol Type => TypeSymbol.Archive;
    
    public BoundImportExpression(SyntaxNode syntax, BoundExpression path)
        : base(syntax)
    {
        Path = path;
    }
}