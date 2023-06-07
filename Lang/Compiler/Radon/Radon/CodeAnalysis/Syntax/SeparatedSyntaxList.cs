using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Syntax;

public readonly struct SeparatedSyntaxList<TNode> : IEnumerable<TNode>
    where TNode : SyntaxNode
{
    private readonly ImmutableArray<SyntaxNode> _nodesAndSeparators;

    internal SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
    {
        _nodesAndSeparators = nodesAndSeparators;
    }

    public int Count => (_nodesAndSeparators.Length + 1) / 2;

    public TNode this[int index] => (TNode) _nodesAndSeparators[index * 2];

    public SyntaxToken GetSeparator(int index)
    {
        if (index < 0 || index >= Count - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return (SyntaxToken) _nodesAndSeparators[index * 2 + 1];
    }

    public ImmutableArray<SyntaxNode> GetWithSeparators()
    {
        return _nodesAndSeparators;
    }

    public IEnumerator<TNode> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}