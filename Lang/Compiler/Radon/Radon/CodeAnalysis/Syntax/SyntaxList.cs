using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Syntax;

public readonly struct ImmutableSyntaxList<TNode> : IEnumerable<TNode>
    where TNode : SyntaxNode
{
    public static readonly ImmutableSyntaxList<TNode> Empty = new(ImmutableArray<TNode>.Empty);
    private readonly ImmutableArray<TNode> _nodes;

    internal ImmutableSyntaxList(ImmutableArray<TNode> nodes)
    {
        _nodes = nodes;
    }

    public int Count => _nodes.Length;

    public TNode this[int index] => _nodes[index];

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

    public ImmutableSyntaxList<T> As<T>()
        where T : SyntaxNode
    {
        var builder = ImmutableArray.CreateBuilder<T>(Count);
        try
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i] is not T converted)
                {
                    return default;
                }
                
                builder.Add(converted);
            }
        }
        catch (InvalidCastException)
        {
            return default;
        }
        
        return new ImmutableSyntaxList<T>(builder.MoveToImmutable());
    }
}
