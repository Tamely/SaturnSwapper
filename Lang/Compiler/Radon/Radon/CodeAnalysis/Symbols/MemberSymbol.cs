using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public abstract class MemberSymbol : Symbol
{
    public static readonly MemberSymbol Error = new ErrorMemberSymbol();
    public abstract TypeSymbol ParentType { get; }
    public abstract override TypeSymbol Type { get; }
    public abstract override ImmutableArray<SyntaxKind> Modifiers { get; }
    public sealed override bool HasType => true;
    
    public abstract MemberSymbol WithType(TypeSymbol type);
    public abstract MemberSymbol WithParentType(TypeSymbol parentType);
}