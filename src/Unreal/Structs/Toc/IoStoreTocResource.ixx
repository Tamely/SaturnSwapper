export module Saturn.Structs.IoStoreTocResource;

import <string>;
import <vector>;

import Saturn.Core.IoStatus;
import Saturn.Structs.SHAHash;
import Saturn.Structs.IoChunkId;
import Saturn.Structs.IoOffsetLength;
import Saturn.Structs.IoStoreTocHeader;
import Saturn.Structs.IoStoreTocEntryMeta;
import Saturn.Structs.IoStoreTocChunkInfo;
import Saturn.Structs.IoStoreTocCompressedBlockEntry;

export enum class EIoStoreTocReadOptions {
    Default,
    ReadDirectoryIndex = (1 << 0),
    ReadTocMeta = (1 << 1),
    ReadAll = ReadDirectoryIndex | ReadTocMeta
};

export enum class EIoStoreTocVersion : uint8_t {
	Invalid = 0,
	Initial,
	DirectoryIndex,
	PartitionSize,
	PerfectHash,
	PerfectHashWithOverflow,
	OnDemandMetaData,
	RemovedOnDemandMetaData,
	ReplaceIoChunkHashWithIoHash,
	LatestPlusOne,
	Latest = LatestPlusOne - 1
};

export struct FIoStoreTocResource {
    enum { CompressionMethodNameLen = 32 };

    std::string TocPath;
    FIoStoreTocHeader Header;
    std::vector<FIoChunkId> ChunkIds;
    std::vector<FIoOffsetAndLength> ChunkOffsetAndLengths;
    std::vector<int32_t> ChunkPerfectHashSeeds;
    std::vector<int32_t> ChunkIndicesWithoutPerfectHash;
    std::vector<FIoStoreTocCompressedBlockEntry> CompressionBlocks;
    std::vector<std::string> CompressionMethods;
    FSHAHash SignatureHash;
    std::vector<FSHAHash> ChunkBlockSignatures;
    std::vector<uint8_t> DirectoryIndexBuffer;
    std::vector<FIoStoreTocEntryMeta> ChunkMetas;

    __forceinline const std::string& GetBlockCompressionMethod(FIoStoreTocCompressedBlockEntry& Block) {
        return CompressionMethods[Block.GetCompressionMethodIndex()];
    }

    FIoStoreTocChunkInfo GetTocChunkInfo(int32_t TocEntryIndex) const;
    static FIoStatus Read(const std::string& TocFilePath, EIoStoreTocReadOptions ReadOptions, FIoStoreTocResource& OutTocResource);
    static FIoStatus Write(const std::string& TocFilePath, FIoStoreTocResource& OutTocResource, uint32_t CompressionBlockSize, uint64_t MaxPartitionSize, uint64_t& OutSize);
    static uint64_t HashChunkIdWithSeed(int32_t Seed, const FIoChunkId& ChunkId);
};