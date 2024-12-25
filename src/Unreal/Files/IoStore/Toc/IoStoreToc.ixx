module;

#include "Saturn/Defines.h"

export module Saturn.Toc.IoStoreToc;

import Saturn.Structs.IoOffsetLength;
import Saturn.Encryption.AES;
import Saturn.Files.FileEntry;
import Saturn.Structs.IoChunkId;

export class FIoStoreToc : public IDiskFile {
public:
    FIoStoreToc() = default;

    FIoStoreToc(std::string& TocFilePath);
    FIoStoreToc(TSharedPtr<struct FIoStoreTocResource> TocRsrc);

    FAESKey& GetEncryptionKey();
    TSharedPtr<struct FIoStoreTocResource> GetResource();
    std::string GetDiskPath() override;
    void SetKey(FAESKey& InKey);
    void SetReader(TSharedPtr<class FIoStoreReader> InReader);
    int32_t GetTocEntryIndex(FIoChunkId& ChunkId);

    FIoOffsetAndLength GetOffsetAndLength(FIoChunkId& ChunkId);
    std::vector<uint8_t> ReadEntry(struct FFileEntryInfo& Entry) override;
private:
    FAESKey Key;
    TSharedPtr<struct FIoStoreTocResource> Toc;
    TSharedPtr<class FIoStoreReader> Reader;
    TMap<FIoChunkId, int32_t> ChunkIdToIndex;
};