using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Types;

internal sealed class BoundErrorType : BoundType
{
    public override BoundNodeKind Kind => BoundNodeKind.ErrorType;
    public SemanticContext Context { get; }
    public BoundErrorType(SyntaxNode syntax, SemanticContext context) 
        : base(syntax, TypeSymbol.Error)
    {
        Context = context;
    }
}