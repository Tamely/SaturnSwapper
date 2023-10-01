using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundMemberAccessExpression : BoundExpression, ISymbolExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.MemberAccessExpression;
    public override TypeSymbol Type { get; }
    public BoundExpression Expression { get; }
    public MemberSymbol Member { get; }
    public Symbol Symbol => Member;
    
    public BoundMemberAccessExpression(SyntaxNode syntax, BoundExpression expression, MemberSymbol member)
        : base(syntax)
    {
        Expression = expression;
        Member = member;
        Type = member.Type;
    }
}