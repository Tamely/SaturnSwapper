#include "Saturn/Defines.h"

import Saturn.Files.FileProvider;

import <string>;
import <filesystem>;

import <iostream>;

import Saturn.Core.IoStatus;

import Saturn.Structs.Guid;
import Saturn.Encryption.AES;

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