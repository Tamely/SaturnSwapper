using System;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[Flags]
internal enum TypeKind : byte
{
    Struct,
    Enum,
    Primitive,
    Numeric,
    Signed,
    FloatingPoint,
    Array
}