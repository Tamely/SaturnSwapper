using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;

namespace Radon.CodeAnalysis.Syntax.Nodes;

public sealed partial class CodeCompilationUnitSyntax : CompilationUnitSyntax
{
    public ImmutableSyntaxList<TypeDeclarationSyntax> DeclaredTypes { get; }

    public CodeCompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<TypeDeclarationSyntax> declaredTypes, 
                               SyntaxToken endOfFileToken)
        : base(syntaxTree, endOfFileToken)
    {
        DeclaredTypes = declaredTypes;
    }
}