#include "Saturn/Defines.h"

import Saturn.Structs.IoStoreTocResource;

import <string>;
import <vector>;
import <memory>;

import Saturn.Core.IoStatus;
import Saturn.Readers.MemoryReader;
import Saturn.Readers.FileReaderNoWrite;

import Saturn.Structs.IoChunkHash;
import Saturn.Structs.IoContainerFlags;
import Saturn.Structs.IoStoreTocHeader;
import Saturn.Structs.IoOffsetLength;
import Saturn.Structs.IoStoreTocEntryMeta;
import Saturn.Structs.IoStoreTocChunkInfo;

FIoStoreTocChunkInfo FIoStoreTocResource::GetTocChunkInfo(int32_t TocEntryIndex) const {
    const FIoStoreTocEntryMeta& Meta = ChunkMetas[TocEntryIndex];
    const FIoOffsetAndLength& OffsetAndLength = ChunkOffsetAndLengths[TocEntryIndex];

    const bool bIsContainerCompressed = EnumHasAnyFlags(Header.ContainerFlags, EIoContainerFlags::Compressed);

    FIoStoreTocChunkInfo ChunkInfo;
    ChunkInfo.Id = ChunkIds[TocEntryIndex];
    ChunkInfo.ChunkType = ChunkInfo.Id.GetChunkType();
    ChunkInfo.Hash = FIoChunkHash::CreateFromIoHash(Meta.ChunkHash);
    ChunkInfo.ChunkHash = Meta.ChunkHash;
    ChunkInfo.bHasValidFileName = false;
    ChunkInfo.bIsCompressed = EnumHasAnyFlags(Meta.Flags, FIoStoreTocEntryMetaFlags::Compressed);
    ChunkInfo.bIsMemoryMapped = EnumHasAnyFlags(Meta.Flags, FIoStoreTocEntryMetaFlags::MemoryMapped);
    ChunkInfo.bForceUncompressed = bIsContainerCompressed && !EnumHasAnyFlags(Meta.Flags, FIoStoreTocEntryMetaFlags::Compressed);
    ChunkInfo.Offset = OffsetAndLength.GetOffset();
    ChunkInfo.Size = OffsetAndLength.GetLength();

    const uint64_t CompressionBlockSize = Header.CompressionBlockSize;
    int32_t FirstBlockIndex = int32_t(ChunkInfo.Offset / CompressionBlockSize);
    int32_t LastBlockIndex = int32_t((Align(ChunkInfo.Offset + ChunkInfo.Size, CompressionBlockSize) - 1) / CompressionBlockSize);

    ChunkInfo.NumCompressedBlocks = LastBlockIndex - FirstBlockIndex + 1;
    ChunkInfo.OffsetOnDisk = CompressionBlocks[FirstBlockIndex].GetOffset();
    ChunkInfo.CompressedSize = 0;
    ChunkInfo.PartitionIndex = -1;
    for (int32_t BlockIndex = FirstBlockIndex; BlockIndex <= LastBlockIndex; ++BlockIndex) {
        const FIoStoreTocCompressedBlockEntry& CompressionBlock = CompressionBlocks[BlockIndex];
        ChunkInfo.CompressedSize += CompressionBlock.GetCompressedSize();
        if (ChunkInfo.PartitionIndex < 0) {
            ChunkInfo.PartitionIndex = int32_t(CompressionBlock.GetOffset() / Header.PartitionSize);
        }
    }
    return ChunkInfo;
}

FIoStatus FIoStoreTocResource::Read(const std::string& TocFilePath, EIoStoreTocReadOptions ReadOptions, FIoStoreTocResource& OutTocResource) {
    OutTocResource.TocPath = TocFilePath;
    FFileReaderNoWrite TocFileHandle(TocFilePath.c_str());

    if (!TocFileHandle.IsValid()) {
        return FIoStatusBuilder(EIoErrorCode::FileOpenFailed) << "Failed to open IoStore TOC file '" << TocFilePath << "'";
    }

    // Header
    FIoStoreTocHeader& Header = OutTocResource.Header;
    TocFileHandle.Serialize(&Header, sizeof(FIoStoreTocHeader));

    if (!Header.CheckMagic()) {
        return FIoStatusBuilder(EIoErrorCode::CorruptToc) << "TOC header magic mismatch while reading '" << TocFilePath << "'";
    }

    if (Header.TocHeaderSize != sizeof(FIoStoreTocHeader)) {
        return FIoStatusBuilder(EIoErrorCode::CorruptToc) << "TOC header size mismatch while reading '" << TocFilePath << "'";
    }

    if (Header.TocCompressedBlockEntrySize != sizeof(FIoStoreTocCompressedBlockEntry)) {
        return FIoStatusBuilder(EIoErrorCode::CorruptToc) << "TOC compressed block entry size mismatch while reading '" << TocFilePath << "'";
    }

    if (Header.Version < static_cast<uint8_t>(EIoStoreTocVersion::DirectoryIndex)) {
        return FIoStatusBuilder(EIoErrorCode::CorruptToc) << "Outdated TOC header version while reading '" << TocFilePath << "'";
    }

    if (Header.Version > static_cast<uint8_t>(EIoStoreTocVersion::Latest)) {
        return FIoStatusBuilder(EIoErrorCode::CorruptToc) << "Too new TOC header version while reading '" << TocFilePath << "'";
    }

    const uint64_t TotalTocSize = TocFileHandle.TotalSize() - sizeof(FIoStoreTocHeader);
    const uint64_t TocMetaSize = Header.TocEntryCount * sizeof(FIoStoreTocEntryMeta);

    const uint64_t DefaultTocSize = TotalTocSize - (Header.DirectoryIndexSize + TocMetaSize);
    uint64_t TocSize = DefaultTocSize;

    if (EnumHasAnyFlags(ReadOptions, EIoStoreTocReadOptions::ReadTocMeta)) {
        TocSize = TotalTocSize; // Meta dataa is at the end of the TOC file
    }
    else if (EnumHasAnyFlags(ReadOptions, EIoStoreTocReadOptions::ReadDirectoryIndex)) {
        TocSize = DefaultTocSize + Header.DirectoryIndexSize;
    }

    TUniquePtr<uint8_t[]> TocBuffer = std::make_unique<uint8_t[]>(TocSize);
    TocFileHandle.Serialize(TocBuffer.get(), TocSize);

    FMemoryReader TocMemReader(TocBuffer.get(), TocSize);

    // Chunk IDs
    TocMemReader.BulkSerializeArray(OutTocResource.ChunkIds, Header.TocEntryCount);

    // Chunk offsets
    TocMemReader.BulkSerializeArray(OutTocResource.ChunkOffsetAndLengths, Header.TocEntryCount);

    // Chunk perfect hash map
    uint32_t PerfectHashSeedsCount = 0;
    uint32_t ChunksWithoutPerfectHashCount = 0;
    if (Header.Version >= static_cast<uint8_t>(EIoStoreTocVersion::PerfectHashWithOverflow)) {
        PerfectHashSeedsCount = Header.TocChunkPerfectHashSeedsCount;
        ChunksWithoutPerfectHashCount = Header.TocChunksWithoutPerfectHashCount;
    }
    else if (Header.Version >= static_cast<uint8_t>(EIoStoreTocVersion::PerfectHash)) {
        PerfectHashSeedsCount = Header.TocChunkPerfectHashSeedsCount;
    }

    if (PerfectHashSeedsCount) {
        TocMemReader.BulkSerializeArray(OutTocResource.ChunkPerfectHashSeeds, PerfectHashSeedsCount);
    }
    if (ChunksWithoutPerfectHashCount) {
        TocMemReader.BulkSerializeArray(OutTocResource.ChunkIndicesWithoutPerfectHash, ChunksWithoutPerfectHashCount);
    }

    // Compression blocks
    TocMemReader.BulkSerializeArray(OutTocResource.CompressionBlocks, Header.TocCompressedBlockEntryCount);

    OutTocResource.CompressionMethods.reserve(Header.CompressionMethodNameCount + 1);
    OutTocResource.CompressionMethods.push_back({});

    for (uint32_t CompressionNameIndex = 0; CompressionNameIndex < Header.CompressionMethodNameCount; CompressionNameIndex++) {
        const char* AnsiCompressionMethodName = reinterpret_cast<const char*>(TocMemReader.GetBufferCur()) + CompressionNameIndex * Header.CompressionMethodNameLength;
        OutTocResource.CompressionMethods.push_back(AnsiCompressionMethodName);
    }
    TocMemReader.SeekCur(Header.CompressionMethodNameCount * Header.CompressionMethodNameLength);

    // Chunk block signatures
    const bool bIsSigned = EnumHasAnyFlags(Header.ContainerFlags, EIoContainerFlags::Signed);
    if (bIsSigned) {
        uint32_t HashSize;
        TocMemReader << HashSize;

        std::vector<uint8_t> TocSignature;
        std::vector<uint8_t> BlockSignature;

        TocMemReader.BulkSerializeArray(TocSignature, HashSize);
        TocMemReader.BulkSerializeArray(BlockSignature, HashSize);

        std::vector<FSHAHash> ChunkBlockSignatures;
        TocMemReader.BulkSerializeArray(ChunkBlockSignatures, Header.TocCompressedBlockEntryCount);

        OutTocResource.ChunkBlockSignatures = ChunkBlockSignatures;
    }

    if (EnumHasAnyFlags(ReadOptions, EIoStoreTocReadOptions::ReadDirectoryIndex) &&
        EnumHasAnyFlags(Header.ContainerFlags, EIoContainerFlags::Indexed) &&
        Header.DirectoryIndexSize > 0) {
            const uint8_t* Buf = TocMemReader.GetBufferCur();

            OutTocResource.DirectoryIndexBuffer = std::vector<uint8_t>(Buf, Buf + Header.DirectoryIndexSize);
    }

    TocMemReader.SeekCur(Header.DirectoryIndexSize);
    if (EnumHasAnyFlags(ReadOptions, EIoStoreTocReadOptions::ReadTocMeta)) {
        const uint8_t* TocMeta = (uint8_t*)TocMemReader.GetBufferCur();

        if (Header.Version >= static_cast<uint8_t>(EIoStoreTocVersion::ReplaceIoChunkHashWithIoHash)) {
            const FIoStoreTocEntryMeta* ChunkMetas = reinterpret_cast<const FIoStoreTocEntryMeta*>(TocMeta);
            OutTocResource.ChunkMetas = std::vector<FIoStoreTocEntryMeta>(ChunkMetas, ChunkMetas + Header.TocEntryCount);
        }
        else {
            struct FIoStoreTocEntryMetaOld {
                uint8_t ChunkHash[32];
                FIoStoreTocEntryMetaFlags Flags;
            };

            const FIoStoreTocEntryMetaOld* ChunkMetas = reinterpret_cast<const FIoStoreTocEntryMetaOld*>(TocMeta);
            std::vector<FIoStoreTocEntryMetaOld> OldChunkMetas = std::vector<FIoStoreTocEntryMetaOld>(ChunkMetas, ChunkMetas + Header.TocEntryCount);
            OutTocResource.ChunkMetas.reserve(OldChunkMetas.size());
            for (const FIoStoreTocEntryMetaOld& OldChunkMeta : OldChunkMetas) {
                FIoStoreTocEntryMeta ChunkMeta;
                
                memcpy(ChunkMeta.ChunkHash.GetBytes(), &OldChunkMeta.ChunkHash, sizeof(ChunkMeta.ChunkHash));
                ChunkMeta.Flags = OldChunkMeta.Flags;

                OutTocResource.ChunkMetas.emplace_back(ChunkMeta);
            }
        }
    }

    if (Header.Version < static_cast<uint8_t>(EIoStoreTocVersion::PartitionSize)) {
        Header.PartitionCount = 1;
        Header.PartitionSize = UINT64_MAX;
    }

    return FIoStatus::Ok;
}