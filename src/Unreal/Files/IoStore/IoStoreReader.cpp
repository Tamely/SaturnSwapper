#include "Saturn/Defines.h"
#include "Saturn/Log.h"

#include <xxhash/xxhash.h>

import Saturn.IoStore.IoStoreReader;

import Saturn.Compression;
import Saturn.Structs.Guid;
import Saturn.Misc.IoBuffer;
import Saturn.Core.IoStatus;
import Saturn.Encryption.AES;
import Saturn.Structs.IoChunkId;
import Saturn.Readers.FileReader;
import Saturn.Misc.IoReadOptions;
import Saturn.Structs.IoOffsetLength;
import Saturn.IoStore.IoDirectoryIndex;
import Saturn.Structs.IoStoreTocHeader;
import Saturn.Readers.FileReaderNoWrite;
import Saturn.Structs.IoStoreTocResource;
import Saturn.Structs.IoStoreTocChunkInfo;
import Saturn.Files.WindowsCriticalSection;
import Saturn.Container.IoStoreCompressedReadResult;

import <cstdint>;
import <atomic>;
import <future>;
import <string>;
import <vector>;
import <optional>;
import <functional>;

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
            ChunkIdToIndex.insert_or_assign(Toc.ChunkIds[ChunkIndex], ChunkIndex);
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
        auto it = ChunkIdToIndex.find(ChunkId);
        if (it != ChunkIdToIndex.end()) {
            return &it->second;
        }
        return nullptr;
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
	// chunks when uncompressed, however with Oodle those get cut by ~half, so with a default block size
	// of 64kb, reads are generally less than 32kb, which is tough to use and get full ssd bandwith out of.
	//
	static constexpr uint32_t NumHandlesPerFile = 12;
	struct FContainerFileAccess {
		FCriticalSection HandleLock[NumHandlesPerFile];
		FFileReader* Handle[NumHandlesPerFile];
		std::atomic_uint32_t NextHandleIndex{ 0 };
		bool bValid = false;

		FContainerFileAccess(std::string& ContainerFileName) {
			bValid = true;
			for (uint32_t i = 0; i < NumHandlesPerFile; i++) {
				Handle[i] = new FFileReader(ContainerFileName.c_str());
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

    // Kick off an async read from the iostore container, rotating between the file handles for the partition
    std::future<void> StartAsyncRead(int32_t InPartitionIndex, int64_t InPartitionOffset, int64_t InReadAmount, uint8_t* OutBuffer, std::atomic_bool* OutSuccess) const {
        return std::async(std::launch::async, [this, InPartitionIndex, InPartitionOffset, OutBuffer, InReadAmount, OutSuccess]() mutable {
            FContainerFileAccess* ContainerFileAccess = this->ContainerFileAccessors[InPartitionIndex].get();

            // Round robin between the file handles. Since we are alwaays reading blocks, everything is ~roughly~ the same
            // size so we don't have to worry about a single huge read backing up one handle.
            uint32_t OurIndex = ContainerFileAccess->NextHandleIndex.fetch_add(1);
            OurIndex %= NumHandlesPerFile;

            // Each file handle can only be touched by one task at a time. We use an OS lock so that the OS scheduler
            // knows we're in a wait state and who we're waiting on.
            // 
            // CAUTION if any overload of FileReader launches tasks (... unlikely ...) this could deadlock if NumHandlesPerFile is more
            // than the number of worker threads, as the OS lock will not do task retraction.
            {
                ContainerFileAccess->HandleLock[OurIndex].Lock();
            }

            bool bReadSucceeded;
            {
                ContainerFileAccess->Handle[OurIndex]->Seek(InPartitionOffset);
                bReadSucceeded = ContainerFileAccess->Handle[OurIndex]->Serialize(OutBuffer, InReadAmount);
            }

            OutSuccess->store(bReadSucceeded);
            ContainerFileAccess->HandleLock[OurIndex].Unlock();
        });
    }

    [[nodiscard]] FIoStatus Initialize(const std::string& InContainerPath, const TMap<FGuid, FAESKey>& InDecryptionKeys) {
        ContainerPath = InContainerPath;

        std::string TocFilePath;
        TocFilePath.append(InContainerPath);
        TocFilePath.append(".utoc");

        FIoStatus TocStatus = TocReader.Read(TocFilePath, InDecryptionKeys);
        if (!TocStatus.IsOk()) {
            return TocStatus;
        }

        FIoStoreTocResource& TocResource = TocReader.GetTocResource();

        ContainerFileAccessors.reserve(TocResource.Header.PartitionCount);
        for (uint32_t PartitionIndex = 0; PartitionIndex < TocResource.Header.PartitionCount; ++PartitionIndex) {
            std::string ContainerFilePath;
            ContainerFilePath.append(InContainerPath);
            if (PartitionIndex > 0) {
                ContainerFilePath.append("_s");
                ContainerFilePath.append(std::to_string(PartitionIndex));
            }
            ContainerFilePath.append(".ucas");

            ContainerFileAccessors.emplace_back(std::unique_ptr<FContainerFileAccess>(new FContainerFileAccess(ContainerFilePath)));
            if (ContainerFileAccessors[PartitionIndex]->IsValid() == false) {
                return FIoStatusBuilder(EIoErrorCode::FileOpenFailed) << "Failed to open IoStore container file '" << TocFilePath << "'";
            }
        }

        return FIoStatus::Ok;
    }

    FIoContainerId GetContainerId() const {
        return TocReader.GetTocResource().Header.ContainerId;
    }

    uint32_t GetVersion() const {
        return TocReader.GetTocResource().Header.Version;
    }

    EIoContainerFlags GetContainerFlags() const {
        return TocReader.GetTocResource().Header.ContainerFlags;
    }

    FGuid GetEncryptionKeyGuid() const {
        return TocReader.GetTocResource().Header.EncryptionKeyGuid;
    }

    std::string GetContainerName() const {
        return ContainerPath.substr(ContainerPath.find_last_of("/\\") + 1);
    }

    int32_t GetChunkCount() const {
        return TocReader.GetTocResource().ChunkIds.size();
    }

    void EnumerateChunks(std::function<bool(FIoStoreTocChunkInfo&&)>&& Callback) const {
        const FIoStoreTocResource& TocResource = TocReader.GetTocResource();

        for (int32_t ChunkIndex = 0; ChunkIndex < TocResource.ChunkIds.size(); ++ChunkIndex) {
            FIoStoreTocChunkInfo ChunkInfo = TocReader.GetTocChunkInfo(ChunkIndex);
            if (!Callback(std::move(ChunkInfo))) {
                break;
            }
        }
    }

    TIoStatusOr<FIoStoreTocChunkInfo> GetChunkInfo(const FIoChunkId& ChunkId) const {
        const int32_t* TocEntryIndex = TocReader.GetTocEntryIndex(ChunkId);
        if (TocEntryIndex) {
            return TocReader.GetTocChunkInfo(*TocEntryIndex);
        }
        else {
            return FIoStatus(EIoErrorCode::NotFound, "Not found");
        }
    }

    TIoStatusOr<FIoStoreTocChunkInfo> GetChunkInfo(const uint32_t TocEntryIndex) const {
        const FIoStoreTocResource& TocResource = TocReader.GetTocResource();

        if (TocEntryIndex < uint32_t(TocResource.ChunkIds.size())) {
            return TocReader.GetTocChunkInfo(TocEntryIndex);
        }
        else {
            return FIoStatus(EIoErrorCode::InvalidParameter, "Invalid TocEntryIndex");
        }
    }

    std::future<TIoStatusOr<FIoBuffer>> ReadAsync(const FIoChunkId& ChunkId, const FIoReadOptions& Options) const {
        struct FState {
            std::vector<uint8_t> CompressedBuffer;
            uint64_t CompressedSize = 0;
            uint64_t UncompressedSize = 0;
            std::optional<FIoBuffer> UncompressedBuffer;
            std::atomic_bool bReadSucceeded { false };
            std::atomic_bool bUncompressFailed { false };
        };

        const FIoOffsetAndLength* OffsetAndLength = TocReader.GetOffsetAndLength(ChunkId);
        if (!OffsetAndLength) {
            // Currently, there's no way to make a task with a valid result that just emplaces
            // without running.
            return std::async(std::launch::deferred, []() {
                return TIoStatusOr<FIoBuffer>(FIoStatus(EIoErrorCode::NotFound, "Unknown chunk ID"));
            });
        }

        const uint64_t RequestedOffset = Options.GetOffset();
        const uint64_t ResolvedOffset = OffsetAndLength->GetOffset() + RequestedOffset;
        const uint64_t ResolvedSize = RequestedOffset <= OffsetAndLength->GetLength() ? std::min(Options.GetSize(), OffsetAndLength->GetLength() - RequestedOffset) : 0;
        const FIoStoreTocResource& TocResource = TocReader.GetTocResource();
        const uint64_t CompressionBlockSize = TocResource.Header.CompressionBlockSize;
        const int32_t FirstBlockIndex = int32_t(ResolvedOffset / CompressionBlockSize);
        const int32_t LastBlockIndex = int32_t((Align(ResolvedOffset + ResolvedSize, CompressionBlockSize) - 1) / CompressionBlockSize);
        const int32_t BlockCount = LastBlockIndex - FirstBlockIndex + 1;
        if (!BlockCount) {
            // Currently there's no way to make a task with a valid result that just emplaces
            // without running.
            return std::async(std::launch::deferred, []() {
                return TIoStatusOr<FIoBuffer>(); // Return an empty buffer
            });
        }
        const FIoStoreTocCompressedBlockEntry& FirstBlock = TocResource.CompressionBlocks[FirstBlockIndex];
        const FIoStoreTocCompressedBlockEntry& LastBlock = TocResource.CompressionBlocks[LastBlockIndex];
        const int32_t PartitionIndex = static_cast<int32_t>(FirstBlock.GetOffset() / TocResource.Header.PartitionSize);
        const uint64_t ReadStartOffset = FirstBlock.GetOffset() % TocResource.Header.PartitionSize;
        const uint64_t ReadEndOffset = (LastBlock.GetOffset() + Align(LastBlock.GetCompressedSize(), FAESKey::AESBlockSize)) % TocResource.Header.PartitionSize;
        FState* State = new FState();
        State->CompressedSize = ReadEndOffset - ReadStartOffset;
        State->UncompressedSize = ResolvedSize;
        State->CompressedBuffer.resize(State->CompressedSize);
        State->UncompressedBuffer.emplace(State->UncompressedSize);

        std::future<void> ReadJob = StartAsyncRead(PartitionIndex, ReadStartOffset, (int32_t)State->CompressedSize, State->CompressedBuffer.data(), &State->bReadSucceeded);

        std::future<TIoStatusOr<FIoBuffer>> ReturnTask = std::async(std::launch::async, [this, State, PartitionIndex, CompressionBlockSize, ResolvedOffset, FirstBlockIndex, LastBlockIndex, ResolvedSize, ReadStartOffset, &TocResource, ReadJob = std::move(ReadJob)]() {
            ReadJob.wait();

            uint64_t CompressedSourceOffset = 0;
            uint64_t UncompressedDestinationOffset = 0;
            uint64_t OffsetInBlock = ResolvedOffset % CompressionBlockSize;
            uint64_t RemainingSize = ResolvedSize;

            std::vector<std::future<void>> DecompressionTasks;

            for (int32_t BlockIndex = FirstBlockIndex; BlockIndex <= LastBlockIndex; ++BlockIndex) {
                DecompressionTasks.emplace_back(std::async(std::launch::async, [this, State, BlockIndex, CompressedSourceOffset, UncompressedDestinationOffset, OffsetInBlock, RemainingSize]() {
                    if (State->bReadSucceeded) {
                        uint8_t* CompressedSource = State->CompressedBuffer.data() + CompressedSourceOffset;
                        uint8_t* UncompressedDestination = State->UncompressedBuffer->Data() + UncompressedDestinationOffset;
                        const FIoStoreTocResource& TocResource = TocReader.GetTocResource();
                        const FIoStoreTocCompressedBlockEntry& CompressionBlock = TocResource.CompressionBlocks[BlockIndex];
                        const uint32_t RawSize = Align(CompressionBlock.GetCompressedSize(), FAESKey::AESBlockSize);
                        const uint32_t UncompressedSize = CompressionBlock.GetUncompressedSize();
                        std::string CompressionMethod = TocResource.CompressionMethods[CompressionBlock.GetCompressionMethodIndex()];
                        if (EnumHasAnyFlags(TocResource.Header.ContainerFlags, EIoContainerFlags::Encrypted)) {
                            TocReader.GetDecryptionKey().DecryptData(CompressedSource, RawSize);
                        }
                        if (CompressionMethod.contains("None")) {
                            memcpy(UncompressedDestination, CompressedSource + OffsetInBlock, UncompressedSize - OffsetInBlock);
                        }
                        else {
                            bool bUncompressed;
                            if (OffsetInBlock || RemainingSize < UncompressedSize) {
                                std::vector<uint8_t> TempBuffer;
                                TempBuffer.resize(UncompressedSize);
                                FCompression::DecompressMemory(CompressionMethod, TempBuffer.data(), UncompressedSize, CompressedSource, CompressionBlock.GetCompressedSize());
                                bUncompressed = std::any_of(TempBuffer.begin(), TempBuffer.end(), [](uint8_t byte) { return byte != 0; });
                                uint64_t CopySize = std::min(static_cast<uint64_t>(UncompressedSize) - OffsetInBlock, RemainingSize);
                                memcpy(UncompressedDestination, TempBuffer.data() + OffsetInBlock, CopySize);
                            }
                            else {
                                FCompression::DecompressMemory(CompressionMethod, UncompressedDestination, UncompressedSize, CompressedSource, CompressionBlock.GetCompressedSize());
                                bUncompressed = std::any_of(UncompressedDestination, UncompressedDestination + UncompressedSize, [](uint8_t byte) { return byte != 0; });
                            }

                            if (!bUncompressed) {
                                State->bUncompressFailed = true;
                            }
                        }
                    } // end if read succeeded
                })); // end decompression lambda

                const FIoStoreTocCompressedBlockEntry& CompressionBlock = TocResource.CompressionBlocks[BlockIndex];
                const uint32_t RawSize = Align(CompressionBlock.GetCompressedSize(), FAESKey::AESBlockSize);
                CompressedSourceOffset += RawSize;
                UncompressedDestinationOffset += CompressionBlock.GetUncompressedSize();
                RemainingSize -= CompressionBlock.GetUncompressedSize();
                OffsetInBlock = 0;
            } // end for each block

            for (auto& Task : DecompressionTasks) {
                Task.get();
            }

            TIoStatusOr<FIoBuffer> Result;
            if (State->bReadSucceeded == false) {
                Result = FIoStatus(EIoErrorCode::ReadError, "Failed reading chunk from container file");
            }
            else if (State->bUncompressFailed) {
                Result = FIoStatus(EIoErrorCode::ReadError, "Failed uncompressing chunk");
            }
            else {
                Result = State->UncompressedBuffer.value();
            }
            delete State;

            return Result;
        });

        return ReturnTask;
    }

    TIoStatusOr<FIoBuffer> Read(const FIoChunkId& ChunkId, const FIoReadOptions& Options) const {
        const FIoOffsetAndLength* OffsetAndLength = TocReader.GetOffsetAndLength(ChunkId);
        if (!OffsetAndLength) {
            return FIoStatus(EIoErrorCode::NotFound, "Unknown chunk ID");
        }

        uint64_t RequestedOffset = Options.GetOffset();
        uint64_t ResolvedOffset = OffsetAndLength->GetOffset() + RequestedOffset;
        uint64_t ResolvedSize = 0;
        if (RequestedOffset <= OffsetAndLength->GetLength()) {
            ResolvedSize = std::min(Options.GetSize(), OffsetAndLength->GetLength() - RequestedOffset);
        }

        const FIoStoreTocResource& TocResource = TocReader.GetTocResource();
        const uint64_t CompressionBlockSize = TocResource.Header.CompressionBlockSize;
        FIoBuffer UncompressedBuffer(ResolvedSize);
        if (ResolvedSize == 0) {
            return UncompressedBuffer;
        }

        // From here on we are reading / decompressing at least one block

        // We try to overlap the IO for the next block with the decrypt/decompress for the current
        // block, which requires two IO buffers.
        std::vector<uint8_t> CompressedBuffers[2];
        std::atomic_bool AsyncReadSucceeded[2];

        int32_t FirstBlockIndex = int32_t(ResolvedOffset / CompressionBlockSize);
        int32_t LastBlockIndex = int32_t((Align(ResolvedOffset + ResolvedSize, CompressionBlockSize) - 1) / CompressionBlockSize);

        // Lambda to kick off a read with a sufficient output buffer.
        auto LaunchBlockRead = [&TocResource, this](int32_t BlockIndex, std::vector<uint8_t>& DestinationBuffer, std::atomic_bool* OutReadSucceeded) {
            const uint64_t CompressionBlockSize = TocResource.Header.CompressionBlockSize;
            const FIoStoreTocCompressedBlockEntry& CompressionBlock = TocResource.CompressionBlocks[BlockIndex];

            // CompressionBlockSize is technically the _uncompressed_ block size, however it's a good
            // size to use for reuse as block compression can vary wildly and we want to be able to
            // read block that happen to be uncompressed.
            uint32_t SizeForDecrypt = Align(CompressionBlock.GetCompressedSize(), FAESKey::AESBlockSize);
            uint32_t CompressedBufferSizeNeeded = std::max(uint32_t(CompressionBlockSize), SizeForDecrypt);

            if (uint32_t(DestinationBuffer.size()) < CompressedBufferSizeNeeded) {
                DestinationBuffer.resize(CompressedBufferSizeNeeded);
            }

            int32_t PartitionIndex = int32_t(CompressionBlock.GetOffset() / TocResource.Header.PartitionSize);
            int64_t PartitionOffset = int64_t(CompressionBlock.GetOffset() % TocResource.Header.PartitionSize);
            return StartAsyncRead(PartitionIndex, PartitionOffset, SizeForDecrypt, DestinationBuffer.data(), OutReadSucceeded);
        };

        // Kick off the first async read
        std::future<void> NextReadRequest;
        uint8_t NextReadBufferIndex = 0;
        NextReadRequest = LaunchBlockRead(FirstBlockIndex, CompressedBuffers[NextReadBufferIndex], &AsyncReadSucceeded[NextReadBufferIndex]);

        uint64_t UncompressedDestinationOffset = 0;
        uint64_t OffsetInBlock = ResolvedOffset % CompressionBlockSize;
        uint64_t RemainingSize = ResolvedSize;
        std::vector<uint8_t> TempBuffer;
        for (int32_t BlockIndex = FirstBlockIndex; BlockIndex <= LastBlockIndex; ++BlockIndex) {
            // Kick off the next block's IO if there is one
            std::future<void> ReadRequest(std::move(NextReadRequest));
            uint8_t OurBufferIndex = NextReadBufferIndex;
            if (BlockIndex + 1 <= LastBlockIndex) {
                NextReadBufferIndex = NextReadBufferIndex ^ 1;
                NextReadRequest = LaunchBlockRead(BlockIndex + 1, CompressedBuffers[NextReadBufferIndex], &AsyncReadSucceeded[NextReadBufferIndex]);
            }

            // Now, wait for _our_ block's IO
            {
                ReadRequest.wait();
            }

            if (AsyncReadSucceeded[OurBufferIndex] == false) {
                return FIoStatus(EIoErrorCode::ReadError, "Failed async read in FIoStoreReader::ReadCompressed");
            }

            const FIoStoreTocCompressedBlockEntry& CompressionBlock = TocResource.CompressionBlocks[BlockIndex];

            // This also happened in the LaunchBlockRead call, so we know the buffer has the necessary size.
            uint32_t RawSize = Align(CompressionBlock.GetCompressedSize(), FAESKey::AESBlockSize);
            if (EnumHasAnyFlags(TocResource.Header.ContainerFlags, EIoContainerFlags::Encrypted)) {
                TocReader.GetDecryptionKey().DecryptData(CompressedBuffers[OurBufferIndex].data(), RawSize);
            }

            std::string CompressionMethod = TocResource.CompressionMethods[CompressionBlock.GetCompressionMethodIndex()];
            uint8_t* UncompressedDestination = UncompressedBuffer.Data() + UncompressedDestinationOffset;
            const uint32_t UncompressedSize = CompressionBlock.GetUncompressedSize();
            if (CompressionMethod.contains("None")) {
                uint64_t CopySize = std::min(static_cast<uint64_t>(UncompressedSize) - OffsetInBlock, RemainingSize);
                memcpy(UncompressedDestination, CompressedBuffers[OurBufferIndex].data() + OffsetInBlock, CopySize);
                UncompressedDestinationOffset += CopySize;
                RemainingSize -= CopySize;
            }
            else {
                bool bUncompressed;
                if (OffsetInBlock || RemainingSize < UncompressedSize) {
                    // If this block is larger than the amount of data actuall requested, decompress to a temp
                    // buffer and then copy out. Should never happen when reading the entire chunk.
                    TempBuffer.clear();
                    TempBuffer.resize(UncompressedSize);
                    FCompression::DecompressMemory(CompressionMethod, TempBuffer.data(), UncompressedSize, CompressedBuffers[OurBufferIndex].data(), CompressionBlock.GetCompressedSize());
                    bUncompressed = std::any_of(TempBuffer.begin(), TempBuffer.end(), [](uint8_t byte) { return byte != 0; });
                    uint64_t CopySize = std::min(static_cast<uint64_t>(UncompressedSize) - OffsetInBlock, RemainingSize);
                    memcpy(UncompressedDestination, TempBuffer.data() + OffsetInBlock, CopySize);
                    UncompressedDestinationOffset += CopySize;
                    RemainingSize -= CopySize;
                }
                else {
                    FCompression::DecompressMemory(CompressionMethod, UncompressedDestination, UncompressedSize, CompressedBuffers[OurBufferIndex].data(), CompressionBlock.GetCompressedSize());
                    bUncompressed = std::any_of(UncompressedDestination, UncompressedDestination + UncompressedSize, [](uint8_t byte) { return byte != 0; });
                    UncompressedDestinationOffset += UncompressedSize;
                    RemainingSize -= UncompressedSize;
                }

                if (!bUncompressed) {
                    return FIoStatus(EIoErrorCode::ReadError, "Failed uncompressing chunk");
                }
            }
            OffsetInBlock = 0;
        }
        return UncompressedBuffer;
    }

    TIoStatusOr<FIoStoreCompressedReadResult> ReadCompressed(const FIoChunkId& ChunkId, const FIoReadOptions& Options, bool bDecrypt) const {
        // Find where in the virtual file the chunk exists.
        const FIoOffsetAndLength* OffsetAndLength = TocReader.GetOffsetAndLength(ChunkId);
        if (!OffsetAndLength) {
            return FIoStatus(EIoErrorCode::NotFound, "Unknown chunk ID");
        }

        // Combine with offset/size requested by the reader
        uint64_t RequestedOffset = Options.GetOffset();
        uint64_t ResolvedOffset = OffsetAndLength->GetOffset() + RequestedOffset;
        uint64_t ResolvedSize = 0;
        if (RequestedOffset <= OffsetAndLength->GetLength()) {
            ResolvedSize = std::min(Options.GetSize(), OffsetAndLength->GetLength() - RequestedOffset);
        }

        // Find what compressed blocks this read straddles.
        const FIoStoreTocResource& TocResource = TocReader.GetTocResource();
        const uint64_t CompressionBlockSize = TocResource.Header.CompressionBlockSize;
        int32_t FirstBlockIndex = int32_t(ResolvedOffset / CompressionBlockSize);
        int32_t LastBlockIndex = int32_t((Align(ResolvedOffset + ResolvedSize, CompressionBlockSize) - 1) / CompressionBlockSize);

        // Determine size of the result and set up output buffers
        uint64_t TotalCompressedSize = 0;
        uint64_t TotalAlignedSize = 0;
        for (int32_t BlockIndex = FirstBlockIndex; BlockIndex <= LastBlockIndex; ++BlockIndex) {
            const FIoStoreTocCompressedBlockEntry& CompressionBlock = TocResource.CompressionBlocks[BlockIndex];
            TotalCompressedSize += CompressionBlock.GetCompressedSize();
            TotalAlignedSize += Align(CompressionBlock.GetCompressedSize(), FAESKey::AESBlockSize);
        }

        FIoStoreCompressedReadResult Result;
        Result.IoBuffer = FIoBuffer(TotalAlignedSize);
        Result.UncompressedOffset = ResolvedOffset % CompressionBlockSize;
        Result.UncompressedSize = ResolvedSize;
        Result.TotalCompressedSize = TotalCompressedSize;

        // Set up the result blocks.
        uint64_t CurrentOffset = 0;
        for (int32_t BlockIndex = FirstBlockIndex; BlockIndex <= LastBlockIndex; ++BlockIndex) {
            const FIoStoreTocCompressedBlockEntry& CompressionBlock = TocResource.CompressionBlocks[BlockIndex];
            FIoStoreCompressedBlockInfo BlockInfo;

            BlockInfo.CompressionMethod = TocResource.CompressionMethods[CompressionBlock.GetCompressionMethodIndex()];
            BlockInfo.CompressedSize = CompressionBlock.GetCompressedSize();
            BlockInfo.UncompressedSize = CompressionBlock.GetUncompressedSize();
            BlockInfo.OffsetInBuffer = CurrentOffset;
            BlockInfo.AlignedSize = Align(CompressionBlock.GetCompressedSize(), FAESKey::AESBlockSize);
            CurrentOffset += BlockInfo.AlignedSize;

            Result.Blocks.push_back(BlockInfo);
        }

        uint8_t* OutputBuffer = Result.IoBuffer.Data();

        // We can read the entire thing at once since we obligate the caller to skip the alignment padding.
        {
            const FIoStoreTocCompressedBlockEntry& CompressionBlock = TocResource.CompressionBlocks[FirstBlockIndex];
            int32_t PartitionIndex = int32_t(CompressionBlock.GetOffset() / TocResource.Header.PartitionSize);
            int64_t PartitionOffset = int64_t(CompressionBlock.GetOffset() % TocResource.Header.PartitionSize);

            std::atomic_bool bReadSucceeded;
            std::future<void> ReadTask = StartAsyncRead(PartitionIndex, PartitionOffset, TotalAlignedSize, OutputBuffer, &bReadSucceeded);

            {
                ReadTask.wait();
            }

            if (bReadSucceeded == false) {
                LOG_ERROR("Read from container {0} failed (partition {1}, offset {2}, size {3})", ContainerPath, PartitionIndex, PartitionOffset, TotalAlignedSize);
                return FIoStoreCompressedReadResult();
            }
        }

        if (bDecrypt && EnumHasAnyFlags(TocResource.Header.ContainerFlags, EIoContainerFlags::Encrypted)) {
            for (int32_t BlockIndex = FirstBlockIndex; BlockIndex <= LastBlockIndex; ++BlockIndex) {
                FIoStoreCompressedBlockInfo& OutputBlock = Result.Blocks[BlockIndex - FirstBlockIndex];
                uint8_t* Buffer = OutputBuffer + OutputBlock.OffsetInBuffer;
                TocReader.GetDecryptionKey().DecryptData(Buffer, OutputBlock.AlignedSize);
            }
        }

        return Result;
    }

    const FIoDirectoryIndexReader& GetDirectoryIndexReader() const {
        return TocReader.GetDirectoryIndexReader();
    }

    bool TocChunkContainsBlockIndex(const int32_t TocEntryIndex, const int32_t BlockIndex) const {
        const FIoStoreTocResource& TocResource = TocReader.GetTocResource();
        const FIoOffsetAndLength& OffsetLength = TocResource.ChunkOffsetAndLengths[TocEntryIndex];

        const uint64_t CompressionBlockSize = TocResource.Header.CompressionBlockSize;
        int32_t FirstBlockIndex = int32_t(OffsetLength.GetOffset() / CompressionBlockSize);
        int32_t LastBlockIndex = int32_t((Align(OffsetLength.GetOffset() + OffsetLength.GetLength(), CompressionBlockSize) - 1) / CompressionBlockSize);

        return BlockIndex >= FirstBlockIndex && BlockIndex <= LastBlockIndex;
    }

    uint32_t GetCompressionBlockSize() const {
        return TocReader.GetTocResource().Header.CompressionBlockSize;
    }

    const std::vector<std::string>& GetCompressionMethods() const {
        return TocReader.GetTocResource().CompressionMethods;
    }

    bool EnumerateCompressedBlocksForChunk(const FIoChunkId& ChunkId, std::function<bool(const FIoStoreTocCompressedBlockInfo&)>&& Callback) const {
        const FIoOffsetAndLength* OffsetAndLength = TocReader.GetOffsetAndLength(ChunkId);
        if (!OffsetAndLength) {
            return false;
        }

        // Find what compressed blocks this chunk straddles.
        const FIoStoreTocResource& TocResource = TocReader.GetTocResource();
        const uint64_t CompressionBlockSize = TocResource.Header.CompressionBlockSize;
        int32_t FirstBlockIndex = int32_t(OffsetAndLength->GetOffset() / CompressionBlockSize);
        int32_t LastBlockIndex = int32_t((Align(OffsetAndLength->GetOffset() + OffsetAndLength->GetLength(), CompressionBlockSize) - 1) / CompressionBlockSize);

        for (int32_t BlockIndex = FirstBlockIndex; BlockIndex <= LastBlockIndex; ++BlockIndex) {
            const FIoStoreTocCompressedBlockEntry& Entry = TocResource.CompressionBlocks[BlockIndex];
            FIoStoreTocCompressedBlockInfo Info {
                Entry.GetOffset(),
                Entry.GetCompressedSize(),
                Entry.GetUncompressedSize(),
                Entry.GetCompressionMethodIndex()
            };
            if (!Callback(Info)) {
                break;
            }
        }
        return true;
    }

    void EnumerateCompressedBlocks(std::function<bool(const FIoStoreTocCompressedBlockInfo&)>&& Callback) const {
        const FIoStoreTocResource& TocResource = TocReader.GetTocResource();

        for (int32_t BlockIndex = 0; BlockIndex <= TocResource.CompressionBlocks.size(); ++BlockIndex) {
            const FIoStoreTocCompressedBlockEntry& Entry = TocResource.CompressionBlocks[BlockIndex];
            FIoStoreTocCompressedBlockInfo Info{
                Entry.GetOffset(),
                Entry.GetCompressedSize(),
                Entry.GetUncompressedSize(),
                Entry.GetCompressionMethodIndex()
            };
            if (!Callback(Info)) {
                break;
            }
        }
    }

    void GetContainerFilePaths(std::vector<std::string>& OutPaths) {
        std::string Sb;

        for (uint32_t PartitionIndex = 0; PartitionIndex < TocReader.GetTocResource().Header.PartitionCount; ++PartitionIndex) {
            Sb.clear();
            Sb.append(ContainerPath);
            if (PartitionIndex > 0) {
                Sb.append("_s" + PartitionIndex);
            }
            Sb.append(".ucas");
            OutPaths.push_back(Sb);
        }
    }

    FIoStoreTocResource& GetTocResource() {
        return TocReader.GetTocResource();
    }

    const FIoOffsetAndLength* GetOffsetAndLength(const FIoChunkId& ChunkId) const {
        return TocReader.GetOffsetAndLength(ChunkId);
    }
private:
    FIoStoreTocReader TocReader;
    std::vector<TUniquePtr<FContainerFileAccess>> ContainerFileAccessors;
    std::string ContainerPath;
};

FIoStoreReader::FIoStoreReader() : Impl(new FIoStoreReaderImpl()) {}
FIoStoreReader::~FIoStoreReader() { delete Impl; }

FIoStatus FIoStoreReader::Initialize(const std::string& InContainerPath, const TMap<FGuid, FAESKey>& InDecryptionKeys) {
    return Impl->Initialize(InContainerPath, InDecryptionKeys);
}

FIoContainerId FIoStoreReader::GetContainerId() const {
    return Impl->GetContainerId();
}

uint32_t FIoStoreReader::GetVersion() const {
    return Impl->GetVersion();
}

EIoContainerFlags FIoStoreReader::GetContainerFlags() const {
    return Impl->GetContainerFlags();
}

FGuid FIoStoreReader::GetEncryptionKeyGuid() const {
    return Impl->GetEncryptionKeyGuid();
}

int32_t FIoStoreReader::GetChunkCount() const {
    return Impl->GetChunkCount();
}

std::string FIoStoreReader::GetContainerName() const {
    return Impl->GetContainerName();
}

void FIoStoreReader::EnumerateChunks(std::function<bool(FIoStoreTocChunkInfo&&)>&& Callback) const {
    Impl->EnumerateChunks(std::move(Callback));
}

TIoStatusOr<FIoStoreTocChunkInfo> FIoStoreReader::GetChunkInfo(const FIoChunkId& Chunk) const {
    return Impl->GetChunkInfo(Chunk);
}

FIoStoreTocResource& FIoStoreReader::GetTocResource() {
    return Impl->GetTocResource();
}

FIoOffsetAndLength* FIoStoreReader::GetOffsetAndLength(FIoChunkId& ChunkId) {
    return const_cast<FIoOffsetAndLength*>(std::move(Impl->GetOffsetAndLength(ChunkId)));
}

TIoStatusOr<FIoStoreTocChunkInfo> FIoStoreReader::GetChunkInfo(const uint32_t TocEntryIndex) const {
    return Impl->GetChunkInfo(TocEntryIndex);
}

TIoStatusOr<FIoBuffer> FIoStoreReader::Read(const FIoChunkId& Chunk, const FIoReadOptions& Options) const {
    return Impl->Read(Chunk, Options);
}

TIoStatusOr<FIoStoreCompressedReadResult> FIoStoreReader::ReadCompressed(const FIoChunkId& Chunk, const FIoReadOptions& Options, bool bDecrypt) const {
    return Impl->ReadCompressed(Chunk, Options, bDecrypt);
}

std::future<TIoStatusOr<FIoBuffer>> FIoStoreReader::ReadAsync(const FIoChunkId& Chunk, const FIoReadOptions& Options) const {
    return Impl->ReadAsync(Chunk, Options);
}

const FIoDirectoryIndexReader& FIoStoreReader::GetDirectoryIndexReader() const {
    return Impl->GetDirectoryIndexReader();
}

uint32_t FIoStoreReader::GetCompressionBlockSize() const {
    return Impl->GetCompressionBlockSize();
}

const std::vector<std::string>& FIoStoreReader::GetCompressionMethods() const {
    return Impl->GetCompressionMethods();
}

void FIoStoreReader::EnumerateCompressedBlocks(std::function<bool(const FIoStoreTocCompressedBlockInfo&)>&& Callback) const {
    Impl->EnumerateCompressedBlocks(std::move(Callback));
}

void FIoStoreReader::EnumerateCompressedBlocksForChunk(const FIoChunkId& Chunk, std::function<bool(const FIoStoreTocCompressedBlockInfo&)>&& Callback) const {
    Impl->EnumerateCompressedBlocksForChunk(Chunk, std::move(Callback));
}

void FIoStoreReader::GetContainerFilePaths(std::vector<std::string>& OutPaths) {
    Impl->GetContainerFilePaths(OutPaths);
}

void FIoStoreReader::GetFiles(TMap<uint64_t, uint32_t>& OutFileList) const {
    const FIoDirectoryIndexReader& DirectoryIndex = GetDirectoryIndexReader();

    DirectoryIndex.IterateDirectoryIndex(
        FIoDirectoryIndexHandle::RootDirectory(),
        "",
        [&OutFileList](std::string Filename, uint32_t TocEntryIndex) -> bool {
            OutFileList.insert({ XXH3_64bits(Filename.c_str(), Filename.size()), TocEntryIndex });
            return true;
        });
}

void FIoStoreReader::GetFiles(std::vector<std::pair<std::string, uint32_t>>& OutFileList) const {
    const FIoDirectoryIndexReader& DirectoryIndex = GetDirectoryIndexReader();

    DirectoryIndex.IterateDirectoryIndex(
        FIoDirectoryIndexHandle::RootDirectory(),
        "",
        [this, &OutFileList](std::string Filename, uint32_t TocEntryIndex) -> bool {
            OutFileList.emplace_back(Filename, TocEntryIndex);
            return true;
        });
}

void FIoStoreReader::GetFilenames(std::vector<std::string>& OutFileList) const {
    const FIoDirectoryIndexReader& DirectoryIndex = GetDirectoryIndexReader();

    DirectoryIndex.IterateDirectoryIndex(
        FIoDirectoryIndexHandle::RootDirectory(),
        "",
        [&OutFileList](std::string Filename, uint32_t TocEntryIndex) -> bool {
            OutFileList.emplace_back(Filename);
            return true;
        });
}

void FIoStoreReader::GetFilenamesbyBlockIndex(const std::vector<int32_t>& InBlockIndexList, std::vector<std::string>& OutFileList) const {
    const FIoDirectoryIndexReader& DirectoryIndex = GetDirectoryIndexReader();

    DirectoryIndex.IterateDirectoryIndex(
        FIoDirectoryIndexHandle::RootDirectory(),
        "",
        [this, &InBlockIndexList, &OutFileList](std::string Filename, uint32_t TocEntryIndex) -> bool {
            for (int32_t BlockIndex : InBlockIndexList) {
                if (Impl->TocChunkContainsBlockIndex(TocEntryIndex, BlockIndex)) {
                    OutFileList.emplace_back(Filename);
                    break;
                }
            }
            return true;
        });
}