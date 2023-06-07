using System;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[Flags]
internal enum TypeKind : byte
{
    Struct,
    Enum,
    GenericType,
    Primitive,
    Numeric,
    Signed,
    FloatingPoint,
}