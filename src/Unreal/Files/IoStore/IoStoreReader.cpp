#include "Saturn/Defines.h"

import Saturn.IoStore.IoStoreReader;

import Saturn.Structs.Guid;
import Saturn.Core.IoStatus;
import Saturn.Encryption.AES;
import Saturn.Structs.IoChunkId;
import Saturn.Structs.IoOffsetLength;
import Saturn.IoStore.IoDirectoryIndex;
import Saturn.Structs.IoStoreTocHeader;
import Saturn.Readers.FileReaderNoWrite;
import Saturn.Structs.IoStoreTocResource;
import Saturn.Structs.IoStoreTocChunkInfo;
import Saturn.Files.WindowsCriticalSection;

import <cstdint>;
import <atomic>;
import <string>;
import <vector>;

class FIoStoreTocReader {
public:
    FIoStoreTocReader() {
        memset(&Toc.Header, 0, sizeof(FIoStoreTocHeader));
    }

    [[nodiscard]] FIoStatus Read(const std::string& TocFilePath, const TMap<FGuid, FAESKey>& DecryptionKeys) {
        FIoStatus TocStatus = FIoStoreTocResource::Read(TocFilePath, EIoStoreTocReadOptions::ReadAll, Toc);
        if (!TocStatus.IsOk()) {
            return TocStatus;
        }

        ChunkIdToIndex.clear();
        for (int32_t ChunkIndex = 0; ChunkIndex < Toc.ChunkIds.size(); ++ChunkIndex) {
            ChunkIdToIndex.insert({ Toc.ChunkIds[ChunkIndex], ChunkIndex });
        }

        if (EnumHasAnyFlags(Toc.Header.ContainerFlags, EIoContainerFlags::Encrypted)) {
            auto It = DecryptionKeys.find(Toc.Header.EncryptionKeyGuid);
            const FAESKey* FindKey = (It != DecryptionKeys.end()) ? &It->second : nullptr;
            if (!FindKey) {
                return FIoStatusBuilder(EIoErrorCode::FileOpenFailed) << "Missing decryption key for IoStore container file '" << TocFilePath << "'";
            }
            DecryptionKey = *FindKey;
        }

        if (EnumHasAnyFlags(Toc.Header.ContainerFlags, EIoContainerFlags::Indexed) &&
            Toc.DirectoryIndexBuffer.size() > 0) {
                FIoStatus DirectoryIndexStatus = DirectoryIndexReader.Initialize(Toc.DirectoryIndexBuffer, DecryptionKey);
                if (!DirectoryIndexStatus.IsOk()) {
                    return DirectoryIndexStatus;
                }
                DirectoryIndexReader.IterateDirectoryIndex(
                    FIoDirectoryIndexHandle::RootDirectory(),
                    "",
                    [this](std::string Filename, uint32_t TocEntryIndex) -> bool
                    {
                        AddFileName(TocEntryIndex, Filename);
                        return true;
                    });
        }

        return TocStatus;
    }

    FIoStoreTocResource& GetTocResource() {
        return Toc;
    }

    const FIoStoreTocResource& GetTocResource() const {
        return Toc;
    }

    const FAESKey& GetDecryptionKey() const {
        return DecryptionKey;
    }

    const FIoDirectoryIndexReader& GetDirectoryIndexReader() const {
        return DirectoryIndexReader;
    }

    const int32_t* GetTocEntryIndex(const FIoChunkId& ChunkId) const {
        auto It = ChunkIdToIndex.find(ChunkId);
        return (It != ChunkIdToIndex.end()) ? &It->second : nullptr;
    }

    const FIoOffsetAndLength* GetOffsetAndLength(const FIoChunkId& ChunkId) const {
        if (const int32_t* Index = GetTocEntryIndex(ChunkId)) {
            return &Toc.ChunkOffsetAndLengths[*Index];
        }
        return nullptr;
    }

    FIoStoreTocChunkInfo GetTocChunkInfo(int32_t TocEntryIndex) const {
        FIoStoreTocChunkInfo ChunkInfo = Toc.GetTocChunkInfo(TocEntryIndex);

        auto It = IndexToFileName.find(TocEntryIndex);
        if (const std::string* FileName = ((It != IndexToFileName.end()) ? &It->second : nullptr); FileName != nullptr) {
            ChunkInfo.FileName = *FileName;
            ChunkInfo.bHasValidFileName = true;
        }
        else {
            ChunkInfo.FileName = std::to_string(static_cast<int>(ChunkInfo.ChunkType)); // TODO: LexToString(ChunkInfo.ChunkType);
            ChunkInfo.bHasValidFileName = false;
        }
        return ChunkInfo;
    }
private:
    void AddFileName(int32_t TocEntryIndex, std::string Filename) {
        IndexToFileName.insert({ TocEntryIndex, Filename });
    }

    FIoStoreTocResource Toc;
    FIoDirectoryIndexReader DirectoryIndexReader;
    FAESKey DecryptionKey;
    TMap<FIoChunkId, int32_t> ChunkIdToIndex;
    TMap<int32_t, std::string> IndexToFileName;
};

class FIoStoreReaderImpl {
public:
	FIoStoreReaderImpl() {}

	//
	// FileReader isn't designed around a lot of jobs throwing accesses at it, so instead we
	// use it directly and round robin between a number of file haandles in order to saturate
	// year 2022 ssd drives. For a file hot in the windows file cache, you can get 4+ GB/s with as few as
	// 4 file handles, however for a cold file you need upwards of 32 in order to reach ~1.5 GB/s. This is
	// low because IoStoreReader (note: not IoDispatcher!) reads are comparatively small - at most you're reading compression block sized
	// chunks when uncompressed, however with Oodle those get cut by ~half, so with a defaualt block size
	// of 64kb, reads are generally less than 32kb, which is tough to use and get full ssd bandwith out of.
	//
	static constexpr uint32_t NumHandlesPerFile = 12;
	struct FContainerFileAccess {
		FCriticalSection HandleLock[NumHandlesPerFile];
		FFileReaderNoWrite* Handle[NumHandlesPerFile];
		std::atomic_uint32_t NextHandleIndex{ 0 };
		bool bValid = false;

		FContainerFileAccess(std::string& ContainerFileName) {
			bValid = true;
			for (uint32_t i = 0; i < NumHandlesPerFile; i++) {
				Handle[i] = new FFileReaderNoWrite(ContainerFileName.c_str());
				if (Handle[i] == nullptr) {
					bValid = false;
				}
			}
		}

		~FContainerFileAccess() {
			for (int32_t Index = 0; Index < NumHandlesPerFile; Index++) {
				if (Handle[Index] != nullptr) {
					Handle[Index]->Close();
					delete Handle[Index];
					Handle[Index] = nullptr;
				}
			}
		}

		bool IsValid() const { return bValid; }
	};

private:
    FIoStoreTocReader TocReader;
    std::vector<TUniquePtr<FContainerFileAccess>> ContainerFileAccessors;
    std::string ContainerPath;
};