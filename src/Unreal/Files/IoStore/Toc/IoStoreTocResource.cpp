#include "Saturn/Defines.h"

import Saturn.Structs.IoStoreTocResource;

import <string>;
import <vector>;
import <memory>;

import Saturn.Core.IoStatus;
import Saturn.Readers.FileReader;
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

        OutTocResource.TocSignature = TocSignature;
        OutTocResource.BlockSignature = BlockSignature;

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

FIoStatus FIoStoreTocResource::Write(const std::string& TocFilePath, FIoStoreTocResource& TocResource, uint32_t CompressionBlockSize, uint64_t MaxPartitionSize, const FIoContainerSettings& ContainerSettings, uint64_t& OutSize) {
    FFileReader TocFileHandle(TocFilePath.c_str());

    if (!TocFileHandle.IsValid()) {
        return FIoStatusBuilder(EIoErrorCode::FileOpenFailed) << "Failed to open IoStore TOC file '" << TocFilePath << "'";
    }

    if (TocResource.ChunkIds.size() != TocResource.ChunkOffsetAndLengths.size()) {
        return FIoStatus(EIoErrorCode::InvalidParameter, "Number of TOC chunk IDs doesn't match the number of offsets");
    }

    if (TocResource.ChunkIds.size() != TocResource.ChunkMetas.size()) {
        return FIoStatus(EIoErrorCode::InvalidParameter, "Number of TOC chunk IDs doesn't match the number of chunk meta data");
    }

    bool bHasExplicitCompressionMethodNone = false;
    for (int32_t CompressionMethodIndex = 0; CompressionMethodIndex < TocResource.CompressionMethods.size(); ++CompressionMethodIndex) {
        if (TocResource.CompressionMethods[CompressionMethodIndex].contains("None")) {
            if (CompressionMethodIndex != 0) {
                return FIoStatus(EIoErrorCode::InvalidParameter, "Compression method None must be the first compression method");
            }
            bHasExplicitCompressionMethodNone = true;
        }
    }

    memset(&TocResource.Header, 0, sizeof(FIoStoreTocHeader));

    FIoStoreTocHeader& TocHeader = TocResource.Header;
    TocHeader.MakeMagic();
    TocHeader.Version = static_cast<uint8_t>(EIoStoreTocVersion::Latest);
    TocHeader.TocHeaderSize = sizeof(TocHeader);
    TocHeader.TocEntryCount = TocResource.ChunkIds.size();
    TocHeader.TocChunkPerfectHashSeedsCount = TocResource.ChunkPerfectHashSeeds.size();
    TocHeader.TocChunksWithoutPerfectHashCount = TocResource.ChunkIndicesWithoutPerfectHash.size();
    TocHeader.TocCompressedBlockEntryCount = TocResource.CompressionBlocks.size();
    TocHeader.TocCompressedBlockEntrySize = sizeof(FIoStoreTocCompressedBlockEntry);
    TocHeader.CompressionBlockSize = CompressionBlockSize;
    TocHeader.CompressionMethodNameCount = TocResource.CompressionMethods.size() - (bHasExplicitCompressionMethodNone ? 1 : 0);
    TocHeader.CompressionMethodNameLength = FIoStoreTocResource::CompressionMethodNameLen;
    TocHeader.DirectoryIndexSize = TocResource.DirectoryIndexBuffer.size();
    TocHeader.ContainerId = ContainerSettings.ContainerId;
    TocHeader.EncryptionKeyGuid = ContainerSettings.EncryptionKeyGuid;
    TocHeader.ContainerFlags = ContainerSettings.ContainerFlags;
    if (TocHeader.TocEntryCount == 0) {
        TocHeader.PartitionCount = 0;
        TocHeader.PartitionSize = UINT64_MAX;
    }
    else if (MaxPartitionSize) {
        const FIoStoreTocCompressedBlockEntry& LastBlock = TocResource.CompressionBlocks[TocResource.CompressionBlocks.size() - 1];
        uint64_t LastBlockEnd = LastBlock.GetOffset() + LastBlock.GetCompressedSize() - 1;
        TocHeader.PartitionCount = static_cast<int32_t>(LastBlockEnd / MaxPartitionSize + 1);
        TocHeader.PartitionSize = MaxPartitionSize;
    }
    else {
        TocHeader.PartitionCount = 1;
        TocHeader.PartitionSize = UINT64_MAX;
    }

    TocFileHandle.Seek(0);
    OutSize = 0;

    // Header
    TocFileHandle.WriteBuffer(&TocResource.Header, sizeof(FIoStoreTocHeader));
    OutSize += sizeof(FIoStoreTocHeader);
    if (TocFileHandle.Tell() != OutSize) {
        return FIoStatus(EIoErrorCode::WriteError, "Failed to write TOC header");
    }

    // Chunk IDs
    TocFileHandle.BulkWriteArray(TocResource.ChunkIds, TocResource.ChunkIds.size());
    OutSize += sizeof(FIoChunkId) * TocResource.ChunkIds.size();
    if (TocFileHandle.Tell() != OutSize) {
        return FIoStatus(EIoErrorCode::WriteError, "Failed to write chunk ids");
    }

    // Chunk offsets
    TocFileHandle.BulkWriteArray(TocResource.ChunkOffsetAndLengths, TocResource.ChunkOffsetAndLengths.size());
    OutSize += sizeof(FIoOffsetAndLength) * TocResource.ChunkOffsetAndLengths.size();
    if (TocFileHandle.Tell() != OutSize) {
        return FIoStatus(EIoErrorCode::WriteError, "Failed to write chunk offsets");
    }

    // Chunk perfect hash map
    TocFileHandle.BulkWriteArray(TocResource.ChunkPerfectHashSeeds, TocResource.ChunkPerfectHashSeeds.size());
    OutSize += sizeof(int32_t) * TocResource.ChunkPerfectHashSeeds.size();
    if (TocFileHandle.Tell() != OutSize) {
        return FIoStatus(EIoErrorCode::WriteError, "Failed to write chunk hash seeds");
    }
    TocFileHandle.BulkWriteArray(TocResource.ChunkIndicesWithoutPerfectHash, TocResource.ChunkIndicesWithoutPerfectHash.size());
    OutSize += sizeof(int32_t) * TocResource.ChunkIndicesWithoutPerfectHash.size();
    if (TocFileHandle.Tell() != OutSize) {
        return FIoStatus(EIoErrorCode::WriteError, "Failed to write chunk indices without perfect hash");
    }

    // Compression blocks
    TocFileHandle.BulkWriteArray(TocResource.CompressionBlocks, TocResource.CompressionBlocks.size());
    OutSize += sizeof(FIoStoreTocCompressedBlockEntry) * TocResource.CompressionBlocks.size();
    if (TocFileHandle.Tell() != OutSize) {
        return FIoStatus(EIoErrorCode::WriteError, "Failed to write chunk block entries");
    }

    // Compression methods
    char AnsiMethodName[FIoStoreTocResource::CompressionMethodNameLen];

    for (std::string& MethodName : TocResource.CompressionMethods) {
        if (MethodName.contains("None")) continue;

        memset(AnsiMethodName, 0, FIoStoreTocResource::CompressionMethodNameLen);
        strcpy_s(AnsiMethodName, FIoStoreTocResource::CompressionMethodNameLen, MethodName.c_str());

        TocFileHandle.WriteBuffer(AnsiMethodName, FIoStoreTocResource::CompressionMethodNameLen);
        OutSize += FIoStoreTocResource::CompressionMethodNameLen;
        if (TocFileHandle.Tell() != OutSize) {
            return FIoStatus(EIoErrorCode::WriteError, "Failed to write compression method TOC entry");
        }
    }

    // Chunk block signatures
    if (EnumHasAnyFlags(TocHeader.ContainerFlags, EIoContainerFlags::Signed)) {
        const int32_t HashSize = TocResource.TocSignature.size();
        TocFileHandle.WriteBuffer(const_cast<int32_t*>(&HashSize), sizeof(int32_t));
        TocFileHandle.WriteBuffer(TocResource.TocSignature.data(), HashSize);
        TocFileHandle.WriteBuffer(TocResource.BlockSignature.data(), HashSize);

        OutSize += sizeof(int32_t) + HashSize + HashSize;

        TocFileHandle.BulkWriteArray(TocResource.ChunkBlockSignatures, TocResource.ChunkBlockSignatures.size());
        OutSize += sizeof(FSHAHash) + TocResource.ChunkBlockSignatures.size();
        if (TocFileHandle.Tell() != OutSize) {
            return FIoStatus(EIoErrorCode::WriteError, "Failed to write chunk block signatures");
        }
    }

    // Directory index (EIoStoreTocReadOptions::ReadDirectoryIndex)
    if (EnumHasAnyFlags(TocHeader.ContainerFlags, EIoContainerFlags::Indexed)) {
        TocFileHandle.WriteBuffer(TocResource.DirectoryIndexBuffer.data(), TocResource.DirectoryIndexBuffer.size());
        OutSize += sizeof(uint8_t) * TocResource.DirectoryIndexBuffer.size();
        if (TocFileHandle.Tell() != OutSize) {
            return FIoStatus(EIoErrorCode::WriteError, "Failed to write directory index buffer");
        }
    }

    // Meta data (EIoStoreTocReadOptions::ReadTocMeta)
    TocFileHandle.BulkWriteArray(TocResource.ChunkMetas, TocResource.ChunkMetas.size());
    OutSize += sizeof(FIoStoreTocEntryMeta) * TocResource.ChunkMetas.size();
    if (TocFileHandle.Tell() != OutSize) {
        return FIoStatus(EIoErrorCode::WriteError, "Failed to write chunk meta data");
    }

    TocFileHandle.Close();

    return FIoStatus::Ok;
}

uint64_t FIoStoreTocResource::HashChunkIdWithSeed(int32_t Seed, const FIoChunkId& ChunkId) {
    const uint8_t* Data = ChunkId.GetData();
    const uint32_t DataSize = ChunkId.GetSize();
    uint64_t Hash = Seed ? static_cast<uint64_t>(Seed) : 0xcbf29ce484222325;
    for (uint32_t Index = 0; Index < DataSize; ++Index) {
        Hash = (Hash * 0x00000100000001B3) ^ Data[Index];
    }
    return Hash;
}