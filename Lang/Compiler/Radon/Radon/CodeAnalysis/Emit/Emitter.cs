using System.Linq;
using Assembly = Radon.CodeAnalysis.Emit.Binary.Assembly;
using AssemblyFlags = Radon.CodeAnalysis.Emit.Binary.AssemblyFlags;

namespace Radon.CodeAnalysis.Emit;

internal sealed class Emitter
{
    private readonly Assembly _assembly;
    public Emitter(Assembly assembly)
    {
        _assembly = assembly;
    }

    public byte[] Emit()
    {
        var binaryEmitter = new BinaryEmitter(_assembly, _assembly.Flags.HasFlag(AssemblyFlags.Encryption),
            _assembly.EncryptionKey);
        return binaryEmitter.Emit();
    }
}