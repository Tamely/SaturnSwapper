using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Members;

internal sealed class BoundMethod : BoundMember
{
    public override BoundNodeKind Kind => BoundNodeKind.Method;
    public MethodSymbol Symbol { get; }
    public ImmutableArray<BoundStatement> Statements { get; }
    public ImmutableArray<LocalVariableSymbol> Locals { get; }
    public BoundMethod(SyntaxNode syntax, MethodSymbol symbol, ImmutableArray<BoundStatement> statements, ImmutableArray<LocalVariableSymbol> locals) 
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