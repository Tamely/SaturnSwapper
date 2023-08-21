using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Radon.Generators;

public abstract class RadonGenerator : ISourceGenerator
{
    public abstract void Initialize(GeneratorInitializationContext context);
    public abstract void Execute(GeneratorExecutionContext context);
    
    protected static IEnumerable<INamedTypeSymbol> GetAllTypes(IAssemblySymbol symbol)
    {
        var result = new List<INamedTypeSymbol>();
        GetAllTypes(result, symbol.GlobalNamespace);
        result.Sort((x, y) => string.Compare(x.MetadataName, y.MetadataName, StringComparison.Ordinal));
        return result;
    }

    protected static void GetAllTypes(ICollection<INamedTypeSymbol> result, INamespaceOrTypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol type)
        {
            result.Add(type);
        }

        foreach (var child in symbol.GetMembers())
        {
            if (child is INamespaceOrTypeSymbol nsChild)
            {
                GetAllTypes(result, nsChild);
            }
        }
    }

    protected static bool IsDerivedFrom(ITypeSymbol type, INamedTypeSymbol baseType)
    {
        var current = type;

        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    protected static bool IsPartial(ISymbol type)
    {
        foreach (var declaration in type.DeclaringSyntaxReferences)
        {
            var syntax = declaration.GetSyntax();
            if (syntax is TypeDeclarationSyntax typeDeclaration)
            {
                foreach (var modifer in typeDeclaration.Modifiers)
                {
                    if (modifer.ValueText == "partial")
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}

[Generator]
public sealed class SyntaxNodeGetChildrenGenerator : RadonGenerator
{
    public override void Initialize(GeneratorInitializationContext context)
    {
    }

    public override void Execute(GeneratorExecutionContext context)
    {
        SourceText sourceText;

        var compilation = (CSharpCompilation)context.Compilation;

        var immutableArrayType = compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");
        var separatedSyntaxListType =
            compilation.GetTypeByMetadataName("Radon.CodeAnalysis.Syntax.SeparatedSyntaxList`1");
        var syntaxListType = compilation.GetTypeByMetadataName("Radon.CodeAnalysis.Syntax.ImmutableSyntaxList`1");
        var syntaxNodeType = compilation.GetTypeByMetadataName("Radon.CodeAnalysis.Syntax.Nodes.SyntaxNode");
        if (immutableArrayType == null || separatedSyntaxListType == null || syntaxNodeType == null ||
            syntaxListType == null)
        {
            return;
        }

        var types = GetAllTypes(compilation.Assembly);
        var syntaxNodeTypes = types.Where(t => !t.IsAbstract && IsPartial(t) && IsDerivedFrom(t, syntaxNodeType));
        var namedTypeSymbols = syntaxNodeTypes as INamedTypeSymbol[] ?? syntaxNodeTypes.ToArray();
        var namespaces = new List<INamespaceSymbol>();
        foreach (var type in namedTypeSymbols)
        {
            if (!namespaces.Contains(type.ContainingNamespace))
            {
                namespaces.Add(type.ContainingNamespace);
            }
        }

        const string indentString = "    ";
        using (var stringWriter = new StringWriter())
        {
            using (var indentedTextWriter = new IndentedTextWriter(stringWriter, indentString))
            {
                indentedTextWriter.WriteLine("using System;");
                indentedTextWriter.WriteLine("using System.Collections.Generic;");
                indentedTextWriter.WriteLine("using System.Collections.Immutable;");
                foreach (var ns in namespaces)
                {
                    indentedTextWriter.WriteLine($"using {ns};");
                }
                
                indentedTextWriter.WriteLine();
                foreach (var type in namedTypeSymbols)
                {
                    // Get all properties that return a SyntaxNode or a SyntaxNode-derived type
                    using (var namespaceCurly = new CurlyIndenter(indentedTextWriter, $"namespace {type.ContainingNamespace}"))
                    {
                        using (var classCurly = new CurlyIndenter(indentedTextWriter, $"public partial class {type.Name}"))
                        {
                            var typeName = type.Name.Replace("Syntax", string.Empty);
                            var sb = new StringBuilder();
                            // What we want: IfStatement -> If Statement
                            for (var i = 0; i < typeName.Length; i++)
                            {
                                var c = typeName[i];
                                if (char.IsUpper(c) && i > 0)
                                {
                                    sb.Append(' ');
                                }

                                sb.Append(c);
                            }
                            
                            var name = sb.ToString();
                            indentedTextWriter.WriteLine($"public override string SyntaxName => \"{name}\";");
                            indentedTextWriter.WriteLine($"public override SyntaxKind Kind => SyntaxKind.{typeName};");
                            
                            if (type.BaseType != null && type.BaseType.Name == "ExpressionSyntax")
                            {
                                indentedTextWriter.WriteLine($"public {type.Name}(SyntaxTree syntaxTree)");
                                indentedTextWriter.WriteLine($"\t: base(syntaxTree)");
                                indentedTextWriter.WriteLine("{");
                                indentedTextWriter.WriteLine("}");
                                indentedTextWriter.WriteLine();
                            }

                            using (var getChildCurly =
                                   new CurlyIndenter(indentedTextWriter,
                                       "public override IEnumerable<SyntaxNode> GetChildren()"))
                            {
                                var properties = type.GetMembers().OfType<IPropertySymbol>();
                                var propertySymbols = properties as IPropertySymbol[] ?? properties.ToArray();
                                if (propertySymbols.Length == 1)
                                {
                                    indentedTextWriter.WriteLine("yield break;");
                                    continue;
                                }

                                foreach (var property in propertySymbols)
                                {
                                    if (property.Type is INamedTypeSymbol propertyType)
                                    {
                                        if (IsDerivedFrom(propertyType, syntaxNodeType))
                                        {
                                            var canBeNull = property.NullableAnnotation == NullableAnnotation.Annotated;
                                            if (canBeNull)
                                            {
                                                indentedTextWriter.WriteLine($"if ({property.Name} != null)");
                                                indentedTextWriter.WriteLine("{");
                                                indentedTextWriter.Indent++;
                                            }

                                            indentedTextWriter.WriteLine($"yield return {property.Name};");

                                            if (canBeNull)
                                            {
                                                indentedTextWriter.Indent--;
                                                indentedTextWriter.WriteLine("}");
                                                indentedTextWriter.WriteLine();
                                            }
                                        }
                                        else
                                        {
                                            if (propertyType.TypeArguments.Length == 1 &&
                                                IsDerivedFrom(propertyType.TypeArguments[0], syntaxNodeType) &&
                                                SymbolEqualityComparer.Default.Equals(
                                                    propertyType.OriginalDefinition, immutableArrayType))
                                            {
                                                using (var foreachCurly = new CurlyIndenter(
                                                           indentedTextWriter, $"foreach (var child in {property.Name})"))
                                                {
                                                    indentedTextWriter.WriteLine("yield return child;");
                                                }

                                                indentedTextWriter.WriteLine();
                                            }
                                            else if (SymbolEqualityComparer.Default.Equals(
                                                         propertyType.OriginalDefinition, separatedSyntaxListType) &&
                                                     IsDerivedFrom(propertyType.TypeArguments[0], syntaxNodeType))
                                            {
                                                using (var foreachCurly = new CurlyIndenter(
                                                           indentedTextWriter,
                                                           $"foreach (var child in {property.Name}.GetWithSeparators())"))
                                                {
                                                    indentedTextWriter.WriteLine("yield return child;");
                                                }

                                                indentedTextWriter.WriteLine();
                                            }
                                            else if (SymbolEqualityComparer.Default.Equals(
                                                         propertyType.OriginalDefinition, syntaxListType) &&
                                                     IsDerivedFrom(propertyType.TypeArguments[0], syntaxNodeType))
                                            {
                                                using (var foreachCurly = new CurlyIndenter(
                                                           indentedTextWriter, $"foreach (var child in {property.Name})"))
                                                {
                                                    indentedTextWriter.WriteLine("yield return child;");
                                                }

                                                indentedTextWriter.WriteLine();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    indentedTextWriter.WriteLine();
                }

                indentedTextWriter.Flush();
                stringWriter.Flush();

                sourceText = SourceText.From(stringWriter.ToString(), Encoding.UTF8);
            }
        }

        const string fileName = "SyntaxNode_GetChildren.g.cs";
        var syntaxNodeFilePath = syntaxNodeType.DeclaringSyntaxReferences.First().SyntaxTree.FilePath;
        var syntaxDirectory = Path.GetDirectoryName(syntaxNodeFilePath);
        var filePath = Path.Combine(syntaxDirectory ?? string.Empty, fileName);

        if (File.Exists(filePath))
        {
            var fileText = File.ReadAllText(filePath);
            var sourceFileText = SourceText.From(fileText, Encoding.UTF8);
            if (sourceText.ContentEquals(sourceFileText))
            {
                return;
            }
        }

        using (var writer = new StreamWriter(filePath))
        {
            sourceText.Write(writer);
        }
    }
}
