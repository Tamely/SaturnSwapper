export module Saturn.Unreal.IoStoreReader;

import Saturn.Structs.FileInfo;
import Saturn.Structs.IoChunkId;
import Saturn.Readers.FileReader;
import Saturn.Structs.IoOffsetLength;
import Saturn.Structs.IoFileIndexEntry;
import Saturn.Structs.IoDirectoryIndexEntry;

import <string>;
import <vector>;
import <cstdint>;
import <unordered_map>;
import <array>;
import <tuple>;

import "xxhash/xxhash.h";

export class IoStoreReader {
public:
    __forceinline IoStoreReader(const std::string& filePath) : m_FilePath(filePath), m_Reader(FFileReader(filePath.c_str())) {
        m_Reader.Seek(24);

        m_Reader << TocEntryCount;
        m_Reader << TocCompressedBlockEntryCount;

        m_Reader.SeekCur(4);

        m_Reader << CompressionMethodNameCount;
        m_Reader << CompressionMethodNameLength;

        m_Reader.SeekCur(8);

        m_Reader << PartitionCount;

        m_Reader.Seek(84);

        m_Reader << TocChunkPerfectHashSeedsCount;
        m_Reader << PartitionSize;
        m_Reader << TocChunksWithoutPerfectHashCount;

        m_Reader.Seek(144);

        for (int i = 0; i < TocEntryCount; i++) {
            FIoChunkId id;
            m_Reader << id;
            ChunkIDs.push_back(id);
        }

        ChunkOffsetLengths.resize(TocEntryCount);
        for (int i = 0; i < TocEntryCount; i++) {
            m_Reader << ChunkOffsetLengths[i];
        }

        m_Reader.SeekCur((TocChunkPerfectHashSeedsCount * sizeof(int32_t)) + (TocChunksWithoutPerfectHashCount * sizeof(int32_t)));
        m_CompressionBlockPosition = m_Reader.Tell();
        m_DirectoryIndexPosition = m_Reader.Tell() + TocCompressedBlockEntryCount * 12 + CompressionMethodNameLength * CompressionMethodNameCount
                                   + sizeof(uint32_t) + HASH_SIZE + HASH_SIZE + 20 * TocCompressedBlockEntryCount;

        m_Reader.Seek(m_DirectoryIndexPosition);

        m_Reader << m_MountPoint;
        if (m_MountPoint.ends_with('\0'))
            m_MountPoint.pop_back();

        m_Reader.BulkSerializeArray(m_DirEntries);
        m_Reader.BulkSerializeArray(m_FileEntries);
        m_Reader << m_StringTable;

        ParseDirectoryIndex("", 0);
    }

    void Close() {
        m_Reader.Close();
    }

    __forceinline VFileInfo GetFile(const std::string& fileName) {
        VFileInfo result;

        result.TocEntryIndex = m_Files[XXH3_64bits(fileName.data(), fileName.size())];
        FIoOffsetAndLength entry = ChunkOffsetLengths[result.TocEntryIndex];
        result.FirstBlockIndex = (entry.GetOffset() / COMPRESSION_BLOCK_SIZE);
        result.BlockCount = ((entry.GetLength() - 1) / COMPRESSION_BLOCK_SIZE) + 1;
        result.ChunkId = ChunkIDs[result.TocEntryIndex];

        return result;
    }
private:
    __forceinline void ParseDirectoryIndex(const std::string& Path, uint32_t DirectoryIndexHandle) {
        static constexpr uint32_t InvalidHandle = ~uint32_t(0);

        uint32_t File = m_DirEntries[DirectoryIndexHandle].FirstFileEntry;

        while (File != InvalidHandle)
        {
            auto& FileEntry = m_FileEntries[File];
            auto& FileName = m_StringTable[FileEntry.Name];
            auto FullDir = m_MountPoint + Path;

            if (FileName.ends_with('\0'))
                FileName.pop_back();

            auto FilePath = FullDir + FileName;
            m_Files[XXH3_64bits(FilePath.data(), FilePath.size())] = FileEntry.UserData;

            File = FileEntry.NextFileEntry;
        }

        auto Child = m_DirEntries[DirectoryIndexHandle].FirstChildEntry;

        while (Child != InvalidHandle)
        {
            auto& Entry = m_DirEntries[Child];

            auto ChildDirPath = Path + m_StringTable[Entry.Name];
            ChildDirPath[ChildDirPath.size() - 1] = '/'; // replace null terminator with path divider 

            uint32_t File = m_DirEntries[DirectoryIndexHandle].FirstFileEntry;

            ParseDirectoryIndex(ChildDirPath, Child);

            Child = Entry.NextSiblingEntry;
        }
    }
public:
    // TODO: Read these
    static const uint32_t COMPRESSION_BLOCK_SIZE = 65536;
    static const uint32_t HASH_SIZE = 512;
private:
    // TODO: Make this read from the modulated structs
    uint32_t TocEntryCount;
    uint32_t TocCompressedBlockEntryCount;
    uint32_t CompressionMethodNameCount;
    uint32_t CompressionMethodNameLength;
    uint32_t PartitionCount;
    uint32_t TocChunkPerfectHashSeedsCount;
    uint64_t PartitionSize;
    uint32_t TocChunksWithoutPerfectHashCount;
private:
    std::vector<FIoChunkId> ChunkIDs;
    std::vector<FIoOffsetAndLength> ChunkOffsetLengths;
private:
    std::string m_MountPoint;
    int64_t m_CompressionBlockPosition;
    int64_t m_DirectoryIndexPosition;

    std::unordered_map<uint64_t, uint32_t> m_Files;

    std::string m_FilePath;
    FFileReader m_Reader;

    std::vector<FIoDirectoryIndexEntry> m_DirEntries;
    std::vector<FIoFileIndexEntry> m_FileEntries;
    std::vector<std::string> m_StringTable;

    friend class IoStoreWriter;
};