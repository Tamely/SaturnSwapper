module;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

export module Saturn.VFS.FileSystem;

import <string>;
import <vector>;
import <mutex>;
import <optional>;
import <filesystem>;
import <shared_mutex>;

import Saturn.Structs.IoChunkId;

export struct FGameFile {
    std::string Path;
    // Extension | TocEntryIndex
    TMap<std::string, uint32_t> Extensions;
};

export class VirtualFileSystem {
public:
    static void Register(const std::string& Path, uint32_t TocEntryIndex);
    static void RegisterBatch(const std::vector<std::pair<std::string, uint32_t>>& Files);
    static void RegisterParallel(const std::vector<std::pair<std::string, uint32_t>>& Files);

    static void PrintRegisteredFiles();
    static std::optional<FGameFile> GetFileByPath(const std::string& Path);
private:
    static std::string GetExtension(const std::string& Path) {
        return std::filesystem::path(Path).extension().string();
    }

    static std::string GetPathWithoutExtension(const std::string& Path) {
        std::filesystem::path fsPath(Path);
        fsPath.make_preferred();
        return fsPath.parent_path().string() + "\\" + fsPath.stem().string();
    }

    static TMap<std::string, FGameFile> s_FileMap;
    static std::shared_mutex s_VFSMutex;
};