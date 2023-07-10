using System;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class NullObject : RuntimeObject
{
    public override int Size => 0;
    public override RuntimeType Type { get; }
    public override object? Value => null;
    
    public NullObject(RuntimeType type)
    {
        Type = type;
    }
    
    public override int ResolveSize() => Size;
    public override byte[] Serialize() => Array.Empty<byte>();
    public override IRuntimeObject? ComputeOperation(OpCode operation, IRuntimeObject? other) => null;
    public override IRuntimeObject ConvertTo(RuntimeType type) => new NullObject(type);
}