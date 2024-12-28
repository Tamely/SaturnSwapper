#include "Saturn/Defines.h"
#include <thread>
#include <queue>
#include <condition_variable>

import Saturn.Files.FileProvider;

import <iostream>;

import <string>;
import <future>;
import <vector>;
import <mutex>;
import <vector>;
import <functional>;
import <filesystem>;

import Saturn.Core.IoStatus;

import Saturn.Structs.Guid;
import Saturn.Encryption.AES;

import Saturn.IoStore.IoStoreReader;

class ThreadPool {
public:
    ThreadPool(size_t NumThreads) {
        for (size_t i = 0; i < NumThreads; ++i) {
            workers.emplace_back([this]() {
                while (true) {
                    std::function<void()> task;
                    {
                        std::unique_lock<std::mutex> lock(queueMutex);
                        condition.wait(lock, [this]() { return stop || !tasks.empty(); });
                        if (stop && tasks.empty()) return;
                        task = std::move(tasks.front());
                        tasks.pop();
                    }
                    task();
                }
            });
        }
    }

    ~ThreadPool() {
        {
            std::unique_lock<std::mutex> lock(queueMutex);
            stop = true;
        }
        condition.notify_all();
        for (std::thread& worker : workers) {
            worker.join();
        }
    }

    template <class F>
    void enqueue(F&& task) {
        {
            std::unique_lock<std::mutex> lock(queueMutex);
            tasks.emplace(std::forward<F>(task));
        }
        condition.notify_one();
    }
private:
    std::vector<std::thread> workers;
    std::queue<std::function<void()>> tasks;
    std::mutex queueMutex;
    std::condition_variable condition;
    bool stop = false;
};

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
                    std::cout << "Error: [" << status.ToString() << "] when reading archive: '" << Archive << "'";
                    delete reader;
                }
                else {
                    std::cout << "Successfully mounted archive: '" << Archive << "'";
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
            std::cout << "Error: [" << status.ToString() << "] when reading archive: '" << Archive << "'";
        }
        else {
            std::cout << "Successfully mounted archive: '" << Archive << "'";
            this->TocArchives.emplace_back(reader);
        }
    }
}