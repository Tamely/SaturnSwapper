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
import Saturn.Core.GlobalContext;
import Saturn.Reflection.Mappings;

import Saturn.Core.IoStatus;
import Saturn.VFS.FileSystem;
import Saturn.IoStore.IoStoreReader;
import Saturn.Readers.ZenPackageReader;

FFileProvider::FFileProvider(const std::string& PakDirectory, const std::string& MappingsFile) {
    VFS = std::make_shared<VirtualFileSystem>();
    Context = std::make_shared<GlobalContext>();
    Mappings::RegisterTypesFromUsmap(MappingsFile, Context->ObjectArray);

    if (!std::filesystem::is_directory(PakDirectory)) {
        LOG_ERROR("Invalid pak directory {0}", PakDirectory);
        return;
    }

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
                    VFS->RegisterParallel(Files);
                    VFS->RegisterReader(reader);

                    if (reader->GetContainerName() == "global") {
                        Context->GlobalToc = std::make_shared<FGlobalTocData>();
                        Context->GlobalToc->Serialize(reader);
                    }

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
            std::vector<std::pair<std::string, uint32_t>> Files;
            reader->GetFiles(Files);
            VFS->RegisterParallel(Files);
            VFS->RegisterReader(reader);

            if (reader->GetContainerName() == "global") {
                Context->GlobalToc = std::make_shared<FGlobalTocData>();
                Context->GlobalToc->Serialize(reader);
            }

            LOG_INFO("Successfully mounted archive: '{0}'", Archive);
            this->TocArchives.emplace_back(reader);
        }
    }
}

void FFileProvider::Unmount() {
    for (FIoStoreReader*& reader : TocArchives) {
        delete reader;
    }
    TocArchives.clear();
    VFS->Clear();
}

UPackagePtr FFileProvider::LoadPackage(const std::string& Path) {
    FExportState State;
    State.LoadTargetOnly = false;

    return LoadPackage(Path, State);
}

UPackagePtr FFileProvider::LoadPackage(const std::string& Path, FExportState& State) {
    std::string AssetPath = Path;
    TIoStatusOr<FIoBuffer> Entry = VFS->GetBufferByPathAndExtension(AssetPath);

    if (!Entry.IsOk()) {
        LOG_ERROR(Entry.Status().ToString());
        return nullptr;
    }

    FIoBuffer Buffer = Entry.ConsumeValueOrDie();
    return LoadPackage(Buffer, State);
}

UPackagePtr FFileProvider::LoadPackage(FIoBuffer& Entry, FExportState& State) {
    FZenPackageReader reader(Entry);
    LOG_INFO("Made reader for {0}", std::string(reader.GetPackageName().begin(), reader.GetPackageName().end()));
    return reader.MakePackage(Context, State);
}
