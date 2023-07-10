using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Radon.CodeAnalysis;

namespace Radon.Utilities;

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

    public static void WritePunctuation(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.DarkGray);
        writer.Write(text);
        writer.ResetColor();
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
            /*else if (isWhitespace)
            {
                // If it's whitespace, set the character to the first non-whitespace character
                character++;
            }*/
            
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

            writer.SetForeground(ConsoleColor.Cyan);
            writer.WriteLine(new string(' ', character - 1) + "^");
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

public static class ObjectExtensions
{
    private static readonly Dictionary<Type, bool> CachedTypes = new();
    
    public static unsafe T Encrypt<T>(this object value, long key)
    {
        if (typeof(T) != value.GetType())
        {
            throw new ArgumentException("The type of the value must be the same as the type of the generic parameter.");
        }
        
        if (value.IsUnmanaged())
        {
            var size = Marshal.SizeOf<T>();
            var bytes = stackalloc byte[size];
            Marshal.StructureToPtr(value, (nint)bytes, true);
            for (var i = 0; i < size; i++)
            {
                bytes[i] ^= (byte)key;
            }

            return Marshal.PtrToStructure<T>((nint)bytes)!;
        }

        return (T)value;
    }
    
    public static T Decrypt<T>(this object value, long key) => value.Encrypt<T>(key);

    public static bool IsUnmanaged(this object obj)
    {
        var type = obj.GetType();
        return IsUnmanaged(type);
    }

    public static bool IsUnmanaged(this Type type)
    {
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (CachedTypes.ContainsKey(type))
        {
            return CachedTypes[type];
        }

        bool result;
        if (type.IsPrimitive || type.IsPointer || type.IsEnum)
        {
            result = true;
        }
        else if (type.IsGenericType || !type.IsValueType)
        {
            result = false;
        }
        else
        {
            result = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .All(f => IsUnmanaged(f.FieldType));
        }
        
        CachedTypes.Add(type, result);
        return result;
    }
}