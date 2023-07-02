#define PARSE_ONLY
//#undef PARSE_ONLY

using Radon.CodeAnalysis;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Text;
using Radon.Utilities;

namespace Radon.Repl;

public static class Program
{
    public static void Main()
    {
        while (true)
        {
            var text = Console.ReadLine();
            if (text == null)
            {
                break;
            }
            
            if (!File.Exists(text))
            {
                Console.WriteLine($"File '{text}' does not exist.");
                continue;
            }
            
            Log("Reading source file...", ConsoleColor.Cyan);
            var sourceText = SourceText.From(File.ReadAllText(text), text);
            Log("Parsing source file...", ConsoleColor.Cyan);
            var syntaxTree = SyntaxTree.Parse(sourceText);
            Log("Generating compilation...", ConsoleColor.Cyan);
            var compilation = new Compilation(syntaxTree);
            var diagnostics = compilation.Diagnostics;
            if (diagnostics.Any())
            {
                Log("Diagnostics were found...", ConsoleColor.Red);
                Console.WriteLine();
                Console.Out.WriteDiagnostics(diagnostics);
            }
            else
            {
                Log("No diagnostics were found...", ConsoleColor.Green);
                var root = syntaxTree.Root;
                
#if DEBUG
                root.WriteTo(Console.Out);
                
                var included = syntaxTree.Included;
                foreach (var include in included)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    include.Root.WriteTo(Console.Out);
                }
#endif
#if PARSE_ONLY
                continue;
#endif

                Log("Compiling...", ConsoleColor.Cyan);
                var bytes = compilation.Compile(out diagnostics);
                if (bytes == null)
                {
                    Log("Compilation failed...", ConsoleColor.Red);
                    Console.Out.WriteDiagnostics(diagnostics);
                    continue;
                }
                
                Log("Compilation succeeded...", ConsoleColor.Green);
                Log($"Writing to {sourceText.FileName}.csp...", ConsoleColor.Cyan);
                File.WriteAllBytes(sourceText.FileName + ".csp", bytes);
                Log("Done!", ConsoleColor.Green);
                
            }
        }
    }

    public static void Log(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
