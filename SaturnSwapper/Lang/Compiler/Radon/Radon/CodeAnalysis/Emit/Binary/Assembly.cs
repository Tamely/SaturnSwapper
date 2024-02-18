using System;
using System.Runtime.InteropServices;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Radon.Common;

namespace Radon.CodeAnalysis.Emit.Binary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Assembly
{
    public readonly AssemblyHeader Header;
    public readonly InstructionTable Instructions;
    public readonly Metadata Metadata;
    
    public Assembly(Guid guid, AssemblyFlags flags, long encryptionKey, InstructionTable instructions, Metadata metadata)
    {
        Header = new AssemblyHeader(6858187695197072735uL, guid, flags, encryptionKey, RadonConstants.RadonVersionNumber);
        Instructions = instructions;
        Metadata = metadata;
    }
}