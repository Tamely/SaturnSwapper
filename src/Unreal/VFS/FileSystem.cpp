import Saturn.VFS.FileSystem;

#include <xxhash/xxhash.h>

#include "Saturn/Log.h"
#include "Saturn/Defines.h"

import <cstdint>;
import <string>;
import <vector>;
import <mutex>;
import <future>;
import <sstream>;
import <optional>;
import <functional>;
import <filesystem>;
import <shared_mutex>;

import Saturn.Core.IoStatus;
import Saturn.Misc.IoBuffer;
import Saturn.IoStore.IoStoreReader;
import Saturn.Structs.IoStoreTocChunkInfo;

// Global storage for the extension pool
uint32_t ExtensionPool::GetOrAdd(const std::string& extension) {
    auto it = s_Pool.find(extension);
    if (it != s_Pool.end())
        return it->second;

    uint32_t id = static_cast<uint32_t>(s_ReverseLookup.size());
    s_ReverseLookup.push_back(extension);
    s_Pool.insert_or_assign(extension, id);
    return id;
}

const std::string& ExtensionPool::Get(uint32_t id) {
    return s_ReverseLookup[id];
}

void VirtualFileSystem::Register(const std::string& Path, uint32_t TocEntryIndex) {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);

    std::string pathWithoutExtension = GetPathWithoutExtension(Path);
    uint64_t hashedPath = XXH3_64bits(pathWithoutExtension.c_str(), pathWithoutExtension.size());
    uint32_t extensionId = ExtensionPool::GetOrAdd(GetExtension(Path));

    auto& file = s_FileMap[hashedPath];
    file.Extensions.emplace_back(extensionId, TocEntryIndex);
}

void VirtualFileSystem::RegisterBatch(const std::vector<std::pair<std::string, uint32_t>>& Files) {
    phmap::flat_hash_map<uint64_t, FGameFile> localFileMap;

    for (const auto& [Path, TocEntryIndex] : Files) {
        std::string pathWithoutExtension = GetPathWithoutExtension(Path);
        uint64_t hashedPath = XXH3_64bits(pathWithoutExtension.c_str(), pathWithoutExtension.size());
        uint32_t extensionId = ExtensionPool::GetOrAdd(GetExtension(Path));

        auto& file = localFileMap[hashedPath];
        file.Extensions.emplace_back(extensionId, TocEntryIndex);
    }

    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);
    for (const auto& [key, localFile] : localFileMap) {
        auto& globalFile = s_FileMap[key];
        globalFile.Extensions.insert(
            globalFile.Extensions.end(),
            localFile.Extensions.begin(),
            localFile.Extensions.end()
        );
    }
}

void VirtualFileSystem::RegisterParallel(const std::vector<std::pair<std::string, uint32_t>>& Files) {
    const size_t numThreads = std::thread::hardware_concurrency();
    const size_t chunkSize = Files.size() / numThreads;

    std::vector<std::future<phmap::flat_hash_map<uint64_t, FGameFile>>> futures;

    // Divide the files into chunks and process them in parallel
    for (size_t i = 0; i < numThreads; ++i) {
        size_t startIdx = i * chunkSize;
        size_t endIdx = (i == numThreads - 1) ? Files.size() : (i + 1) * chunkSize;

        futures.emplace_back(std::async(std::launch::async, [startIdx, endIdx, &Files]() {
            phmap::flat_hash_map<uint64_t, FGameFile> localFileMap;
            for (size_t j = startIdx; j < endIdx; ++j) {
                const auto& [Path, TocEntryIndex] = Files[j];

                std::string pathWithoutExtension = GetPathWithoutExtension(Path);
                uint64_t hashedPath = XXH3_64bits(pathWithoutExtension.c_str(), pathWithoutExtension.size());
                uint32_t extensionId = ExtensionPool::GetOrAdd(GetExtension(Path));

                auto& file = localFileMap[hashedPath];
                file.Extensions.emplace_back(extensionId, TocEntryIndex);
            }
            return localFileMap;
        }));
    }

    // Merge the results into the global file map
    for (auto& future : futures) {
        phmap::flat_hash_map<uint64_t, FGameFile> localFileMap = future.get();
        std::unique_lock<std::shared_mutex> lock(s_VFSMutex);
        for (const auto& [key, localFile] : localFileMap) {
            auto& globalFile = s_FileMap[key];
            globalFile.Extensions.insert(
                globalFile.Extensions.end(),
                localFile.Extensions.begin(),
                localFile.Extensions.end()
            );
        }
    }
}

void VirtualFileSystem::RegisterReader(FIoStoreReader* Reader) {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);
    s_Readers.push_back(Reader);
}

void VirtualFileSystem::RegisterReaders(std::vector<FIoStoreReader*>& Readers) {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);
    for (auto& Reader : Readers) {
        s_Readers.push_back(Reader);
    }
}

void VirtualFileSystem::Clear() {
    std::unique_lock<std::shared_mutex> lock(s_VFSMutex);
    s_FileMap.clear();
    s_Readers.clear();
}

std::optional<FGameFile> VirtualFileSystem::GetFileByPath(const std::string& Path) {
    std::shared_lock<std::shared_mutex> lock(s_VFSMutex);

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
    uint32_t extensionId = ExtensionPool::GetOrAdd(GetExtension(Path));

    auto it = std::find_if(file.Extensions.begin(), file.Extensions.end(),
        [extensionId](const auto& pair) { return pair.first == extensionId; });

    if (it == file.Extensions.end()) {
        LOG_ERROR("File '{0}' has not been registered with extension '{1}!", Path, ExtensionPool::Get(extensionId));
        return FIoStatus(EIoErrorCode::NotFound, "Provided file not registered with extension.");
    }

    const uint32_t TocEntryIndex = it->second;
    for (auto& Reader : s_Readers) {
        TIoStatusOr<FIoStoreTocChunkInfo> chunkStatus = Reader->GetChunkInfo(TocEntryIndex);
        if (!chunkStatus.IsOk()) continue;

        FIoStoreTocChunkInfo chunkInfo = chunkStatus.ConsumeValueOrDie();
        if (NormalizeFilePath(chunkInfo.FileName) != NormalizeFilePath(Path)) continue;

        return Reader->Read(chunkInfo.Id, FIoReadOptions(0, chunkInfo.Size));
    }
    LOG_ERROR("File '{0}' does not exist in registered readers!", Path);
    return FIoStatus(EIoErrorCode::NotFound, "Provided file does not exist in registered readers.");
}

FIoStoreReader* VirtualFileSystem::GetReaderByPathAndExtension(const std::string& Path) {
    std::optional<FGameFile> fileStatus = VirtualFileSystem::GetFileByPath(Path);
    if (!fileStatus.has_value()) {
        LOG_ERROR("File '{0}' has not been registered!", Path);
        return nullptr;
    }

    FGameFile& file = fileStatus.value();
    uint32_t extensionId = ExtensionPool::GetOrAdd(GetExtension(Path));

    auto it = std::find_if(file.Extensions.begin(), file.Extensions.end(),
        [extensionId](const auto& pair) { return pair.first == extensionId; });

    if (it == file.Extensions.end()) {
        LOG_ERROR("File '{0}' has not been registered with extension '{1}!", Path, ExtensionPool::Get(extensionId));
        return nullptr;
    }

    const uint32_t TocEntryIndex = it->second;
    for (auto& Reader : s_Readers) {
        TIoStatusOr<FIoStoreTocChunkInfo> chunkStatus = Reader->GetChunkInfo(TocEntryIndex);
        if (!chunkStatus.IsOk()) continue;

        FIoStoreTocChunkInfo chunkInfo = chunkStatus.ConsumeValueOrDie();
        if (NormalizeFilePath(chunkInfo.FileName) != NormalizeFilePath(Path)) continue;

        return Reader;
    }
    LOG_ERROR("File '{0}' does not exist in registered readers!", Path);
    return nullptr;
}

std::string VirtualFileSystem::GetExtension(const std::string& Path) {
    return std::filesystem::path(Path).extension().string();
}

std::string VirtualFileSystem::GetPathWithoutExtension(const std::string& Path) {
    std::filesystem::path fsPath(Path);
    fsPath.make_preferred();
    return NormalizeFilePath(fsPath.parent_path().string() + "\\" + fsPath.stem().string());
}

static const std::string InternalGameName = "fortnitegame";
std::string VirtualFileSystem::NormalizeFilePath(const std::string& path) {
    std::string normalizedPath = path;
    std::replace(normalizedPath.begin(), normalizedPath.end(), '\\', '/');

    if (!normalizedPath.empty() && normalizedPath[0] == '/') {
        normalizedPath = normalizedPath.substr(1);
    }

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

    std::string result;
    for (const auto& comp : components) {
        if (!result.empty()) {
            result += "/";
        }
        result += comp;
    }

    std::string lastPart = result.substr(result.find_last_of("/") + 1);
    if (lastPart.find('.') != std::string::npos && lastPart.substr(0, lastPart.find('.')) == lastPart.substr(lastPart.find('.') + 1)) {
        result = result.substr(0, result.find_last_of("/")) + "/" + lastPart.substr(0, lastPart.find('.'));
    }

    std::transform(result.begin(), result.end(), result.begin(), ::tolower);

    std::string root = result.substr(0, result.find('/'));
    std::string tree = result.substr(result.find('/') + 1);

    if (root == "game" || root == "engine") {
        std::string gameName = (root == "engine") ? "engine" : InternalGameName;
        std::string root2 = tree.substr(0, tree.find('/'));
        if (root2 == "config" || root2 == "content" || root2 == "plugins") {
            result = gameName + "/" + tree;
        }
        else {
            result = gameName + "/content/" + tree;
        }
    }
    else if (root == InternalGameName) {
        // Everything is fine, no changes needed
    }
    else if (InternalGameName == "fortnitegame") {
        result = InternalGameName + "/plugins/gamefeatures/" + root + "/content/" + tree;
    }

    return result;
}

void VirtualFileSystem::PrintRegisteredFiles() {
    std::shared_lock<std::shared_mutex> lock(s_VFSMutex);

    for (const auto& [path, file] : s_FileMap) {
        std::string extensions;
        for (const auto& [extId, tocIdx] : file.Extensions) {
            extensions.append("(" + ExtensionPool::Get(extId) + "[" + std::to_string(tocIdx) + "]) ");
        }
        LOG_INFO("Path: {0}, Extensions: [ {1}]", path, extensions);
    }
}
