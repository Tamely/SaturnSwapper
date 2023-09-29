using Radon.CodeAnalysis.Binding.Binders;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Binding;

internal sealed class SemanticContext
{
    public static readonly SemanticContext Empty = new(TextLocation.Empty, null, null, new DiagnosticBag(true));
    
    public SourceText Text { get; }
    public TextLocation Location { get; }
    public Binder? Binder { get; }
    public SyntaxNode? Node { get; }
    public SyntaxTree? SyntaxTree { get; }
    public DiagnosticBag Diagnostics { get; }
    public object? Tag { get; set; }
    
    public SemanticContext(TextLocation location, Binder? binder, SyntaxNode? node, DiagnosticBag diagnostics)
    {
        Text = location.Text;
        Location = location;
        Binder = binder;
        Node = node;
        SyntaxTree = node?.SyntaxTree;
        Diagnostics = diagnostics;
    }
    
    public SemanticContext(Binder binder, SyntaxNode node, DiagnosticBag diagnostics)
        : this(node.Location, binder, node, diagnostics)
    {
    }
    
    public static SemanticContext CreateEmpty(Binder binder, DiagnosticBag diagnostics)
    {
        return new SemanticContext(TextLocation.Empty, binder, null, diagnostics);
    }
    
    public T? GetBinder<T>() 
        where T : Binder
    {
        return (T?)Binder;
    }
}