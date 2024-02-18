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
    public ImmutableArray<LocalVariableSymbol> Locals { get; }
    public BoundConstructor(SyntaxNode syntax, ConstructorSymbol symbol, ImmutableArray<BoundStatement> statements, 
                            ImmutableArray<LocalVariableSymbol> locals) 
        : base(syntax)
    {
        Symbol = symbol;
        Statements = statements;
        Locals = locals;
    }
    
    public override string ToString()
    {
        return Symbol.ToString();
    }
}