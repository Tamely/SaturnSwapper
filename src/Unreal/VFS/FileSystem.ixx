module;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

export module Saturn.VFS.FileSystem;

import <string>;
import <vector>;
import <mutex>;
import <optional>;
import <filesystem>;

import Saturn.Structs.IoChunkId;

export struct FGameFile {
    std::string Path;
    std::vector<std::string> Extensions;
};

export class VirtualFileSystem {
public:
    static void Register(const std::string& Path);
    static void PrintRegisteredFiles();
    static std::optional<FGameFile> GetFileByPath(const std::string& Path);
private:
    static std::string GetExtension(const std::string& Path) {
        std::filesystem::path fsPath(Path);
        return fsPath.has_extension() ? fsPath.extension().string() : "";
    }

    static std::string GetPathWithoutExtension(const std::string& Path) {
        std::filesystem::path fsPath(Path);
        std::filesystem::path parentPath = fsPath.parent_path();
        std::string stem = fsPath.stem().string();
        std::string combined = (parentPath / stem).string();

        std::replace(combined.begin(), combined.end(), '\\', '/');
        return combined;
    }

    static TMap<std::string, FGameFile> s_FileMap;
    static std::mutex s_VFSMutex;
};