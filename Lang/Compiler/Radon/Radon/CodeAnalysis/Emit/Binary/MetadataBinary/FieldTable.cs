using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct FieldTable
{
    public readonly Field[] Fields;
    
    public FieldTable(Field[] fields)
    {
        Fields = fields;
    }
}