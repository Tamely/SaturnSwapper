export module Saturn.Structs.IoStoreTocChunkInfo;

import Saturn.Structs.IoChunkId;
import Saturn.Structs.IoHash;
import Saturn.Structs.IoChunkHash;

import <string>;
import <cstdint>;

export enum EIoChunkType {
    None
};

export struct FIoStoreTocChunkInfo {
    FIoChunkId Id;
    FIoHash ChunkHash;
    FIoChunkHash Hash;
    std::string FileName;
    uint64_t Offset;
    uint64_t OffsetOnDisk;
    uint64_t Size;
    uint64_t CompressedSize;
    uint32_t NumCompressedBlocks;
    int32_t PartitionIndex;
    EIoChunkType ChunkType;
    bool bHasValidFileName;
    bool bForceUncompressed;
    bool bIsMemoryMapped;
    bool bIsCompressed;

    FIoStoreTocChunkInfo() = default;
    FIoStoreTocChunkInfo(const FIoStoreTocChunkInfo&) = default;
    FIoStoreTocChunkInfo(FIoStoreTocChunkInfo&&) = default;
    FIoStoreTocChunkInfo& operator=(FIoStoreTocChunkInfo&) = default;
    FIoStoreTocChunkInfo& operator=(FIoStoreTocChunkInfo&&) = default;
};