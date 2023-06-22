using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Members;

internal sealed class BoundField : BoundMember
{
    public override BoundNodeKind Kind => BoundNodeKind.Field;
    public FieldSymbol Symbol { get; }
    public BoundExpression? Initializer { get; }
    public BoundField(SyntaxNode syntax, FieldSymbol symbol, BoundExpression? initializer) 
        : base(syntax)
    {
        Symbol = symbol;
        Initializer = initializer;
    }
}