using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Directives;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
    private Dictionary<SyntaxNode, SyntaxNode>? _parentMap;
    private delegate void ParseHandler(SyntaxTree syntaxTree,
                                       IEnumerable<IncludePath> includedFiles,
                                       out CompilationUnitSyntax root,
                                       out ImmutableArray<SyntaxTree> included,
                                       out ImmutableArray<Diagnostic> diagnostics);

    public SourceText Text { get; }
    public TextLocation Location => new(Text, TextSpan.FromBounds(0, Text.Length));
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public ImmutableArray<SyntaxTree> Included { get; }
    public CompilationUnitSyntax Root { get; }
    public ImmutableArray<SyntaxToken> Tokens => ParseTokens(Text);

    private SyntaxTree(SourceText text, ParseHandler handler, IEnumerable<IncludePath>? includedFiles = null)
    {
        _parentMap = null;
        Text = text;
        includedFiles ??= Enumerable.Empty<IncludePath>();
        try
        {
            handler(this, includedFiles, out var root, out var included, out var diagnostics);
            Root = root;
            Included = included;
            Diagnostics = diagnostics;
        }
        catch (Exception e)
        {
            Root = new TopLevelStatementCompilationUnitSyntax(this, ImmutableSyntaxList<StatementSyntax>.Empty,
                                             SyntaxKind.EndOfFileToken.ManifestToken(this));
            var diagnosticBag = new DiagnosticBag();
            diagnosticBag.ReportInternalCompilerError(e);
            Included = ImmutableArray<SyntaxTree>.Empty;
            Diagnostics = diagnosticBag.ToImmutableArray();
        }
    }

    public static SyntaxTree Load(string fileName)
    {
        var text = File.ReadAllText(fileName);
        var sourceText = SourceText.From(text, fileName);
        return Parse(sourceText);
    }
    
    public static SyntaxTree Load(string fileName, IEnumerable<IncludePath> included)
    {
        var text = File.ReadAllText(fileName);
        var sourceText = SourceText.From(text, fileName);
        return Parse(sourceText, included);
    }

    private static void Parse(SyntaxTree tree, IEnumerable<IncludePath> includedFiles, out CompilationUnitSyntax root, 
                              out ImmutableArray<SyntaxTree> included, out ImmutableArray<Diagnostic> diagnostics)
    {
        try
        {
            var parser = new Parser(tree);
            var directives = parser.ParseDirectives();
            var parserDiags = parser.Diagnostics;
            var includePaths = new List<IncludePath>
            {
                new(tree.Text.FileName, null)
            };
            
            foreach (var directive in directives)
            {
                if (directive.Kind == SyntaxKind.IncludeDirective)
                {
                    var includeDirective = (IncludeDirectiveSyntax)directive;
                    var includePath = includeDirective.Path;
                    var workingDirectory = Path.GetDirectoryName(tree.Text.FileName);
                    if (workingDirectory is null)
                    {
                        throw new InvalidOperationException("Working directory is null.");
                    }
                    
                    var path = ParsePath(includePath.Text, workingDirectory, parserDiags);
                    var ip = new IncludePath(path, includeDirective);
                    if (includePaths.Any(x => x.Path == ip.Path))
                    {
                        parserDiags.ReportDuplicateInclude(includeDirective.Location, includePath.Text);
                    }
                    else
                    {
                        includePaths.Add(ip);
                    }
                }
            }
            
            var includedTrees = new List<SyntaxTree>();
            foreach (var path in includePaths)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                if (includedFiles.Any(x => x.Path == path.Path) &&
                    path.Path != tree.Text.FileName)
                {
                    parserDiags.ReportCircularInclude(
                        // We know it's a circular include because it's not an include directive.
                        path.IncludeDirective?.Location ?? TextLocation.Empty,
                        path.Path
                        );

                    continue;
                }
                
                if (File.Exists(path.Path) &&
                    path.Path != tree.Text.FileName)
                {
                    var includedTree = Load(path.Path, includePaths);
                    includedTrees.Add(includedTree);
                    includedTrees.AddRange(includedTree.Included);
                    parserDiags.AddRange(includedTree.Diagnostics);
                }
                else if (path.IncludeDirective is not null)
                {
                    parserDiags.ReportIncludePathDoesNotExist(path.IncludeDirective.Location, path.Path);
                }
            }

            included = includedTrees.ToImmutableArray();
            root = parser.ParseCompilationUnit(directives);
            diagnostics = parserDiags.ToImmutableArray();
        }
        catch (Exception e)
        {
            included = ImmutableArray<SyntaxTree>.Empty;
            root = new TopLevelStatementCompilationUnitSyntax(tree, ImmutableSyntaxList<StatementSyntax>.Empty,
                                             SyntaxKind.EndOfFileToken.ManifestToken(tree));
            var diagnosticBag = new DiagnosticBag();
            diagnosticBag.ReportInternalCompilerError(e);
            diagnostics = diagnosticBag.ToImmutableArray();
        }
    }
    
    private static SyntaxTree Parse(SourceText text, IEnumerable<IncludePath> included)
    {
        return new SyntaxTree(text, Parse, included);
    }
    
    public static Task<SyntaxTree> ParseAsync(string text)
    {
        return ParseAsync(SourceText.From(text));
    }
    
    public static async Task<SyntaxTree> ParseAsync(SourceText text)
    {
        return await Task.Run(() => Parse(text));
    }
    
    public static SyntaxTree Parse(SourceText text)
    {
        return new SyntaxTree(text, Parse);
    }
    
    public static SyntaxTree Parse(string text)
    {
        return Parse(SourceText.From(text));
    }
    
    public static ImmutableArray<SyntaxToken> ParseTokens(string text, bool includeEndOfFile = true)
    {
        return ParseTokens(SourceText.From(text), includeEndOfFile);
    }
    
    public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics, bool includeEndOfFile = false)
    {
        var sourceText = SourceText.From(text);
        return ParseTokens(sourceText, out diagnostics, includeEndOfFile);
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, bool includeEndOfFile = false)
    {
        return ParseTokens(text, out _, includeEndOfFile);
    }
    
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, out ImmutableArray<Diagnostic> diagnostics, bool includeEndOfFile = false)
    {
        var tokens = new List<SyntaxToken>();

        // ReSharper disable once LocalFunctionHidesMethod
        void ParseTokens(SyntaxTree st, IEnumerable<IncludePath>? includedFiles, out CompilationUnitSyntax root, 
                         out ImmutableArray<SyntaxTree> syntaxTrees, out ImmutableArray<Diagnostic> diags)
        {
            syntaxTrees = ImmutableArray<SyntaxTree>.Empty;
            var l = new Lexer(st);
            while (true)
            {
                var token = l.Lex();

                if (token.Kind != SyntaxKind.EndOfFileToken || includeEndOfFile)
                {
                    tokens.Add(token);
                }

                if (token.Kind == SyntaxKind.EndOfFileToken)
                {
                    root = new TopLevelStatementCompilationUnitSyntax(st, ImmutableSyntaxList<StatementSyntax>.Empty, token);
                    break;
                }
            }

            diags = l.Diagnostics.ToImmutableArray();
        }

        var syntaxTree = new SyntaxTree(text, ParseTokens);
        diagnostics = syntaxTree.Diagnostics;
        return tokens.ToImmutableArray();
    }
    
    internal SyntaxNode GetParent(SyntaxNode node)
    {
        if (_parentMap == null)
        {
            var parentMap = CreateParentMap(Root);
            Interlocked.CompareExchange(ref _parentMap, parentMap, null); // thread safe
        }

        return _parentMap[node];
    }
    
    private static Dictionary<SyntaxNode, SyntaxNode> CreateParentMap(SyntaxNode root)
    {
        var result = new Dictionary<SyntaxNode, SyntaxNode> { { root, SyntaxNode.Empty } };
        CreateParentMap(result, root);
        return result;
    }
    
    private static void CreateParentMap(IDictionary<SyntaxNode, SyntaxNode> result, SyntaxNode node)
    {
        foreach (var child in node.GetChildren())
        {
            result.Add(child, node);
            CreateParentMap(result, child);
        }
    }
    
    private static string ParsePath(string text, string workingDirectory, DiagnosticBag diagnostics)
    {
        var path = text;
        if (path.StartsWith("\"") && path.EndsWith("\""))
        {
            path = path[1..^1];
        }

        var currentDirectory = workingDirectory;
        var pathParts = path.Split('/');
        foreach (var part in pathParts)
        {
            if (part == "..")
            {
                var parent = Directory.GetParent(currentDirectory);
                if (parent == null)
                {
                    diagnostics.ReportCannotGoUpDirectory();
                    return string.Empty;
                }

                currentDirectory = parent.FullName;
            }
            else
            {
                currentDirectory = Path.Combine(currentDirectory, part);
            }
        }
        
        return currentDirectory;
    }
    
    public sealed class IncludePath
    {
        public string Path { get; }
        public IncludeDirectiveSyntax? IncludeDirective { get; }
        public IncludePath(string path, IncludeDirectiveSyntax? includeDirective)
        {
            Path = path;
            IncludeDirective = includeDirective;
        }
    }
}
