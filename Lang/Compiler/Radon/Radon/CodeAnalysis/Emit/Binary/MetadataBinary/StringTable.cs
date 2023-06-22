using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct StringTable
{
    public readonly string[] Strings;
    
    public StringTable(string[] strings)
    {
        Strings = strings;
    }
}