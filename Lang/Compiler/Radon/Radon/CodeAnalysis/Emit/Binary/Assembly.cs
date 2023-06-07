using System;
using System.Runtime.InteropServices;
using Radon.CodeAnalysis.Emit.Binary.MetadataBinary;
using Constants = Radon.Utilities.Constants;

namespace Radon.CodeAnalysis.Emit.Binary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct Assembly
{
    public readonly ulong MagicNumber;
    public readonly Guid Guid;
    public readonly AssemblyFlags Flags;
    public readonly long EncryptionKey;
    public readonly double Version;
    public readonly InstructionTable Instructions;
    public readonly Metadata Metadata;
    
    public Assembly(Guid guid, AssemblyFlags flags, long encryptionKey, InstructionTable instructions, Metadata metadata)
    {
        MagicNumber = 6858187695197072735uL;
        Guid = guid;
        Flags = flags;
        EncryptionKey = encryptionKey;
        Version = Constants.RadonVersion;
        Instructions = instructions;
        Metadata = metadata;
    }
}