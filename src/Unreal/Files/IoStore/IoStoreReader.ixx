module;

#include "Saturn/Defines.h"

export module Saturn.IoStore.IoStoreReader;

import <vector>;
import <memory>;
import <atomic>;
import <string>;
import <cstdint>;
import <future>;
import <functional>;

import Saturn.Structs.Guid;
import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.Encryption.AES;
import Saturn.Structs.IoChunkId;
import Saturn.Misc.IoReadOptions;
import Saturn.Structs.IoContainerId;
import Saturn.IoStore.IoDirectoryIndex;
import Saturn.Structs.IoContainerFlags;
import Saturn.Structs.IoStoreTocChunkInfo;
import Saturn.Container.IoStoreCompressedReadResult;

export class FIoStoreReader : public std::enable_shared_from_this<FIoStoreReader> {
public:
    FIoStoreReader();
    ~FIoStoreReader();

    FIoStatus Initialize(const std::string& InContainerPath, const TMap<FGuid, FAESKey>& InDecryptionKeys);
    FIoContainerId GetContainerId() const;
    uint32_t GetVersion() const;
    EIoContainerFlags GetContainerFlags() const;
    FGuid GetEncryptionKeyGuid() const;
    int32_t GetChunkCount() const;
    std::string GetContainerName() const; // The container name is the babse filename of ContainerPath, e.g. "global"

    void EnumerateChunks(std::function<bool(FIoStoreTocChunkInfo&&)>&& Callback) const;
    TIoStatusOr<FIoStoreTocChunkInfo> GetChunkInfo(const FIoChunkId& Chunk) const;
    TIoStatusOr<FIoStoreTocChunkInfo> GetChunkInfo(const uint32_t TocEntryIndex) const;

    // Reads the chunk off the disk, decryption/decompressing as necessary.
    TIoStatusOr<FIoBuffer> Read(const FIoChunkId& Chunk, const FIoReadOptions& Options) const;

    // As Read(), except returns a task that will contain the result after a .wait/.get.
    std::future<TIoStatusOr<FIoBuffer>> ReadAsync(const FIoChunkId& Chunk, const FIoReadOptions& Options) const;

    // Reads and decrypts if necessary the compressed blocks, bbut does _not_ decompress them, The totality of the data is stored
    // in FIoStoreCompressedReadResult::FIoBuffer as a contiguous buffer, however each block is padded during encryption, so
    // either use FIoStoreCompressedBlockInfo::AlignedSize to advance through the buffer, or use FIoStoreCompressedBlockInfo::OffsetInBuffer
    // directly.
    TIoStatusOr<FIoStoreCompressedReadResult> ReadCompressed(const FIoChunkId& Chunk, const FIoReadOptions& Options, bool bDecrypt = true) const;

    const FIoDirectoryIndexReader& GetDirectoryIndexReader() const;

    // TMap<{xxhashed file path}, TocEntryIndex>
    void GetFiles(TMap<uint64_t, uint32_t>& OutFileList) const;
    void GetFiles(std::vector<std::pair<std::string, std::pair<uint32_t, class FIoStoreReader*>>>& OutFileList) const;
    void GetFilenamesbyBlockIndex(const std::vector<int32_t>& InBlockIndexList, std::vector<std::string>& OutFileList) const;
    void GetFilenames(std::vector<std::string>& OutFileList) const;

    uint32_t GetCompressionBlockSize() const;
    const std::vector<std::string>& GetCompressionMethods() const;
    void EnumerateCompressedBlocks(std::function<bool(const FIoStoreTocCompressedBlockInfo&)>&& Callback) const;
    void EnumerateCompressedBlocksForChunk(const FIoChunkId& Chunk, std::function<bool(const FIoStoreTocCompressedBlockInfo&)>&& Callback) const;

    // Returns the .ucas file path and all partition(s) ({containername}_s1.ucas, {containername}_s2.ucas)
    void GetContainerFilePaths(std::vector<std::string>& OutPaths);
private:
    class FIoStoreReaderImpl* Impl;
};