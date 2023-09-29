using System;
using Radon.CodeAnalysis.Emit.Binary;
using Radon.Common;
using Assembly = Radon.CodeAnalysis.Emit.Binary.Assembly;
using AssemblyFlags = Radon.CodeAnalysis.Emit.Binary.AssemblyFlags;

namespace Radon.CodeAnalysis.Disassembly;

public static class Disassembler
{
    public static unsafe Assembly Disassemble(byte[] bytes)
    {
        AssemblyHeader header;
        fixed (byte* ptr = bytes)
        {
            header = *(AssemblyHeader*)ptr;
        }
        
        if (header.MagicNumber != 6858187695197072735uL)
        {
            throw new Exception("Invalid magic number");
        }
        
        var flags = header.Flags;
        var encryptionKey = header.EncryptionKey;
        var builder = new BinaryParser(bytes, flags.HasFlag(AssemblyFlags.Encryption), encryptionKey);
        var assembly = builder.Parse(typeof(Assembly));
        return (Assembly)assembly;
    }
}