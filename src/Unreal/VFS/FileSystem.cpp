import Saturn.VFS.FileSystem;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

import <string>;
import <vector>;
import <mutex>;
import <optional>;
import <filesystem>;

TMap<std::string, FGameFile> VirtualFileSystem::s_FileMap;
std::mutex VirtualFileSystem::s_VFSMutex;

void VirtualFileSystem::Register(const std::string& Path) {
    std::lock_guard<std::mutex> lock(s_VFSMutex);

    std::string pathWithoutExtension = GetPathWithoutExtension(Path);
    std::string extension = GetExtension(Path);

    auto it = s_FileMap.find(pathWithoutExtension);
    if (it != s_FileMap.end()) {
        if (!extension.empty() &&
            std::find(it->second.Extensions.begin(), it->second.Extensions.end(), extension) == it->second.Extensions.end()) {
            it->second.Extensions.push_back(extension);
        }
    }
    else {
        FGameFile file = { pathWithoutExtension, { extension } };
        s_FileMap[pathWithoutExtension] = file;
    }
}

std::optional<FGameFile> VirtualFileSystem::GetFileByPath(const std::string& Path) {
    std::lock_guard<std::mutex> lock(s_VFSMutex);

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

void VirtualFileSystem::PrintRegisteredFiles() {
    std::lock_guard<std::mutex> lock(s_VFSMutex);

    for (const auto& [path, file] : s_FileMap) {
        LOG_INFO("Path: {0}, Extensions: [ {1}]", file.Path, VectorToString(file.Extensions));
    }
}