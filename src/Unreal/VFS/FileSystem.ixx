module;

#include "Saturn/Defines.h"

export module Saturn.VFS.FileSystem;

import <string>;
import <vector>;
import <sstream>;
import <mutex>;
import <optional>;
import <filesystem>;
import <shared_mutex>;

import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.Structs.IoChunkId;

export struct FGameFile {
    std::string Path;
    // Extension | <TocEntryIndex, Reader>
    TMap<std::string, uint32_t> Extensions;
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
    static std::string GetExtension(const std::string& Path) {
        return std::filesystem::path(Path).extension().string();
    }

    static std::string GetPathWithoutExtension(const std::string& Path) {
        std::filesystem::path fsPath(Path);
        fsPath.make_preferred();
        return NormalizeFilePath(fsPath.parent_path().string() + "\\" + fsPath.stem().string());
    }

    static std::string NormalizeFilePath(const std::string& path) {
        // Step 1: Replace all backslashes with forward slashes
        std::string normalizedPath = path;
        for (char& ch : normalizedPath) {
            if (ch == '\\') {
                ch = '/';
            }
        }

        // Step 2: Split the path into components
        std::vector<std::string> components;
        std::istringstream stream(normalizedPath);
        std::string segment;
        while (std::getline(stream, segment, '/')) {
            if (segment == "..") {
                if (!components.empty()) {
                    components.pop_back();
                }
            }
            else if (!segment.empty() && segment != ".") {
                components.push_back(segment);
            }
        }

        // Step 3: Reconstruct the normalized path
        std::string result;
        for (const auto& comp : components) {
            if (!result.empty()) {
                result += "/";
            }
            result += comp;
        }

        return result;
    }

    // Key is xxhashed normalized path
    static TMap<uint64_t, FGameFile> s_FileMap;
    static std::shared_mutex s_VFSMutex;
    static std::vector<class FIoStoreReader*> s_Readers;
};