using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct Sign
{
    public readonly string Key;
    public readonly string Value;
    
    public Sign(string key, string value)
    {
        Key = key;
        Value = value;
    }
}