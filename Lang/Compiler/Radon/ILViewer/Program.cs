using System;
using System.IO;
using Radon.CodeAnalysis.Disassembly;

namespace ILViewer;

public static class Program
{
    public static void Main()
    {
        while (true)
        {
            WriteLine("Enter the path to the compiled Radon file: ", WriteLevel.Info);
            var path = Console.ReadLine();
            if (path == null)
            {
                break;
            }
            
            if (path.StartsWith('"') && path.EndsWith('"'))
            {
                path = path[1..^1];
            }
            
            path = path.Trim();
            if (!File.Exists(path))
            {
                WriteLine($"Invalid path. Received {path}, expected a valid path to a Radon executable", WriteLevel.Error);
                return;
            }
            
            var file = File.ReadAllBytes(path);
            WriteLine($"Read {file.Length} bytes from {path}", WriteLevel.Info);
            var assembly = Disassembler.Disassemble(file);
            var assemblyInfo = new AssemblyInfo(assembly);
            WriteLine("Disassembled successfully", WriteLevel.Info);

            var ilWriter = new ILWriter(assemblyInfo);
            foreach (var token in ilWriter.Tokens)
            {
                WriteToken(token);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }
    }

    private static void WriteToken(ILToken token)
    {
        switch (token.Kind)
        {
            case ILTokenKind.TypeIdentifier:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case ILTokenKind.VariableIdentifier:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                break;
            case ILTokenKind.MethodIdentifier:
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case ILTokenKind.FieldIdentifier:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case ILTokenKind.EnumMemberIdentifier:
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;
            case ILTokenKind.Keyword:
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
            case ILTokenKind.Label:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
            case ILTokenKind.OpCode:
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                break;
            case ILTokenKind.Punctuation:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case ILTokenKind.String:
                Console.ForegroundColor = ConsoleColor.DarkRed;
                break;
            case ILTokenKind.Number:
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                break;
            case ILTokenKind.UnknownValue:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case ILTokenKind.DirectiveKeyword:
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                break;
            case ILTokenKind.Comment:
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                break;
            case ILTokenKind.Trivia:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
            case ILTokenKind.EOF:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (token.Kind == ILTokenKind.Trivia)
        {
            if (token.Text == "\n")
            {
                Console.WriteLine();
            }
            else if (token.Text == "\t")
            {
                Console.Write("\t");
            }
            else
            {
                Console.Write(token.Text);
            }
            
            return;
        }
        
        Console.Write(token.Text);
        Console.ResetColor();
    }
    
    private static void WriteLine(string text, WriteLevel level)
    {
        var color = (ConsoleColor)level;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
