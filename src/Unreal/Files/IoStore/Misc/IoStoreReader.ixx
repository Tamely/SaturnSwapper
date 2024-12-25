module;

#include "Saturn/Defines.h"

export module Saturn.IoStore.IoStoreReader;

import <memory>;
import <atomic>;
import <string>;
import <cstdint>;
import <functional>;

import Saturn.Structs.Guid;
import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.Encryption.AES;
import Saturn.Structs.IoChunkId;
import Saturn.Misc.IoReadOptions;
import Saturn.Structs.IoContainerId;
import Saturn.Structs.IoContainerFlags;
import Saturn.Structs.IoStoreTocChunkInfo;

export class FIoStoreReader : public std::enable_shared_from_this<FIoStoreReader> {
public:
    FIoStoreReader();
    ~FIoStoreReader();

    FIoStatus Initialize(const std::string& ContainerPath, const TMap<FGuid, FAESKey>& InDecryptionKeys);
    FIoContainerId GetContainerId() const;
    uint32_t GetVersion() const;
    EIoContainerFlags GetContainerFlags() const;
    FGuid GetEncryptionKeyGuid() const;
    int32_t GetChunkCount() const;
    std::string GetContainerName() const; // The container name is the babse filename of ContainerPath, e.g. "global"

    void EnumerateChunks(std::function<bool(FIoStoreTocChunkInfo&&)>&& Callback) const;
    FIoStatus GetChunkInfo(const FIoChunkId& Chunk, FIoStoreTocChunkInfo& OutChunkInfo) const;
    FIoStatus GetChunkInfo(const uint32_t TocEntryIndex, FIoStoreTocChunkInfo& OutChunkInfo) const;

    // Reads the chunk off the disk, decryption/decompressing as necessary.
    FIoStatus Read(const FIoChunkId& Chunk, const FIoReadOptions& Options, FIoBuffer& OutBuffer) const;
};