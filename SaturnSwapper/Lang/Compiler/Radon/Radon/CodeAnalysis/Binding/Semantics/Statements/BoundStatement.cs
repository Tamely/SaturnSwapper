using System;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal abstract class BoundStatement : BoundNode
{
    protected BoundStatement(SyntaxNode syntax) 
        : base(syntax)
    {
    }
}