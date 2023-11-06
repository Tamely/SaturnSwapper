using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Text;

namespace Radon.Ide.Backend.Core;

public sealed class TextDocument
{
    public SourceText SourceText { get; set; }
    public ImmutableArray<SyntaxToken> Tokens => SyntaxTree.ParseTokens(SourceText, true);
    
    public TextDocument(SourceText sourceText)
    {
        SourceText = sourceText;
    }
}
