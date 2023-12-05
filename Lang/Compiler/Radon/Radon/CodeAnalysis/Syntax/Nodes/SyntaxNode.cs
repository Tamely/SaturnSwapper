using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Syntax.Nodes;

public abstract class SyntaxNode
{
    public static SyntaxNode Empty => new EmptySyntaxNode();
    public abstract SyntaxKind Kind { get; }
    public abstract string SyntaxName { get; }

    public virtual TextSpan Span
    {
        get
        {
            if (this is EmptySyntaxNode)
            {
                return TextSpan.Empty;
            }
            
            var first = GetChildren().First().Span;
            var last = GetChildren().Last().Span;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }
    
    public virtual int Position => Span.Start;
    public virtual string Text => SyntaxTree.Text.ToString(Span);
    
    public TextLocation Location => new(SyntaxTree.Text, Span);
    public SyntaxTree SyntaxTree { get; }
    public SyntaxNode Parent => SyntaxTree.GetParent(this);

    protected SyntaxNode(SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
    }
    
    public abstract IEnumerable<SyntaxNode> GetChildren();

    public IEnumerable<SyntaxNode> AncestorsAndSelf()
    {
        var current = (SyntaxNode?)this;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }
    
    public IEnumerable<SyntaxNode> Ancestors()
    {
        return AncestorsAndSelf().Skip(1);
    }
    
    public IEnumerable<SyntaxNode> DescendantsAndSelf()
    {
        var stack = new Stack<SyntaxNode>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;
            foreach (var child in current.GetChildren().Reverse())
            {
                stack.Push(child);
            }
        }
    }
    
    public IEnumerable<SyntaxNode> Descendants()
    {
        return DescendantsAndSelf().Skip(1);
    }
    
    public SyntaxToken GetLastToken()
    {
        if (this is SyntaxToken token)
        {
            return token;
        }
        
        var lastChild = GetChildren().LastOrDefault();
        if (lastChild == null)
        {
            throw new InvalidOperationException("Syntax node has no tokens.");
        }
        
        return lastChild.GetLastToken();
    }
    
    public ImmutableSyntaxList<SyntaxToken> Dissolve()
    {
        // The concept of "dissolving" a syntax node is to take the node, and break it down into a list of tokens.
        var builder = ImmutableArray.CreateBuilder<SyntaxToken>();
        Dissolve(builder);
        return new ImmutableSyntaxList<SyntaxToken>(builder.ToImmutable());
    }
    
    private void Dissolve(ImmutableArray<SyntaxToken>.Builder builder)
    {
        foreach (var child in GetChildren())
        {
            if (child is SyntaxToken token)
            {
                builder.Add(token);
            }
            else
            {
                child.Dissolve(builder);
            }
        }
    }

    public void WriteTo(TextWriter writer)
    {
        PrettyPrint(writer, this);
    }
    
    private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true)
    {
        var isToConsole = writer == Console.Out;
        var token = node as SyntaxToken;
        if (token != null)
        {
            foreach (var trivia in token.LeadingTrivia)
            {
                if (isToConsole)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }

                writer.Write(indent);
                writer.Write("├──");

                if (isToConsole)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }

                writer.Write($"L: {trivia.Kind}");
                if (trivia.Kind.HasAttribute(SKAttributes.Comment))
                {
                    Console.Write(' ');
                    Console.WriteLine(trivia.Text);
                }
                else
                {
                    Console.WriteLine();
                }
            }
        }

        var hasTrailingTrivia = token != null && token.TrailingTrivia.Any();
        var tokenMarker = !hasTrailingTrivia && isLast ? "└──" : "├──";
        if (isToConsole)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        writer.Write(indent);
        writer.Write(tokenMarker);
        if (isToConsole)
        {
            Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;
        }

        writer.Write(node.Kind);
        if (token is { Value: { } })
        {
            writer.Write(" ");
            var previousColor = Console.ForegroundColor;
            if (isToConsole)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            
            writer.Write(token.Value);
            
            if (isToConsole)
            {
                Console.ForegroundColor = previousColor;
            }
        }
        else if (token != null && token.Kind == SyntaxKind.IdentifierToken)
        {
            writer.Write(" ");
            var savedColor = Console.ForegroundColor;
            if (isToConsole)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }
            writer.Write(token.Text);
            
            if (isToConsole)
            {
                Console.ForegroundColor = savedColor;
            }
        }

        if (isToConsole)
        {
            Console.ResetColor();
        }

        writer.WriteLine();
        if (token != null)
        {
            foreach (var trivia in token.TrailingTrivia)
            {
                var isLastTrailingTrivia = trivia == token.TrailingTrivia.Last();
                var triviaMarker = isLast && isLastTrailingTrivia ? "└──" : "├──";

                if (isToConsole)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }

                writer.Write(indent);
                writer.Write(triviaMarker);

                if (isToConsole)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }

                writer.WriteLine($"T: {trivia.Kind}");
            }
        }

        indent += isLast ? "   " : "│  ";
        var lastChild = node.GetChildren().LastOrDefault();
        foreach (var child in node.GetChildren())
        {
            PrettyPrint(writer, child, indent, child == lastChild);
        }
    }
    
    public override string ToString()
    {
        return Text;
    }
}