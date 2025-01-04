module;

#include "Saturn/Defines.h"

export module Saturn.Files.FileProvider;

import <string>;
import <vector>;
import <mutex>;
import <memory>;

import Saturn.Misc.IoBuffer;
import Saturn.Core.UObject;
import Saturn.Structs.Guid;
import Saturn.VFS.FileSystem;
import Saturn.Encryption.AES;
import Saturn.Core.GlobalContext;
import Saturn.Readers.ZenPackageReader;

export class FFileProvider {
public:
    FFileProvider(const std::string& PakDirectory, const std::string& MappingsFile);
    
    void SubmitKey(FGuid& Guid, FAESKey& Key);
    void SubmitKeys(TMap<FGuid, FAESKey>& DecryptionKeys);

    void MountAsync();
    void Mount();
    void Unmount();

    UPackagePtr LoadPackage(const std::string& Path);
    UPackagePtr LoadPackage(const std::string& Path, FExportState& State);
    UPackagePtr LoadPackage(FIoBuffer& Entry, FExportState& State);
public:
    std::vector<class FIoStoreReader*>& GetArchives() { return TocArchives; }
private:
    TMap<FGuid, FAESKey> DecryptionKeys;
    std::vector<std::string> ArchivePaths;
    std::vector<class FIoStoreReader*> TocArchives;
    std::mutex TocArchivesMutex;
    TSharedPtr<GlobalContext> Context;
    TSharedPtr<VirtualFileSystem> VFS;
};