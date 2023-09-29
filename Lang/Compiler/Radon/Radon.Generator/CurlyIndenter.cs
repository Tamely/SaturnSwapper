using System;
using System.CodeDom.Compiler;

namespace Radon.Generators;

internal class CurlyIndenter : IDisposable
{
    private readonly IndentedTextWriter _indentedTextWriter;
    
    public CurlyIndenter(IndentedTextWriter indentedTextWriter, string openingLine = "")
    {
        _indentedTextWriter = indentedTextWriter;
        if (!string.IsNullOrWhiteSpace(openingLine))
        {
            indentedTextWriter.WriteLine(openingLine);
        }
        
        indentedTextWriter.WriteLine("{");
        indentedTextWriter.Indent++;
    }
    
    public void Dispose()
    {
        _indentedTextWriter.Indent--;
        _indentedTextWriter.WriteLine("}");
    }
}