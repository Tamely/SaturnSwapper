namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class SignStatementSyntax : StatementSyntax
{
    public SyntaxToken SignKeyword { get; }
    public SyntaxToken ColonToken { get; }
    public SyntaxToken KeyStringToken { get; }
    public SyntaxToken CommaToken { get; }
    public SyntaxToken ValueStringToken { get; }
    
    public SignStatementSyntax(SyntaxTree syntaxTree, SyntaxToken signKeyword, SyntaxToken colonToken, 
                               SyntaxToken keyStringToken, SyntaxToken commaToken, SyntaxToken valueStringToken) 
        : base(syntaxTree)
    {
        SignKeyword = signKeyword;
        ColonToken = colonToken;
        KeyStringToken = keyStringToken;
        CommaToken = commaToken;
        ValueStringToken = valueStringToken;
    }
}