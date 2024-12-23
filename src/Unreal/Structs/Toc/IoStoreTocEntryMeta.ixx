module;

#include "Saturn/Defines.h"

export module Saturn.Structs.IoStoreTocEntryMeta;

import Saturn.Structs.IoHash;

export enum class FIoStoreTocEntryMetaFlags : uint8_t {
    None,
    Compressed = (1 << 0),
    MemoryMapped = (1 << 1)
};

ENUM_CLASS_FLAGS(FIoStoreTocEntryMetaFlags);

export struct FIoStoreTocEntryMeta {
    FIoHash ChunkHash;
    FIoStoreTocEntryMetaFlags Flags = FIoStoreTocEntryMetaFlags::None;
    uint8_t Pad[3] = { 0 };
};