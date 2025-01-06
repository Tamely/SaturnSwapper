module;

#include "Saturn/Defines.h"

export module Saturn.VFS.FileSystem;

import <string>;
import <vector>;
import <optional>;
import <shared_mutex>;

import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.Structs.IoChunkId;

// A global extension pool to deduplicate extension strings
export class ExtensionPool {
public:
    static uint32_t GetOrAdd(const std::string& extension);
    static const std::string& Get(uint32_t id);

private:
    static inline phmap::flat_hash_map<std::string, uint32_t> s_Pool;
    static inline std::vector<std::string> s_ReverseLookup;
};

export struct FGameFile {
    // Extensions are stored as IDs from ExtensionPool
    std::vector<std::pair<uint32_t, uint32_t>> Extensions; // <ExtensionID, TocEntryIndex>
};

export class VirtualFileSystem {
public:
    void Register(const std::string& Path, uint32_t TocEntryIndex);
    void RegisterBatch(const std::vector<std::pair<std::string, uint32_t>>& Files);
    void RegisterParallel(const std::vector<std::pair<std::string, uint32_t>>& Files);

    void RegisterReader(class FIoStoreReader* Reader);
    void RegisterReaders(std::vector<class FIoStoreReader*>& Readers);

    void Clear();

    void PrintRegisteredFiles();
    std::optional<FGameFile> GetFileByPath(const std::string& Path);
    TIoStatusOr<FIoBuffer> GetBufferByPathAndExtension(const std::string& Path);
    class FIoStoreReader* GetReaderByPathAndExtension(const std::string& Path);

private:
    static std::string GetExtension(const std::string& Path);
    static std::string GetPathWithoutExtension(const std::string& Path);
    static std::string NormalizeFilePath(const std::string& path);

    // Key is xxhashed normalized path
    TMap<uint64_t, FGameFile> s_FileMap;
    std::shared_mutex s_VFSMutex;
    std::vector<class FIoStoreReader*> s_Readers;
};