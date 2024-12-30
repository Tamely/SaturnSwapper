import Saturn.VFS.FileSystem;

#include <xxhash/xxhash.h>

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

import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.IoStore.IoStoreReader;
import Saturn.Structs.IoStoreTocChunkInfo;

TMap<uint64_t, FGameFile> VirtualFileSystem::s_FileMap;
std::shared_mutex VirtualFileSystem::s_VFSMutex;
std::vector<class FIoStoreReader*> VirtualFileSystem::s_Readers;

void VirtualFileSystem::Register(const std::string& Path, uint32_t TocEntryIndex) {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);

    std::string pathWithoutExtension = GetPathWithoutExtension(Path);
    uint64_t hashedPath = XXH3_64bits(pathWithoutExtension.c_str(), pathWithoutExtension.size());
    std::string extension = GetExtension(Path);

    auto it = s_FileMap.find(hashedPath);
    if (it != s_FileMap.end()) {
        it->second.Extensions[extension] = TocEntryIndex;
    }
    else {
        FGameFile file = { pathWithoutExtension, {{ extension, TocEntryIndex }} };
        s_FileMap[hashedPath] = file;
    }
}

void VirtualFileSystem::RegisterBatch(const std::vector<std::pair<std::string, uint32_t>>& Files) {
    TMap<uint64_t, FGameFile> localFileMap;

    // Build the file map locally without locking
    for (const auto& [Path, TocEntryIndex] : Files) {
        std::string pathWithoutExtension = GetPathWithoutExtension(Path);
        uint64_t hashedPath = XXH3_64bits(pathWithoutExtension.c_str(), pathWithoutExtension.size());
        std::string extension = GetExtension(Path);

        auto& file = localFileMap[hashedPath];
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

void VirtualFileSystem::RegisterParallel(const std::vector<std::pair<std::string, uint32_t>>& Files) {
    const size_t numThreads = std::thread::hardware_concurrency();
    const size_t chunkSize = Files.size() / numThreads;

    std::vector<std::future<TMap<uint64_t, FGameFile>>> futures;

    // Divide the files into chukns and process them in parallel
    for (size_t i = 0; i < numThreads; ++i) {
        size_t startIdx = i * chunkSize;
        size_t endIdx = (i == numThreads - 1) ? Files.size() : (i + 1) * chunkSize;

        futures.emplace_back(std::async(std::launch::async, [startIdx, endIdx, &Files]() {
            TMap<uint64_t, FGameFile> localFileMap;
            for (size_t j = startIdx; j < endIdx; ++j) {
                const auto& [Path, TocEntryIndex] = Files[j];

                std::string pathWithoutExtension = GetPathWithoutExtension(Path);
                uint64_t hashedPath = XXH3_64bits(pathWithoutExtension.c_str(), pathWithoutExtension.size());
                std::string extension = GetExtension(Path);

                auto& file = localFileMap[hashedPath];
                if (file.Path.empty()) {
                    file.Path = pathWithoutExtension;
                }
                file.Extensions[extension] = TocEntryIndex;
            }
            return localFileMap;
        }));
    }

    // Merge the results into the global file map
    for (auto& future : futures) {
        TMap<uint64_t, FGameFile> localFileMap = future.get();
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

void VirtualFileSystem::RegisterReader(FIoStoreReader* Reader) {
    s_Readers.push_back(Reader);
}

void VirtualFileSystem::RegisterReaders(std::vector<FIoStoreReader*>& Readers) {
    for (auto& Reader : Readers) {
        RegisterReader(Reader);
    }
}

void VirtualFileSystem::Clear() {
    s_FileMap.clear();
    s_Readers.clear();
}

std::optional<FGameFile> VirtualFileSystem::GetFileByPath(const std::string& Path) {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);

    std::string pathWithoutExtension = GetPathWithoutExtension(Path);
    uint64_t hashedPath = XXH3_64bits(pathWithoutExtension.c_str(), pathWithoutExtension.size());

    auto it = s_FileMap.find(hashedPath);
    if (it != s_FileMap.end()) {
        return it->second;
    }
    return std::nullopt;
}

TIoStatusOr<FIoBuffer> VirtualFileSystem::GetBufferByPathAndExtension(const std::string& Path) {
    std::optional<FGameFile> fileStatus = VirtualFileSystem::GetFileByPath(Path);
    if (!fileStatus.has_value()) {
        LOG_ERROR("File '{0}' has not been registered!", Path);
        return FIoStatus(EIoErrorCode::NotFound, "Provided file not registered.");
    }

    FGameFile& file = fileStatus.value();
    if (file.Extensions.find(GetExtension(Path)) == file.Extensions.end()) {
        LOG_ERROR("File '{0}' has not been registered with extension '{1}!", Path, GetExtension(Path));
        return FIoStatus(EIoErrorCode::NotFound, "Provided file not registered with extension.");
    }

    const uint32_t TocEntryIndex = file.Extensions[GetExtension(Path)];
    for (auto& Reader : s_Readers) {
        TIoStatusOr<FIoStoreTocChunkInfo> chunkStatus = Reader->GetChunkInfo(TocEntryIndex);
        if (!chunkStatus.IsOk()) continue;

        FIoStoreTocChunkInfo chunkInfo = chunkStatus.ConsumeValueOrDie();
        if (NormalizeFilePath(chunkInfo.FileName) != NormalizeFilePath(Path)) {
            LOG_WARN("File paths {0} and {1} did not match.", NormalizeFilePath(chunkInfo.FileName), NormalizeFilePath(Path));
            continue;
        }

        return Reader->Read(chunkInfo.Id, FIoReadOptions(0, chunkInfo.Size));
    }
    LOG_ERROR("File '{0}' does not exist in registered readers!", Path);
    return FIoStatus(EIoErrorCode::NotFound, "Provided file does not exist in registered readers.");
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