using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }
    public virtual BoundConstant? ConstantValue => null;
    private protected BoundExpression(SyntaxNode syntax) 
        : base(syntax)
    {
    }
}