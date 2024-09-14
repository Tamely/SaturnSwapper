export module Saturn.Structs.FileInfo;

import Saturn.Structs.IoChunkId;

import <cstdint>;

export struct VFileInfo {
    uint32_t TocEntryIndex;
    uint32_t FirstBlockIndex;
    uint32_t BlockCount;
    FIoChunkId ChunkId;
};