using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Radon.CodeAnalysis;

namespace Radon.Common;

public static class TextWriterExtensions
{
    private static bool IsConsole(this TextWriter writer)
    {
        if (writer == Console.Out)
        {
            return !Console.IsOutputRedirected;
        }

        if (writer == Console.Error)
        {
            return !Console.IsErrorRedirected && !Console.IsOutputRedirected; // Color codes are always output to Console.Out
        }

        if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole())
        {
            return true;
        }

        return false;
    }

    private static void SetForeground(this TextWriter writer, ConsoleColor color)
    {
        if (writer.IsConsole())
        {
            Console.ForegroundColor = color;
        }
    }

    private static void ResetColor(this TextWriter writer)
    {
        if (writer.IsConsole())
        {
            Console.ResetColor();
        }
    }
    
    public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diagnostics)
    {
        var diagEnumerable = diagnostics as Diagnostic[] ?? diagnostics.ToArray();
        foreach (var diagnostic in diagEnumerable.OrderBy(d => d.Location.FileName)
                     .ThenBy(d => d.Location.Span.Start)
                     .ThenBy(d => d.Location.Span.Length))
        {
            var messageColor = diagnostic.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
            var locationColor = ConsoleColor.DarkGray;

            var lineIndex = diagnostic.Location.Text.GetLineIndex(diagnostic.Location.Span.Start);
            // Get the text of the span
            var text = diagnostic.Location.Text;
            var errorText = text.ToString(diagnostic.Location.Span);
            // Check if it's a newline \n or \r\n
            
            var line = diagnostic.Location.Text.Lines[lineIndex];
            //var isWhitespace = string.IsNullOrWhiteSpace(errorText);
            var character = diagnostic.Location.Span.Start - line.Start + 1;
            if (errorText is "\n" or "\r\n")
            {
                // If it is, set the character to 1 past the end of the line
                character = line.Length + 1;
            }
            
            // If the position is the end of the file, set the character to 1 past the end of the line
            
            if (diagnostic.Location.Span.Start == text.Length - 1)
            {
                character = line.Length + 1;
            }

            writer.SetForeground(locationColor);
            writer.Write($"({diagnostic.Location.FileName}, {lineIndex + 1}, {character}): ");
            writer.ResetColor();

            writer.SetForeground(messageColor);
            writer.Write($"RADON{(int)diagnostic.Code}: {diagnostic.Message}");
            writer.ResetColor();

            writer.WriteLine();
            writer.SetForeground(locationColor);
            writer.Write('\t');
            writer.WriteLine(line.ToString());
            writer.Write('\t');

            writer.SetForeground(ConsoleColor.Red);
            writer.Write(new string(' ', character - 1));
            writer.WriteLine(new string('^', diagnostic.Location.Span.Length));
            writer.ResetColor();
            
#if DEBUG
            writer.WriteLine();
            writer.SetForeground(ConsoleColor.Blue);
            writer.WriteLine(diagnostic.SourceMethod);
            writer.WriteLine();
            writer.ResetColor();
#endif
        }
        
        writer.WriteLine();
    }
}