import Saturn.VFS.FileSystem;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

import <cstdint>;
import <string>;
import <vector>;
import <mutex>;
import <optional>;
import <filesystem>;
import <shared_mutex>;

TMap<std::string, FGameFile> VirtualFileSystem::s_FileMap;
std::shared_mutex VirtualFileSystem::s_VFSMutex;

void VirtualFileSystem::Register(const std::string& Path, uint32_t TocEntryIndex) {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);

    std::string pathWithoutExtension = GetPathWithoutExtension(Path);
    std::string extension = GetExtension(Path);

    auto it = s_FileMap.find(pathWithoutExtension);
    if (it != s_FileMap.end()) {
        it->second.Extensions[extension] = TocEntryIndex;
    }
    else {
        FGameFile file = { pathWithoutExtension, {{ extension, TocEntryIndex }} };
        s_FileMap[pathWithoutExtension] = file;
    }
}

void VirtualFileSystem::RegisterBatch(const std::vector<std::pair<std::string, uint32_t>>& Files) {
    TMap<std::string, FGameFile> localFileMap;

    // Build the file map locally without locking
    for (const auto& [Path, TocEntryIndex] : Files) {
        std::string pathWithoutExtension = GetPathWithoutExtension(Path);
        std::string extension = GetExtension(Path);

        auto& file = localFileMap[pathWithoutExtension];
        if (file.Path.empty()) {
            file.Path = pathWithoutExtension;
        }
        file.Extensions[extension] = TocEntryIndex;
    }

    // Merge local changes inot the main map with a single lock
    {
        std::unique_lock<std::shared_mutex> lock(s_VFSMutex);
        for (const auto& [key, localFile] : localFileMap) {
            auto& globalFile = s_FileMap[key];
            if (globalFile.Path.empty()) {
                globalFile.Path = localFile.Path;
            }
            for (const auto& [ext, tocIdx] : localFile.Extensions) {
                globalFile.Extensions[ext] = tocIdx;
            }
        }
    }
}

std::optional<FGameFile> VirtualFileSystem::GetFileByPath(const std::string& Path) {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);

    auto it = s_FileMap.find(GetPathWithoutExtension(Path));
    if (it != s_FileMap.end()) {
        return it->second;
    }
    return std::nullopt;
}

std::string VectorToString(const std::vector<std::string>& Vec) {
    std::string Out;
    for (const auto& ext : Vec) {
        Out.append(ext + " ");
    }
    return Out;
}

std::string TMapToString(const TMap<std::string, uint32_t>& Map) {
    std::string Out;
    for (const auto& kvp : Map) {
        Out.append("(");
        Out.append(kvp.first);
        Out.append("[");
        Out.append(std::to_string(kvp.second));
        Out.append("]) ");
    }
    return Out;
}

void VirtualFileSystem::PrintRegisteredFiles() {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);

    for (const auto& [path, file] : s_FileMap) {
        LOG_INFO("Path: {0}, Extensions: [ {1}]", file.Path, TMapToString(file.Extensions));
    }
}