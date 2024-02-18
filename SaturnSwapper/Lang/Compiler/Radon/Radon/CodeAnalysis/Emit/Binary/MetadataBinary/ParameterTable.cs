using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct ParameterTable
{
    public readonly Parameter[] Parameters;
    
    public ParameterTable(Parameter[] parameters)
    {
        Parameters = parameters;
    }
}