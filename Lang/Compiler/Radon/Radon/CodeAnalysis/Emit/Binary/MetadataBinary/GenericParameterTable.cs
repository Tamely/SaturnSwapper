using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct GenericParameterTable
{
    public readonly GenericParameter[] GenericParameters;
    
    public GenericParameterTable(GenericParameter[] genericParameters)
    {
        GenericParameters = genericParameters;
    }
}