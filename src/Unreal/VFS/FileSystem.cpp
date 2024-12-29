import Saturn.VFS.FileSystem;

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

import <cstdint>;
import <string>;
import <vector>;
import <mutex>;
import <future>;
import <optional>;
import <functional>;
import <filesystem>;
import <shared_mutex>;

TMap<std::string, FGameFile> VirtualFileSystem::s_FileMap;
std::shared_mutex VirtualFileSystem::s_VFSMutex;

void VirtualFileSystem::Register(const std::string& Path, uint32_t TocEntryIndex, class FIoStoreReader* Reader) {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);

    std::string pathWithoutExtension = GetPathWithoutExtension(Path);
    std::string extension = GetExtension(Path);

    auto it = s_FileMap.find(pathWithoutExtension);
    if (it != s_FileMap.end()) {
        it->second.Extensions[extension] = { TocEntryIndex, Reader };
    }
    else {
        FGameFile file = { pathWithoutExtension, {{ extension, { TocEntryIndex, Reader } }} };
        s_FileMap[pathWithoutExtension] = file;
    }
}

void VirtualFileSystem::RegisterBatch(const std::vector<std::pair<std::string, std::pair<uint32_t, class FIoStoreReader*>>>& Files) {
    TMap<std::string, FGameFile> localFileMap;

    // Build the file map locally without locking
    for (const auto& File : Files) {
        const auto& Path = File.first;
        const auto& TocEntryIndex = File.second.first;
        const auto& Reader = File.second.second;

        std::string pathWithoutExtension = GetPathWithoutExtension(Path);
        std::string extension = GetExtension(Path);

        auto& file = localFileMap[pathWithoutExtension];
        if (file.Path.empty()) {
            file.Path = pathWithoutExtension;
        }
        file.Extensions[extension] = { TocEntryIndex, Reader };
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

void VirtualFileSystem::RegisterParallel(const std::vector<std::pair<std::string, std::pair<uint32_t, class FIoStoreReader*>>>& Files) {
    const size_t numThreads = std::thread::hardware_concurrency();
    const size_t chunkSize = Files.size() / numThreads;

    std::vector<std::future<TMap<std::string, FGameFile>>> futures;

    // Divide the files into chukns and process them in parallel
    for (size_t i = 0; i < numThreads; ++i) {
        size_t startIdx = i * chunkSize;
        size_t endIdx = (i == numThreads - 1) ? Files.size() : (i + 1) * chunkSize;

        futures.emplace_back(std::async(std::launch::async, [startIdx, endIdx, &Files]() {
            TMap<std::string, FGameFile> localFileMap;
            for (size_t j = startIdx; j < endIdx; ++j) {
                const auto& File = Files[j];
                const auto& Path = File.first;
                const auto& TocEntryIndex = File.second.first;
                const auto& Reader = File.second.second;

                std::string pathWithoutExtension = GetPathWithoutExtension(Path);
                std::string extension = GetExtension(Path);

                auto& file = localFileMap[pathWithoutExtension];
                if (file.Path.empty()) {
                    file.Path = pathWithoutExtension;
                }
                file.Extensions[extension] = { TocEntryIndex, Reader };
            }
            return localFileMap;
        }));
    }

    // Merge the results into the global file map
    for (auto& future : futures) {
        TMap<std::string, FGameFile> localFileMap = future.get();
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

void VirtualFileSystem::Clear() {
    s_FileMap.clear();
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

std::string TMapToString(const TMap<std::string, std::pair<uint32_t, class FIoStoreReader*>>& Map) {
    std::string Out;
    for (const auto& kvp : Map) {
        Out.append("(");
        Out.append(kvp.first);
        Out.append("[");
        Out.append(std::to_string(kvp.second.first));
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