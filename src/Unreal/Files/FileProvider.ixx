module;

#include "Saturn/Defines.h"

export module Saturn.Files.FileProvider;

import <string>;
import <vector>;
import <mutex>;
import <memory>;

import Saturn.Structs.Guid;
import Saturn.Encryption.AES;
import Saturn.Core.GlobalContext;

export class FFileProvider {
public:
    FFileProvider(const std::string& PakDirectory, const std::string& MappingsFile);
    
    void SubmitKey(FGuid& Guid, FAESKey& Key);
    void SubmitKeys(TMap<FGuid, FAESKey>& DecryptionKeys);

    void MountAsync();
    void Mount();
    void Unmount();
public:
    std::vector<class FIoStoreReader*>& GetArchives() { return TocArchives; }
private:
    TMap<FGuid, FAESKey> DecryptionKeys;
    std::vector<std::string> ArchivePaths;
    std::vector<class FIoStoreReader*> TocArchives;
    std::mutex TocArchivesMutex;
    TSharedPtr<GlobalContext> Context;
};