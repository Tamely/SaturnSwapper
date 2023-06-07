using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Members;

internal sealed class BoundConstructor : BoundMember
{
    public override BoundNodeKind Kind => BoundNodeKind.Constructor;
    public ConstructorSymbol Symbol { get; }
    public ImmutableArray<BoundStatement> Statements { get; }
    public BoundConstructor(SyntaxNode syntax, ConstructorSymbol symbol, ImmutableArray<BoundStatement> statements) 
        : base(syntax)
    {
        Symbol = symbol;
        Statements = statements;
    }
}