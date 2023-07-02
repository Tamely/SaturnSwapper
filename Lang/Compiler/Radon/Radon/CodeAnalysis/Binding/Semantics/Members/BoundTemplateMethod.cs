using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Members;

internal sealed class BoundTemplateMethod : BoundMember
{
    public override BoundNodeKind Kind => BoundNodeKind.TemplateMethod;
    public TemplateMethodSymbol Symbol { get; }
    public ImmutableArray<BoundStatement> Statements { get; }
    public ImmutableArray<LocalVariableSymbol> Locals { get; }
    public BoundTemplateMethod(SyntaxNode syntax, TemplateMethodSymbol symbol, ImmutableArray<BoundStatement> statements, ImmutableArray<LocalVariableSymbol> locals) 
        : base(syntax)
    {
        Symbol = symbol;
        Statements = statements;
        Locals = locals;
    }
}