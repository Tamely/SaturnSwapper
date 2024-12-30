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
    static void Register(const std::string& Path, uint32_t TocEntryIndex);
    static void RegisterBatch(const std::vector<std::pair<std::string, uint32_t>>& Files);
    static void RegisterParallel(const std::vector<std::pair<std::string, uint32_t>>& Files);

    static void RegisterReader(class FIoStoreReader* Reader);
    static void RegisterReaders(std::vector<class FIoStoreReader*>& Readers);

    static void Clear();

    static void PrintRegisteredFiles();
    static std::optional<FGameFile> GetFileByPath(const std::string& Path);
    static TIoStatusOr<FIoBuffer> GetBufferByPathAndExtension(const std::string& Path);

private:
    static std::string GetExtension(const std::string& Path);
    static std::string GetPathWithoutExtension(const std::string& Path);
    static std::string NormalizeFilePath(const std::string& path);

    // Key is xxhashed normalized path
    static TMap<uint64_t, FGameFile> s_FileMap;
    static std::shared_mutex s_VFSMutex;
    static std::vector<class FIoStoreReader*> s_Readers;
};