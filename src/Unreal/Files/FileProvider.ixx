module;

#include "Saturn/Defines.h"

export module Saturn.Files.FileProvider;

import <string>;
import <vector>;
import <mutex>;

import Saturn.Structs.Guid;
import Saturn.Encryption.AES;

import Saturn.IoStore.IoStoreReader;

export class FFileProvider {
public:
    FFileProvider(const std::string& PakDirectory);
    
    void SubmitKey(FGuid& Guid, FAESKey& Key);
    void SubmitKeys(TMap<FGuid, FAESKey>& DecryptionKeys);

    void MountAsync();
    void Mount();
    void Unmount();
private:
    TMap<FGuid, FAESKey> DecryptionKeys;
    std::vector<std::string> ArchivePaths;
    std::vector<FIoStoreReader*> TocArchives;
    std::mutex TocArchivesMutex;
};