using System;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[Flags]
public enum TypeKind
{
    Struct = 0x01,
    Enum = 0x02,
    Primitive = 0x04,
    Numeric = 0x08,
    Signed = 0x10,
    FloatingPoint = 0x20,
    Array = 0x40,
    ValueType = 0x80,
    Pointer = 0x100,
}
