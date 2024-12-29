#include "Saturn/Defines.h"
#include "Saturn/Log.h"

import Saturn.Files.FileProvider;

import <iostream>;

import <vector>;
import <future>;
import <filesystem>;

import Saturn.Core.ThreadPool;

import Saturn.Structs.Guid;
import Saturn.Encryption.AES;

import Saturn.Core.IoStatus;
import Saturn.VFS.FileSystem;
import Saturn.IoStore.IoStoreReader;

FFileProvider::FFileProvider(const std::string& PakDirectory) {
    std::error_code Code;
    {
        for (auto& File : std::filesystem::directory_iterator(PakDirectory, Code)) {
            if (File.path().extension() == ".utoc") {
                auto TocPath = File.path();
                this->ArchivePaths.push_back(std::move(TocPath.replace_extension("").string()));
            }
        }
    }
}

void FFileProvider::SubmitKey(FGuid& Guid, FAESKey& Key) {
    this->DecryptionKeys.insert({ Guid, Key });
}

void FFileProvider::SubmitKeys(TMap<FGuid, FAESKey>& DecryptionKeys) {
    for (auto& kvp : DecryptionKeys) {
        this->DecryptionKeys.insert(kvp);
    }
}

void FFileProvider::MountAsync() {
    ThreadPool pool(std::thread::hardware_concurrency());
    std::vector<std::future<void>> futures;

    for (const auto& Archive : ArchivePaths) {
        futures.emplace_back(std::async(std::launch::async, [this, &pool, Archive]() {
            pool.enqueue([this, Archive]() {
                FIoStoreReader* reader = new FIoStoreReader();
                FIoStatus status = reader->Initialize(Archive, this->DecryptionKeys);
                if (!status.IsOk()) {
                    LOG_WARN("Error: [{0}] while reading archive: '{1}'", status.ToString(), Archive);
                    delete reader;
                }
                else {
                    std::vector<std::pair<std::string, uint32_t>> Files;
                    reader->GetFiles(Files);
                    VirtualFileSystem::RegisterParallel(Files);

                    LOG_INFO("Successfully mounted archive: '{0}'", Archive);
                    std::lock_guard<std::mutex> lock(this->TocArchivesMutex);
                    this->TocArchives.emplace_back(reader);
                }
                });
            }));
    }

    for (auto& future : futures) {
        future.get();
    }
}

void FFileProvider::Mount() {
    for (auto Archive : ArchivePaths) {
        FIoStoreReader* reader = new FIoStoreReader();
        FIoStatus status = reader->Initialize(Archive, this->DecryptionKeys);
        if (!status.IsOk()) {
            LOG_WARN("Error: [{0}] while reading archive: '{1}'", status.ToString(), Archive);
        }
        else {
            LOG_INFO("Successfully mounted archive: '{0}'", Archive);
            this->TocArchives.emplace_back(reader);
        }
    }
}

void FFileProvider::Unmount() {
    for (FIoStoreReader*& reader : this->TocArchives) {
        delete reader;
    }
    this->TocArchives.clear();

    VirtualFileSystem::Clear();
}
