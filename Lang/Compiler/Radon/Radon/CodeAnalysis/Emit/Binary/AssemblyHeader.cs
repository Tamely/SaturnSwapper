using System;
using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct AssemblyHeader
{
    public readonly ulong MagicNumber;
    public readonly Guid Guid;
    public readonly AssemblyFlags Flags;
    public readonly long EncryptionKey;
    public readonly double Version;

    public AssemblyHeader(ulong magicNumber, Guid guid, AssemblyFlags flags, long encryptionKey, double version)
    {
        MagicNumber = magicNumber;
        Guid = guid;
        Flags = flags;
        EncryptionKey = encryptionKey;
        Version = version;
    }
}